using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TasksDatabase.Models
{
    public class Problem
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int CourseId { get; set; }
        public int TaskTypeId { get; set; }
        public string DeveloperId { get; set; }
        public string ReviewerId { get; set; }

        public Course Course { get; set; }
        public TaskType TaskType { get; set; }
    }
}