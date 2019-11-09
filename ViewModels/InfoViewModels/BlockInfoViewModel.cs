using System.Collections.Generic;
using TasksDatabase.Models;

namespace TasksDatabase.ViewModels
{
    public class BlockInfoViewModel
    {
        public List<Block> Blocks { get; set; }
        public List<Course> Courses { get; set; }
        public int CurrentBlock { get; set; }
    }
}
