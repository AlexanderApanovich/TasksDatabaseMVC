using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TasksDatabase.Models;
using TasksDatabase.ViewModels;

namespace TasksDatabase.Controllers
{
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
            int pageSize = Convert.ToInt32(_config.GetSection("Pagination").GetSection("TasksPerPage").Value);
            bool acceptTask = true;

            var user = await _userManager.GetUserAsync(User);
            string userId = user.Id;

            using (DbContext db = new DbContext(new DbContextOptions<DbContext>()))
            {
                var latestTrackingQuery = db.Trackings.Where(t => t.User.Id == userId)  //подзапрос для получения последнего трекинга по каждому заданию
                                                      .GroupBy(t => t.Task.Id)
                                                      .Select(t => t.Max(t => t.Id));

                var allLatestTrackingsList = db.Trackings.Join(latestTrackingQuery, t => t.Id, l => l, (t, l) => t)  //последний трекинг по каждому заданию
                                                         .Include(t => t.Status)
                                                         .Include(t => t.User)
                                                         .Include(t => t.Task)
                                                             .ThenInclude(t => t.Course)
                                                                 .ThenInclude(c => c.Department)
                                                         .Include(t => t.Task)
                                                             .ThenInclude(t => t.TaskType)
                                                         .Select(t => t).ToList();

                var completedTrackingsList = db.Trackings.Where(t => t.User.Id == userId && t.StartTime != null)  //трекинги завершенных заданий
                                                         .Include(t => t.Status)
                                                         .Include(t => t.User)
                                                         .Include(t => t.Task)
                                                             .ThenInclude(t => t.Course)
                                                                 .ThenInclude(c => c.Department)
                                                         .Include(t => t.Task)
                                                             .ThenInclude(t => t.TaskType)
                                                         .Select(t => t).ToList();

                int completedCount = completedTrackingsList.Count();

                var allTrackingsList = allLatestTrackingsList.Union(completedTrackingsList)  //все трекинги
                                                             .OrderBy(t => t.Status.Id)
                                                                 .ThenByDescending(t => t.Time)
                                                             .Select(t => t)
                                                             .ToList();

                var totalCount = allTrackingsList.Count();

                foreach (var tracking in allTrackingsList)
                {
                    acceptTask = !(new string[] { "Переносится", "Проверяется", "Дорабатывается" }.Contains(tracking.Status.Name));
                }

                var items = allTrackingsList.Skip((page - 1) * pageSize)
                                            .Take(pageSize)
                                            .Select(t => t)
                                            .ToList();

                PageViewModel pageViewModel = new PageViewModel(totalCount, page, pageSize);
                IndexViewModel viewModel = new IndexViewModel
                {
                    PageViewModel = pageViewModel,
                    Trackings = items,
                    CompletedCount = completedCount,
                    TotalCount = totalCount,
                    CanAcceptTask = acceptTask,
                    UserId = userId
                };

                return View(viewModel);
            }
        }

        //todo добавить промежуточные страницы
        //     поменять {{tracking.id}} и остальные свойства кнопок





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Table(LoginViewModel model)
        {



            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Name, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Неправильный логин/пароль");
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
struct Tracks
{
    Tracking tracking;
    public Tracks(Tracking t)
    {
        tracking = t;
    }
}
