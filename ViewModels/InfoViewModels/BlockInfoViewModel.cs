using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
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
