using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TasksDatabase.Models.Configurations
{
    public class TrackingConfiguration : IEntityTypeConfiguration<Tracking>
    {
        public void Configure(EntityTypeBuilder<Tracking> builder)
        {
            builder.Property(u => u.Time).IsRequired();
            builder.Property(u => u.StartTime).IsRequired();
            builder.Property(u => u.Comment).HasMaxLength(255);
        }
    }
}
