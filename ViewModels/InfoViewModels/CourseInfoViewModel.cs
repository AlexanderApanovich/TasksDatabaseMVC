using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TasksDatabase.Models;

namespace TasksDatabase.ViewModels
{
    public class CourseInfoViewModel
    {
        public List<Tracking> Trackings { get; set; }
        public string Title { get; set; }
    }
}
