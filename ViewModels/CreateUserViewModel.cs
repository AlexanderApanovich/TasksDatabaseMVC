using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TasksDatabase.ViewModels
{
    public class CreateUserViewModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
    }
}
