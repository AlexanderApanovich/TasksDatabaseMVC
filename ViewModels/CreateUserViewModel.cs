using System.ComponentModel.DataAnnotations;

namespace TasksDatabase.ViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        [Display(Name = "Логин")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Пароль")]
        public string Password { get; set; }
    }
}
