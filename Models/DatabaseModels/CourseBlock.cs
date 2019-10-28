using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TasksDatabase.Models
{
    public class CourseBlock
    {
        public int Id { get; set; }

        public int CourseId { get; set; }
        public int BlockId { get; set; }
        public Course Course { get; set; }
        public Block Block { get; set; }
    }
}
