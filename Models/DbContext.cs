using Microsoft.EntityFrameworkCore;
using TasksDatabase.Models;
using TasksDatabase.Models.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace TasksDatabase
{
    public class DbContext : IdentityDbContext<User>
    {
        public DbSet<User> UsersList { get; set; }
        public DbSet<Tracking> Trackings { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Block> Blocks { get; set; }
        public DbSet<CourseBlock> CourseBlocks { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Problem> Problems { get; set; }
        public DbSet<TaskType> TaskTypes { get; set; }

        public DbContext(DbContextOptions<DbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb; Database=TasksDatabase; Trusted_Connection=True");

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new CourseConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new ProblemConfiguration());
            modelBuilder.ApplyConfiguration(new TrackingConfiguration());
        }
    }
}