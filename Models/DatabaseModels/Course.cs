using System.Collections.Generic;

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