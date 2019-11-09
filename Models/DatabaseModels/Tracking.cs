using System;

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

        public override string ToString()
        {
            return $"<Tracking: Id = {Id}, Time = {Time}, StartTime = {StartTime}," +
                $" Comment = {Comment}, Status = {Status}, Task = {Problem}, User = {User}";
        }

    }
}
