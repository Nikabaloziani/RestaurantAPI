using Microsoft.EntityFrameworkCore;
using RestaurantDATA.Entities;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace RestaurantDATA.Configurations
{
    public class RestaurantConfiguration : IEntityTypeConfiguration<Entities.Restaurant>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Entities.Restaurant> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Name)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(r => r.Code)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(r => r.Slug)
                .HasMaxLength(150)
                .IsRequired();

            builder.HasIndex(r => r.Code).IsUnique();
            builder.HasIndex(r => r.Slug).IsUnique();
        }
    }
}
