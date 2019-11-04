using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TasksDatabase.Models;
using TasksDatabase.ViewModels;

namespace TasksDatabase.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly IConfiguration _config;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public TasksController(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration config)
        {
            _config = config;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Table(int page = 1)
        {
            int PageSize = Convert.ToInt32(_config.GetSection("Pagination").GetSection("TasksPerPage").Value);
            bool AcceptTask = true;

            var CurrentUser = await _userManager.GetUserAsync(User);
            string UserId = CurrentUser.Id;

            using (DbContext db = new DbContext(new DbContextOptions<DbContext>()))
            {
                var allLatestTrackingsList = DatabaseQueries.GetAllLatestTrackings(db, UserId);

                var completedTrackingsList = DatabaseQueries.GetCompletedTrackings(db, UserId);

                int completedCount = completedTrackingsList.Count();

                var allTrackingsList = allLatestTrackingsList.Union(completedTrackingsList)  //все трекинги
                                                             .OrderBy(t => t.Status.Id)
                                                                 .ThenByDescending(t => t.Time)
                                                             .Select(t => t)
                                                             .ToList();

                var totalCount = allTrackingsList.Count();

                foreach (var tracking in allTrackingsList)
                {
                    if (new string[] { "Переносится", "Проверяется", "Дорабатывается", "Утверждается" }.Contains(tracking.Status.Name))
                        AcceptTask = false;
                }

                var items = allTrackingsList.Skip((page - 1) * PageSize)
                                            .Take(PageSize)
                                            .Select(t => new TaskViewModel { Tracking = t })
                                            .ToList();

                PageViewModel pageViewModel = new PageViewModel(totalCount, page, PageSize);

                TableViewModel viewModel = new TableViewModel
                {
                    PageViewModel = pageViewModel,
                    TaskViewModelList = items,
                    CompletedCount = completedCount,
                    TotalCount = totalCount,
                    CanAcceptTask = AcceptTask,
                    UserId = UserId
                };

                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Table(TaskViewModel model)
        {
            var CurrentUser = await _userManager.GetUserAsync(User);
            string UserId = CurrentUser.Id;

            Tracking CurrentTracking, AcceptedTracking;
            string NextStatus = "";
            int NextStatusId;

            string TrackingComment = Request.Form["taskViewModel.Comment"].First().ToString();
            string NextTask = Request.Form["taskViewModel.Tracking.Id"].FirstOrDefault();

            int CurrentTrackingId;
            Int32.TryParse(Request.Form["tracking.Id"].ToString(), out CurrentTrackingId);

            using (DbContext db = new DbContext(new DbContextOptions<DbContext>()))
            {
                var allLatestTrackingsList = db.Trackings.Join(DatabaseQueries.GetLatestTrackings(db, UserId), t => t.Id, l => l, (t, l) => t)
                                                         .Where(t => t.User.Id == UserId)
                                                         .Include(t => t.Status)
                                                         .Include(t => t.Problem)
                                                         .Select(t => t).ToList();

                if (!allLatestTrackingsList.Select(t => t.Id).Contains(CurrentTrackingId))
                {
                    throw new Exception("Неверный id!");
                }

                CurrentTracking = allLatestTrackingsList.Where(t => t.Id == CurrentTrackingId).FirstOrDefault();

                if (CurrentTracking == null)
                {
                    throw new Exception("Трекинга не существует!");
                }

                switch (CurrentTracking.Status.Name)
                {
                    case "Не перенесён":
                        NextStatus = "Переносится";
                        break;
                    case "Не проверен":
                        NextStatus = "Проверяется";
                        break;
                    case "Не доработан":
                        NextStatus = "Дорабатывается";
                        break;
                    case "Не утверждён":
                        NextStatus = (NextTask == "Ok") ? "Утверждён" : "Утверждается";
                        break;
                    case "Переносится":
                        NextStatus = "Перенесён";
                        break;
                    case "Проверяется":
                        NextStatus = "Проверен";
                        break;
                    case "Дорабатывается":
                        NextStatus = "Доработан";
                        break;
                }

                NextStatusId = db.Statuses.Where(s => s.Name == NextStatus).FirstOrDefault().Id;

                AcceptedTracking = new Tracking
                {
                    ProblemId = CurrentTracking.ProblemId,
                    StatusId = NextStatusId,
                    UserId = UserId,
                    Time = DateTime.Now,
                    Comment = TrackingComment
                };

                if (new string[] { "Переносится", "Проверяется", "Дорабатывается", "Утверждается" }.Contains(CurrentTracking.Status.Name)
                    || CurrentTracking.Status.Name == "Не утверждён")
                {
                    AcceptedTracking.StartTime = CurrentTracking.Time;
                }

                db.Add(AcceptedTracking);

                if (CurrentTracking.Status.Name == "Переносится")
                {
                    int ToReviewStatusId = db.Statuses.Where(s => s.Name == "Не проверен").FirstOrDefault().Id;

                    Tracking ToReviewTracking = new Tracking
                    {
                        ProblemId = CurrentTracking.ProblemId,
                        StatusId = ToReviewStatusId,
                        UserId = CurrentTracking.Problem.ReviewerId,
                        Time = AcceptedTracking.Time.AddSeconds(1),
                        Comment = TrackingComment
                    };

                    db.Add(ToReviewTracking);
                }

                if (new string[] { "Проверяется", "Дорабатывается" }.Contains(CurrentTracking.Status.Name))
                {
                    string Status;
                    string NextUserId;

                    if (CurrentTracking.Status.Name == "Проверяется" && NextTask == "Rework")
                    {
                        Status = "Не доработан";
                        NextUserId = CurrentTracking.Problem.DeveloperId;
                    }
                    else
                    {
                        Status = "Не утверждён";
                        NextUserId = db.Users.Where(u => u.UserName == "Admin").FirstOrDefault().Id;
                    }

                    int SecondStatusId = db.Statuses.Where(s => s.Name == Status).FirstOrDefault().Id;

                    Tracking SecondTracking = new Tracking
                    {
                        ProblemId = CurrentTracking.ProblemId,
                        StatusId = SecondStatusId,
                        UserId = NextUserId,
                        Time = AcceptedTracking.Time.AddSeconds(1),
                        Comment = TrackingComment,
                    };

                    db.Add(SecondTracking);
                }

                if (CurrentTracking.Status.Name == "Не утверждён" && (NextTask == "Rework" || NextTask == "Review"))
                {
                    string Status;
                    string NextUserId;

                    if (NextTask == "Rework")
                    {
                        Status = "Не доработан";
                        NextUserId = CurrentTracking.Problem.DeveloperId;
                    }
                    else
                    {
                        Status = "Не проверен";
                        NextUserId = CurrentTracking.Problem.ReviewerId;
                    }

                    int SecondStatusId = db.Statuses.Where(s => s.Name == Status).FirstOrDefault().Id;

                    Tracking SecondTracking = new Tracking
                    {
                        ProblemId = CurrentTracking.ProblemId,
                        StatusId = SecondStatusId,
                        UserId = NextUserId,
                        Time = AcceptedTracking.Time.AddSeconds(1),
                        Comment = TrackingComment,
                    };

                    db.Add(SecondTracking);
                }

                await db.SaveChangesAsync();
            }

            return RedirectToAction("table");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    static class DatabaseQueries
    {
        public static IQueryable<int> GetLatestTrackings(DbContext db, string userId)
        {
            //подзапрос для получения последнего трекинга по каждому заданию
            var latestTracking = db.Trackings.GroupBy(t => t.Problem.Id)
                                             .Select(t => t.Max(t => t.Id));
            return latestTracking;
        }

        public static List<Tracking> GetAllLatestTrackings(DbContext db, string userId)
        {
            //последний трекинг по каждому заданию
            var latestTrackingQuery = GetLatestTrackings(db, userId);

            var allLatestTrackingsList = db.Trackings.Join(latestTrackingQuery, t => t.Id, l => l, (t, l) => t)
                                                     .Where(t => t.User.Id == userId)
                                                     .Include(t => t.Status)
                                                     .Include(t => t.User)
                                                     .Include(t => t.Problem)
                                                         .ThenInclude(t => t.Course)
                                                             .ThenInclude(c => c.Department)
                                                     .Include(t => t.Problem)
                                                         .ThenInclude(t => t.TaskType)
                                                     .Select(t => t).ToList();
            return allLatestTrackingsList;
        }

        public static List<Tracking> GetCompletedTrackings(DbContext db, string userId)
        {
            var completedTrackingsList = db.Trackings.Where(t => t.User.Id == userId && t.StartTime != null)  //трекинги завершенных заданий
                                         .Include(t => t.Status)
                                         .Include(t => t.User)
                                         .Include(t => t.Problem)
                                             .ThenInclude(t => t.Course)
                                                 .ThenInclude(c => c.Department)
                                         .Include(t => t.Problem)
                                             .ThenInclude(t => t.TaskType)
                                         .Select(t => t).ToList();
            return completedTrackingsList;
        }
    }
}