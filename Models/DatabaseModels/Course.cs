using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TasksDatabase.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }

        public Department Department { get; set; }
        public ICollection<CourseBlock> CourseBlocks { get; set; }
    }
}