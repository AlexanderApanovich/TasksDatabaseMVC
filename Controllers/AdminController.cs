using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TasksDatabase.Models;
using TasksDatabase.ViewModels;
using Microsoft.EntityFrameworkCore;
using TasksDatabase.Controllers;

namespace TasksDatabase.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AdminController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }


        //todo убрать рекомпиляцию
        //todo диаграмма ганта
        //todo отчеты блоки, курсы, таски, юзеры


        [HttpGet]
        public IActionResult Add()
        {
            List<Course> courses;
            List<TaskType> types;
            List<User> users;

            using (DbContext db = new DbContext(new DbContextOptions<DbContext>()))
            {
                courses = db.Courses.Select(c => c).ToList();
                types = db.TaskTypes.Select(t => t).ToList();
                users = db.Users.Select(c => c).ToList();
            }

            AddViewModel model = new AddViewModel
            {
                Courses = courses,
                Types = types,
                Users = users
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddViewModel model)
        {
            string name = Request.Form["Name"];
            string developerId = Request.Form["DeveloperId"];
            string reviewerId = Request.Form["ReviewerId"];
            string comment = Request.Form["Comment"];

            int courseId;
            int typeId;
            int notStartedStatus;

            Int32.TryParse(Request.Form["CourseId"], out courseId);
            Int32.TryParse(Request.Form["TypeId"], out typeId);

            using (DbContext db = new DbContext(new DbContextOptions<DbContext>()))
            {
                notStartedStatus = db.Statuses.Where(s => s.Name == "Не перенесён").Select(s => s.Id).First();

                Problem problem = new Problem
                {
                    CourseId = courseId,
                    TaskTypeId = typeId,
                    Name = name,
                    DeveloperId = developerId,
                    ReviewerId = reviewerId
                };
                await db.AddAsync(problem);
                await db.SaveChangesAsync();

                Tracking tracking = new Tracking
                {
                    Time = DateTime.Now,
                    Comment = comment,
                    StatusId = notStartedStatus,
                    ProblemId = problem.Id,
                    UserId = developerId
                };
                await db.AddAsync(tracking);
                await db.SaveChangesAsync();
            }

            return RedirectToAction("add");
        }

        [HttpGet]
        public async Task<IActionResult> BlockInfo(int block)
        {
            using (DbContext db = new DbContext(new DbContextOptions<DbContext>()))
            {
                BlockInfoViewModel model = new BlockInfoViewModel();

                var blocksQuery = db.Blocks.Select(b => b);
                model.Blocks = await blocksQuery.ToListAsync();

                if (block != 0)
                {
                    model.Courses = db.CourseBlocks.Where(b => b.BlockId == block)
                                                   .Include(c => c.Course)
                                                   .Join(db.Courses,
                                                         b => b.CourseId,
                                                         c => c.Id,
                                                         (b, c) => c).ToList();
                }
                else
                {
                    model.Courses = db.Courses.Select(c => c).ToList();
                }
                model.CurrentBlock = block;

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> CourseInfo(int block, int course)
        {
            using (DbContext db = new DbContext(new DbContextOptions<DbContext>()))
            {
                CourseInfoViewModel model = new CourseInfoViewModel();
                List<int> latestTracking;

                if (block == 0 && course == 0)
                {
                    latestTracking = await db.Trackings.GroupBy(t => t.Problem.Id)
                                                 .Select(t => t.Max(t => t.Id)).ToListAsync();
                    model.Title = $"Задания всех блоков";
                }
                else if (course == 0)
                {
                    latestTracking = await db.Trackings.Where(t => t.Problem.Course.CourseBlocks
                                                                  .Any(cb => cb.BlockId == block))
                                                 .GroupBy(t => t.Problem.Id)
                                                 .Select(t => t.Max(t => t.Id)).ToListAsync();
                    model.Title = $"Задания блока { db.Blocks.Where(b => b.Id == block).FirstOrDefault().Name }";
                }
                else
                {
                    latestTracking = await db.Trackings.Where(t => t.Problem.Course.Id == course)
                                                 .GroupBy(t => t.Problem.Id)
                                                 .Select(t => t.Max(t => t.Id)).ToListAsync();
                    model.Title = $"Задания дисциплины { db.Courses.Where(c => c.Id == course).FirstOrDefault().FullName }";
                }

                model.Trackings = db.Trackings.Where(t => latestTracking.Contains(t.Id))
                                              .Include(t => t.Problem)
                                                .ThenInclude(p => p.Course)
                                              .Include(t => t.Status)
                                              .Include(t => t.User)
                                              .OrderBy(t => t.Problem.Course.Id)
                                              .ThenBy(t => t.Problem.Name)
                                              .Select(t => t)
                                              .ToList();

                return View(model);
            }
        }

        public IActionResult TaskInfo(int task)
        {
            using (DbContext db = new DbContext(new DbContextOptions<DbContext>()))
            {
                CourseInfoViewModel model = new CourseInfoViewModel();

                var TrackingsList = db.Trackings.Where(t => t.Problem.Id == task)
                                              .Include(t => t.Problem)
                                                .ThenInclude(p => p.Course)
                                              .Include(t => t.Status)
                                              .Include(t => t.User)
                                              .OrderByDescending(t => t.Time)
                                              .Select(t => t)
                                              .ToList();

                return View(TrackingsList);
            }
        }

        public async Task<IActionResult> UserInfo(string user)
        {
            using (DbContext db = new DbContext(new DbContextOptions<DbContext>()))
            {
                UserInfoViewModel model = new UserInfoViewModel();
                model.UserInfos = await db.Users.Select(u => u.).ToListAsync();




                if (user != "")
                {
                    var latestTracking = db.Trackings.Where(t => t.User.Id == user)
                                                     .GroupBy(t => t.Problem.Id)
                                                     .Select(t => t.Max(t => t.Id)).ToList();
                    var allTrackings = db.Trackings.Where(t => latestTracking.Contains(t.Id))
                                                   .Include(t => t.Problem)
                                                       .ThenInclude(p => p.TaskType)
                                                   .Include(t => t.Problem.Course)
                                                   .Include(t => t.Status)
                                                   .Select(t => t).ToList();

                    var CurrentTracking = allTrackings.Where(t =>
                                            new string[] { "Дорабатывается", "Переносится", "Проверяется" }
                                            .Contains(t.Status.Name)).FirstOrDefault();

                    model.CurrentTracking = CurrentTracking;
                    model.CurrentTaskTime = (DateTime.Now - CurrentTracking.Time).TotalMinutes;

                    var trackings = db.Trackings.Where(t => t.User.Id == user && t.StartTime != null)
                                                .Include(t => t.Status).ToList();
                    model.WorkCount = trackings.Where(t => t.Status.Name == "Перенесён").Count();
                    model.ReviewTime = trackings.Where(t => t.Status.Name == "Проверен").Count();
                    model.ReworkCount = trackings.Where(t => t.Status.Name == "Доработан").Count();


                    model.WorkTime = db.Trackings.Where(t => t.Status.Name == "Перенесён")
                                                 .Sum(t => (t.Time - t.StartTime).Value.TotalMinutes);

                    model.ReviewTime = db.Trackings.Where(t => t.Status.Name == "Проверен")
                                                   .Sum(t => (t.Time - t.StartTime).Value.TotalMinutes);

                    model.ReworkTime = db.Trackings.Where(t => t.Status.Name == "Доработан")
                                                   .Sum(t => (t.Time - t.StartTime).Value.TotalMinutes);
                }

                return View(model);
            }
        }

    }

}