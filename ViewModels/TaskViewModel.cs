using System.ComponentModel.DataAnnotations;
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
