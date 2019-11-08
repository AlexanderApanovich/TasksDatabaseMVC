using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TasksDatabase.Models
{
    static class DatabaseQueries
    {
        public static IQueryable<int> GetLatestTrackings(DbContext db, string userId)
        {
            //подзапрос для получения последнего трекинга по каждому заданию
            var latestTracking = db.Trackings.GroupBy(t => t.Problem.Id)
                                             .Select(t => t.Max(t => t.Id));
            return latestTracking;
        }

        public static List<Tracking> GetAllLatestTrackings(DbContext db, string userId)
        {
            //последний трекинг по каждому заданию
            var latestTrackingQuery = GetLatestTrackings(db, userId);

            var allLatestTrackingsList = db.Trackings.Join(latestTrackingQuery, t => t.Id, l => l, (t, l) => t)
                                                     .Where(t => t.User.Id == userId)
                                                     .Include(t => t.Status)
                                                     .Include(t => t.User)
                                                     .Include(t => t.Problem)
                                                         .ThenInclude(t => t.Course)
                                                             .ThenInclude(c => c.Department)
                                                     .Include(t => t.Problem)
                                                         .ThenInclude(t => t.TaskType)
                                                     .Select(t => t).ToList();
            return allLatestTrackingsList;
        }

        public static List<Tracking> GetCompletedTrackings(DbContext db, string userId)
        {
            var completedTrackingsList = db.Trackings.Where(t => t.User.Id == userId && t.StartTime != null)  //трекинги завершенных заданий
                                         .Include(t => t.Status)
                                         .Include(t => t.User)
                                         .Include(t => t.Problem)
                                             .ThenInclude(t => t.Course)
                                                 .ThenInclude(c => c.Department)
                                         .Include(t => t.Problem)
                                             .ThenInclude(t => t.TaskType)
                                         .Select(t => t).ToList();
            return completedTrackingsList;
        }

        public static IEnumerable<(string UserId,int Count)> GetCount(IEnumerable<Tracking> query)
        {
            return query.GroupBy(t => t.UserId)
                        .Select(t => (t.FirstOrDefault().UserId, t.Count()));
        }
        public static IEnumerable<(string UserId, int? Sum)> GetTime(IEnumerable<Tracking> query)
        {
            return query.GroupBy(t => t.UserId)
                        .Select(t => (t.FirstOrDefault().UserId, (int?) t.Sum(t => (t.Time - t.StartTime).Value.TotalMinutes)));
        }
    }
}