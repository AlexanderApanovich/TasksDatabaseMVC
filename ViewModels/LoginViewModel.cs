﻿using System.ComponentModel.DataAnnotations;

namespace TasksDatabase.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name ="Логин")]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name ="Пароль")]
        public string Password { get; set; }

        [Display(Name ="Запомнить меня")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
    }
}
