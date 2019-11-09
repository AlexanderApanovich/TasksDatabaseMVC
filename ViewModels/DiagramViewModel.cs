using System;
using System.ComponentModel.DataAnnotations;

namespace TasksDatabase.ViewModels
{
    public class DiagramViewModel
    {
        [Display(Name = "Начальная дата")]
        [DataType(DataType.Date)]
        public DateTime? StartTime { get; set; }

        [Display(Name = "Конечная дата")]
        [DataType(DataType.Date)]
        public DateTime? EndTime { get; set; }

        public string Json { get; set; }
    }
}
