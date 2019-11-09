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
using TasksDatabase.Models;
using TasksDatabase.ViewModels;
using System.Text.Json;

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

                IEnumerable<Tracking> allTrackingsList = allLatestTrackingsList.Union(completedTrackingsList)  //все трекинги
                                                                               .OrderBy(t => t.Status.Id)
                                                                                   .ThenByDescending(t => t.Time)
                                                                               .Select(t => t);

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
            //int NextStatusId;

            string TrackingComment = Request.Form["taskViewModel.Comment"].First().ToString();
            string NextTask = Request.Form["taskViewModel.Tracking.Id"].FirstOrDefault();

            int CurrentTrackingId;
            Int32.TryParse(Request.Form["tracking.Id"].ToString(), out CurrentTrackingId);

            using (DbContext db = new DbContext(new DbContextOptions<DbContext>()))
            {
                IEnumerable<Tracking> allLatestTrackingsList = db.Trackings.Join(DatabaseQueries.GetLatestTrackings(db, UserId),
                                                                                                 t => t.Id, l => l, (t, l) => t)
                                                                           .Where(t => t.User.Id == UserId)
                                                                           .Include(t => t.Status)
                                                                           .Include(t => t.Problem)
                                                                           .Select(t => t);

                if (!allLatestTrackingsList.Select(t => t.Id).Contains(CurrentTrackingId))
                {
                    throw new Exception("Неверный id!");
                }

                CurrentTracking = allLatestTrackingsList.Where(t => t.Id == CurrentTrackingId).FirstOrDefault();

                //валидация входных данных
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

                var NextStatusId = await db.Statuses.Where(s => s.Name == NextStatus).FirstOrDefaultAsync();

                AcceptedTracking = new Tracking
                {
                    ProblemId = CurrentTracking.ProblemId,
                    StatusId = NextStatusId.Id,
                    UserId = UserId,
                    Time = DateTime.Now,
                    Comment = TrackingComment
                };

                //задание завершено
                if (new string[] { "Переносится", "Проверяется", "Дорабатывается", "Утверждается" }.Contains(CurrentTracking.Status.Name)
                    || CurrentTracking.Status.Name == "Не утверждён")
                {
                    AcceptedTracking.StartTime = CurrentTracking.Time;
                }

                await db.AddAsync(AcceptedTracking);

                //нужна проверка после переноса
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

                    await db.AddAsync(ToReviewTracking);
                }

                //следующее задание
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

                    await db.AddAsync(SecondTracking);
                }

                //следующее задание (после проверки админом)
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

                    await db.AddAsync(SecondTracking);
                }

                await db.SaveChangesAsync();
            }

            return RedirectToAction("table");
        }

        public async Task<IActionResult> Diagram(DiagramViewModel model)
        {
            DateTime? StartTime = model.StartTime ?? DateTime.MinValue;
            DateTime? EndTime = model.EndTime ?? DateTime.Now;
            EndTime = EndTime.Value.AddDays(1);

            using (DbContext db = new DbContext(new DbContextOptions<DbContext>()))
            {
                var CurrentUser = await _userManager.GetUserAsync(User);
                string userId = CurrentUser.Id;

                var allLatestTrackingsList = DatabaseQueries.GetAllLatestTrackings(db, userId);

                var completedTrackingsList = DatabaseQueries.GetCompletedTrackings(db, userId);

                var allTrackingsList = allLatestTrackingsList.Union(completedTrackingsList)
                                                         .OrderBy(t => t.Status.Id)
                                                             .ThenByDescending(t => t.Time)
                                                         .Where(t => ((t.Time >= StartTime && t.Time <= EndTime)
                                                                        || (t.StartTime >= StartTime && t.StartTime <= EndTime)))
                                                         .Select(t => new
                                                         {
                                                             Id = t.Id,
                                                             Problem = t.Problem.Name,
                                                             Course = t.Problem.Course.Name,
                                                             Status = t.Status.Name,
                                                             Type = t.Problem.TaskType.Name,
                                                             Time = t.Time,
                                                             StartTime = t.StartTime
                                                         });

                model.Json = JsonSerializer.Serialize(allTrackingsList); //сериализуем
            }
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

}