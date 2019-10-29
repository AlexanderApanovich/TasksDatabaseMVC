using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TasksDatabase.Models;

namespace TasksDatabase.ViewModels
{
    public class EditUserViewModel
    {
        [Required]
        [Display(Name = "Id")]
        public string Id { get; set; }

        [Required]
        [Display(Name = "Логин")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Админ")]
        public bool IsAdmin { get; set; }
    }
}
