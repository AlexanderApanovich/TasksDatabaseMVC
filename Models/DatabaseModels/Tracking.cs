using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TasksDatabase.Models
{
    public class Tracking
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public DateTime StartTime { get; set; }
        public string Comment { get; set; }
        
        public Status Status { get; set; }
        public Task Task { get; set; }
        public User User { get; set; }

        public override string ToString()
        {
            return $"<Tracking: Id = {Id}, Time = {Time}, StartTime = {StartTime}," +
                $" Comment = {Comment}>";
        }

    }
}
