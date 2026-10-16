using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantDATA.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantDATA.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.HasOne(p => p.Order)
                .WithMany()
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
