using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TasksDatabase.Models.Configurations
{
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.Property(u => u.Name).IsRequired().HasMaxLength(20);
            builder.Property(u => u.FullName).IsRequired().HasMaxLength(50);
        }
    }
}
