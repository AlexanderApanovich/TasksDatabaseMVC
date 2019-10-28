using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TasksDatabase.Models.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasIndex(u => u.UserName);
            builder.Property(u => u.UserName).IsRequired().HasMaxLength(20);
            builder.Property(u => u.IsAdmin).IsRequired();
            builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(128);
        }
    }
}
