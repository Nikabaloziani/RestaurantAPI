using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantDATA.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantDATA.Configurations
{
    public class BasketConfiguration : IEntityTypeConfiguration<Basket>
    {
        public void Configure(EntityTypeBuilder<Basket> builder)
        {
            builder.ToTable("Baskets");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.ProductId)
                     .IsRequired();

            builder.Property(b => b.Quantity)

                     .IsRequired();

            //builder.Property(b => b.Price)
            //            .IsRequired()
            //            .HasColumnType("decimal(18,2)");


        }
    }
}
