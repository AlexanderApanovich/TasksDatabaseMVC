using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TasksDatabase.Models;

namespace TasksDatabase.ViewModels
{
    public class TaskViewModel
    {
        public Tracking Tracking { get; set; }

        [Display(Name="Отправить на доработку?")]
        public bool NeedRework { get; set; }

        [DataType(DataType.Text)]
        public string Comment { get; set; }
    }
}
