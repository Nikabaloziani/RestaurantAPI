using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantDATA.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantDATA.Configurations
{
    public class TableConfiguration : IEntityTypeConfiguration<Table>
    {
        public void Configure(EntityTypeBuilder<Table> builder)
        {
            builder.ToTable("Tables");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.TableNumber)
                   .IsRequired();

            builder.Property(t => t.Capacity)
                     .IsRequired();

            builder.Property(t => t.IsOccupied)
                     .IsRequired()
                     .HasDefaultValue(false);

            builder.Property(t => t.Zone)
                        .IsRequired();

            builder.Property(t => t.IsActive)
                        .IsRequired()
                        .HasDefaultValue(true);

            builder.Property(t => t.IsDeleted)
                        .IsRequired()
                        .HasDefaultValue(false);

            builder.Property(t => t.CreatedAt)
                        .IsRequired()
                        .HasDefaultValueSql("GETUTCDATE()");


        }
    }
}
