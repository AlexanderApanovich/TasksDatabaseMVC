using System;
using System.Collections.Generic;
using TasksDatabase.ViewModels;
using System.Linq;

namespace TasksDatabase.Models
{
    public class TableViewModel
    {
        public List<TaskViewModel> TaskViewModelList { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public int CompletedCount { get; set; }
        public int TotalCount { get; set; }
        public bool CanAcceptTask { get; set; }
        public string UserId { get; set; }
    }
}


