using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TasksDatabase.Models;
using TasksDatabase.Models.Configurations;

namespace TasksDatabase
{
    public class Context : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Block> Trackings { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Block> Blocks { get; set; }
        public DbSet<CourseBlock> CourseBlock { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<TaskType> TaskTypes { get; set; }

        public Context()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb; Database=TasksDatabase; Trusted_Connection=True");

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CourseConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new TaskConfiguration());
            modelBuilder.ApplyConfiguration(new TrackingConfiguration());
        }
    }
}