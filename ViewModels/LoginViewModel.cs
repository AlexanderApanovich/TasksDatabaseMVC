using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using TasksDatabase.Models;

namespace TasksDatabase.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name ="Логин")]
        public string Name { get; set; }

        [Required]
        [Display(Name ="Пароль")]
        public string Password { get; set; }

        [Display(Name ="Запомнить меня")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
    }
}
