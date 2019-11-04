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
        public DateTime? StartTime { get; set; }
        public string Comment { get; set; }


        public int StatusId { get; set; }
        public int ProblemId { get; set; }
        public string UserId { get; set; }
        public Status Status { get; set; }
        public Problem Problem { get; set; }
        public User User { get; set; }

        //public Tracking Clone()
        //{
        //    return new Tracking
        //    {
        //        Id = this.Id,
        //        Time = this.Time,
        //        StartTime = this.StartTime,
        //        Comment = this.Comment,
        //        Status = this.Status,
        //        Task = this.Task,
        //        User = this.User
        //    };
        //}

        public override string ToString()
        {
            return $"<Tracking: Id = {Id}, Time = {Time}, StartTime = {StartTime}," +
                $" Comment = {Comment}, Status = {Status}, Task = {Problem}, User = {User}";
        }

    }
}
