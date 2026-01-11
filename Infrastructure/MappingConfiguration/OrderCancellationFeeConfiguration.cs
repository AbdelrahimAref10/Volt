using Domain.Models;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfiguration
{
    public class OrderCancellationFeeConfiguration : IEntityTypeConfiguration<OrderCancellationFee>
    {
        public void Configure(EntityTypeBuilder<OrderCancellationFee> builder)
        {
            builder.ToTable("VO_OrderCancellationFee");

            // Configure primary key
            builder.HasKey(ocf => ocf.Id);

            // Configure properties
            builder.Property(ocf => ocf.Id)
                .HasColumnName("Id")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(ocf => ocf.CustomerId)
                .HasColumnName("CustomerId")
                .IsRequired();

            builder.Property(ocf => ocf.OrderId)
                .HasColumnName("OrderId")
                .IsRequired();

            builder.Property(ocf => ocf.Amount)
                .HasColumnName("Amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(ocf => ocf.State)
                .HasColumnName("State")
                .HasConversion<int>()
                .HasDefaultValue(CancellationFeeState.NotYet)
                .IsRequired();

            // Configure audit properties
            builder.Property(ocf => ocf.CreatedBy)
                .HasColumnName("CreatedBy")
                .HasMaxLength(256);

            builder.Property(ocf => ocf.CreatedDate)
                .HasColumnName("CreatedDate")
                .IsRequired();

            builder.Property(ocf => ocf.LastModifiedBy)
                .HasColumnName("LastModifiedBy")
                .HasMaxLength(256);

            builder.Property(ocf => ocf.LastModifiedDate)
                .HasColumnName("LastModifiedDate")
                .IsRequired();

            // Configure relationships
            builder.HasOne(ocf => ocf.Customer)
                .WithMany()
                .HasForeignKey(ocf => ocf.CustomerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(ocf => ocf.Order)
                .WithOne(o => o.OrderCancellationFee)
                .HasForeignKey<OrderCancellationFee>(ocf => ocf.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Configure indexes
            builder.HasIndex(ocf => ocf.OrderId)
                .HasDatabaseName("IX_VO_OrderCancellationFee_OrderId")
                .IsUnique();

            builder.HasIndex(ocf => ocf.CustomerId)
                .HasDatabaseName("IX_VO_OrderCancellationFee_CustomerId");

            builder.HasIndex(ocf => ocf.State)
                .HasDatabaseName("IX_VO_OrderCancellationFee_State");
        }
    }
}

