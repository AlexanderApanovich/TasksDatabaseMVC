using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

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
