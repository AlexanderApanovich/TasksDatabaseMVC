using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TasksDatabase.Models;
using TasksDatabase.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace TasksDatabase.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AdminController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

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
            return View(model); //отправляем модель в представление
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
                notStartedStatus = await db.Statuses.Where(s => s.Name == "Не перенесён").Select(s => s.Id).FirstAsync();

                Problem problem = new Problem
                {
                    CourseId = courseId,
                    TaskTypeId = typeId,
                    Name = name,
                    DeveloperId = developerId,
                    ReviewerId = reviewerId
                };
                await db.AddAsync(problem);  //добавляем задание со статусом "Не перенесён"
                await db.SaveChangesAsync();

                Tracking tracking = new Tracking
                {
                    Time = DateTime.Now,
                    Comment = comment,
                    StatusId = notStartedStatus,
                    ProblemId = problem.Id,
                    UserId = developerId
                };
                await db.AddAsync(tracking); //добавляем трекинг, соответствующий этому заданию
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

                model.Blocks = await db.Blocks.Select(b => b).ToListAsync();

                if (block != 0) //все блоки
                {
                    model.Courses = await db.CourseBlocks.Where(b => b.BlockId == block)
                                                         .Include(c => c.Course)
                                                         .Join(db.Courses,
                                                               b => b.CourseId,
                                                               c => c.Id,
                                                               (b, c) => c)
                                                         .ToListAsync();
                }
                else //текущий блок
                {
                    model.Courses = await db.Courses.Select(c => c).ToListAsync();
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
                IEnumerable<int> latestTracking;

                if (block == 0 && course == 0) //страница по умолчанию
                {
                    latestTracking = db.Trackings.GroupBy(t => t.Problem.Id)
                                                 .Select(t => t.Max(t => t.Id));
                    model.Title = $"Задания всех блоков";
                }
                else if (course == 0) //выбран блок
                {
                    latestTracking = db.Trackings.Where(t => t.Problem.Course.CourseBlocks
                                                 .Any(cb => cb.BlockId == block))
                                                 .GroupBy(t => t.Problem.Id)
                                                 .Select(t => t.Max(t => t.Id));
                    model.Title = $"Задания блока { db.Blocks.Where(b => b.Id == block).FirstOrDefault().Name }";
                }
                else //выбран курс
                {
                    latestTracking = db.Trackings.Where(t => t.Problem.Course.Id == course)
                                                 .GroupBy(t => t.Problem.Id)
                                                 .Select(t => t.Max(t => t.Id));
                    model.Title = $"Задания дисциплины { db.Courses.Where(c => c.Id == course).FirstOrDefault().FullName }";
                }

                model.Trackings = await db.Trackings.Where(t => latestTracking.Contains(t.Id))
                                                    .Include(t => t.Problem)
                                                      .ThenInclude(p => p.Course)
                                                    .Include(t => t.Status)
                                                    .Include(t => t.User)
                                                    .OrderBy(t => t.Problem.Course.Id)
                                                    .ThenBy(t => t.Problem.Name)
                                                    .Select(t => t)
                                                    .ToListAsync();

                return View(model);
            }
        }

        public async Task<IActionResult> TaskInfo(int task)
        {
            using (DbContext db = new DbContext(new DbContextOptions<DbContext>()))
            {
                CourseInfoViewModel model = new CourseInfoViewModel();

                List<Tracking> TrackingsList = await db.Trackings.Where(t => t.Problem.Id == task)
                                                                 .Include(t => t.Problem)
                                                                   .ThenInclude(p => p.Course)
                                                                 .Include(t => t.Status)
                                                                 .Include(t => t.User)
                                                                 .OrderByDescending(t => t.Time)
                                                                 .Select(t => t)
                                                                 .ToListAsync();

                return View(TrackingsList);
            }
        }

        public async Task<IActionResult> UserInfo(string user)
        {
            using (DbContext db = new DbContext(new DbContextOptions<DbContext>()))
            {
                UserInfoViewModel model = new UserInfoViewModel();
                model.Users = db.Users.Select(u => u);

                IEnumerable<User> UsersList; //для выбора пользователя в представлении
                IEnumerable<Tracking> latestTrackingCommon;

                if (user != null)
                {
                    UsersList = await db.Users.Where(u => u.Id == user).Select(u => u).ToListAsync();
                    latestTrackingCommon = db.Trackings.Include(t => t.Status)
                                                       .Where(t => t.User.Id == user);
                }
                else
                {
                    UsersList = await db.Users.Select(u => u).ToListAsync();
                    latestTrackingCommon = db.Trackings.Include(t => t.Status);
                }

                var latestTracking = latestTrackingCommon.GroupBy(t => t.ProblemId)
                                                         .Select(t => t.Max(t => t.Id));

                IEnumerable<Tracking> allTrackings = db.Trackings.Where(t => latestTracking.Contains(t.Id))
                                                                 .Include(t => t.Problem)
                                                                     .ThenInclude(p => p.TaskType)
                                                                 .Include(t => t.Problem.Course)
                                                                 .Include(t => t.User)
                                                                 .Select(t => t);

                //текущее задание и время его выполнения
                var CurrentTracking = allTrackings.Where(t =>
                                      new string[] { "Дорабатывается", "Переносится", "Проверяется" }
                                              .Contains(t.Status.Name)).Select(t => t);

                var CurrentTaskTime = CurrentTracking.Select(t => (UserId: t.UserId, CurrentTime: (int?)(DateTime.Now - t.Time).TotalMinutes));

                //получение количества выполненных заданий и общего времени на каждый тип заданий
                var WorkDone = latestTrackingCommon.Where(t => t.StartTime != null && t.Status.Name == "Перенесён");
                var ReviewDone = latestTrackingCommon.Where(t => t.StartTime != null && t.Status.Name == "Проверен");
                var ReworkDone = latestTrackingCommon.Where(t => t.StartTime != null && t.Status.Name == "Доработан");

                var WorkCount = DatabaseQueries.GetCount(WorkDone);
                var ReviewCount = DatabaseQueries.GetCount(ReviewDone);
                var ReworkCount = DatabaseQueries.GetCount(ReworkDone);

                var WorkTime = DatabaseQueries.GetTime(WorkDone);
                var ReviewTime = DatabaseQueries.GetTime(ReviewDone);
                var ReworkTime = DatabaseQueries.GetTime(ReworkDone);

                //объединение результатов в одну таблицу
                var UsersWithInfo = from u in UsersList

                                    join ctgGroup in CurrentTracking on u.Id equals ctgGroup.UserId into ctgGroup
                                    from ctg in ctgGroup.DefaultIfEmpty() //LEFT OUTER JOIN

                                    join cttGroup in CurrentTaskTime on u.Id equals cttGroup.UserId into cttGroup
                                    from ctt in cttGroup.DefaultIfEmpty()


                                    join wcGroup in WorkCount on u.Id equals wcGroup.UserId into wcGroup
                                    from wc in wcGroup.DefaultIfEmpty()

                                    join revcGroup in ReviewCount on u.Id equals revcGroup.UserId into revcGroup
                                    from revc in revcGroup.DefaultIfEmpty()

                                    join rewcGroup in ReworkCount on u.Id equals rewcGroup.UserId into rewcGroup
                                    from rewc in rewcGroup.DefaultIfEmpty()


                                    join wtGroup in WorkTime on u.Id equals wtGroup.UserId into wtGroup
                                    from wt in wtGroup.DefaultIfEmpty()

                                    join revtGroup in ReviewTime on u.Id equals revtGroup.UserId into revtGroup
                                    from revt in revtGroup.DefaultIfEmpty()

                                    join rewtGroup in ReworkTime on u.Id equals rewtGroup.UserId into rewtGroup
                                    from rewt in rewtGroup.DefaultIfEmpty()

                                    select new UserInfoViewModel.UserInfo
                                    {
                                        User = u,
                                        CurrentTracking = ctg,
                                        CurrentTaskTime = ctt.CurrentTime,
                                        WorkCount = wc.Count,
                                        ReviewCount = revc.Count,
                                        ReworkCount = rewc.Count,
                                        WorkTime = wt.Sum,
                                        ReviewTime = revt.Sum,
                                        ReworkTime = rewt.Sum,
                                    };

                model.UserInfos = UsersWithInfo.ToList();

                return View(model);
            }
        }
    }

}