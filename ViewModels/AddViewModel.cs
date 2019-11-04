using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TasksDatabase.Models;

namespace TasksDatabase.ViewModels
{
    public class AddViewModel
    {
        [Required]
        [Display(Name = "Название задания")]
        public string Name { get; set; }
        
        [Required]
        public int CourseId { get; set; } 

        [Required]
        public int TypeId { get; set; }
        
        [Required]
        public string DeveloperId { get; set; }

        [Required]
        public string ReviewerId { get; set; }
        
        [DataType(DataType.Text)]
        [Display(Name = "Комментарий (опционально)")]
        public string Comment { get; set; }

        public IEnumerable<Course> Courses { get; set; }
        public IEnumerable<TaskType> Types { get; set; }
        public IEnumerable<User> Users { get; set; }
    }
}
