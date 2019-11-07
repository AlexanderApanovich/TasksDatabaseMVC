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
        public List<UserInfo> UserInfos { get; set; }

        public class UserInfo
        {
            public User User { get; set; }
            public double? CurrentTaskTime { get; set; }
            public double? WorkTime { get; set; }
            public double? ReviewTime { get; set; }
            public double? ReworkTime { get; set; }
            public Tracking CurrentTracking { get; set; }
            public int WorkCount { get; set; }
            public int ReviewCount { get; set; }
            public int ReworkCount { get; set; }
        }
    }
}
