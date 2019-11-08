using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TasksDatabase.Models;

namespace TasksDatabase.ViewModels
{
    public class UserInfoViewModel
    {
        public IEnumerable<UserInfo> UserInfos { get; set; }
        public IEnumerable<User> Users { get; set; }

        public class UserInfo
        {
            public User User { get; set; }
            public Tracking CurrentTracking { get; set; }
            public int? CurrentTaskTime { get; set; }

            public int WorkCount { get; set; }
            public int ReviewCount { get; set; }
            public int ReworkCount { get; set; }

            public int? WorkTime { get; set; }
            public int? ReviewTime { get; set; }
            public int? ReworkTime { get; set; }
        }
    }
}
