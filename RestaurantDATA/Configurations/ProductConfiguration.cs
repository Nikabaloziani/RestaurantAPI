using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantDATA.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantDATA.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Products>
    {
        public void Configure(EntityTypeBuilder<Products> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.Price)
                   .HasColumnType("decimal(18,2)");

            builder.HasOne(x => x.Category)
                   .WithMany(c => c.Products)
                   .HasForeignKey(x => x.CategoryId);
        }
    }
}
