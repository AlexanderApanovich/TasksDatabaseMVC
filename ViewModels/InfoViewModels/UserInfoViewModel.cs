using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TasksDatabase.Models;

namespace TasksDatabase.ViewModels
{
    public class CurrentTrackingViewModel
    {
        double? CurrentTaskTime { get; set; }
        double? WorkTime { get; set; }
        double? ReviewTime { get; set; }
        double? ReworkTime { get; set; }
        Tracking CurrentTracking { get; set; }
        public int WorkCount { get; set; }
        public int ReviewCount { get; set; }
        public int ReworkCount { get; set; }
    }
}
