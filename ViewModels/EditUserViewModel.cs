using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TasksDatabase.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public bool IsAdmin { get; set; }
    }
}
