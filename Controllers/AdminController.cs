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
        public async Task<IActionResult> Add(AddViewModel model)//AddViewModel model)
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
    }
}
