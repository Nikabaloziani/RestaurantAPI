using Microsoft.EntityFrameworkCore;
using RestaurantDATA.Entities;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace RestaurantDATA.Configurations
{
    public class RestaaurantUserConfiguration : IEntityTypeConfiguration<Entities.RestaurantUser>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Entities.RestaurantUser> builder)
        {
            builder.HasKey(ru => new
            {
                ru.RestaurantId,
                ru.UserId
            });

            builder.HasOne(ru => ru.Restaurant)
                .WithMany(r => r.RestaurantUsers)
                .HasForeignKey(ru => ru.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ru => ru.User)
                .WithMany()
                .HasForeignKey(ru => ru.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    
    }
}
