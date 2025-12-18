using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfiguration
{
    public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> builder)
        {
            builder.ToTable("Vehicles");

            // Configure primary key
            builder.HasKey(v => v.VehicleId);

            // Configure properties
            builder.Property(v => v.VehicleId)
                .HasColumnName("VehicleId")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(v => v.Name)
                .HasColumnName("Name")
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(v => v.ImageUrl)
                .HasColumnName("ImageUrl")
                .HasColumnType("nvarchar(max)");

            builder.Property(v => v.PricePerHour)
                .HasColumnName("PricePerHour")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(v => v.Status)
                .HasColumnName("Status")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(v => v.CreatedThisMonth)
                .HasColumnName("CreatedThisMonth");

            builder.Property(v => v.CategoryId)
                .HasColumnName("CategoryId")
                .IsRequired();

            // Configure audit properties
            builder.Property(v => v.CreatedBy)
                .HasColumnName("CreatedBy")
                .HasMaxLength(256);

            builder.Property(v => v.CreatedDate)
                .HasColumnName("CreatedDate")
                .IsRequired();

            builder.Property(v => v.LastModifiedBy)
                .HasColumnName("LastModifiedBy")
                .HasMaxLength(256);

            builder.Property(v => v.LastModifiedDate)
                .HasColumnName("LastModifiedDate")
                .IsRequired();

            // Configure relationships
            builder.HasOne(v => v.Category)
                .WithMany(c => c.Vehicles)
                .HasForeignKey(v => v.CategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Configure indexes
            builder.HasIndex(v => v.CategoryId)
                .HasDatabaseName("IX_Vehicles_CategoryId");

            builder.HasIndex(v => v.Status)
                .HasDatabaseName("IX_Vehicles_Status");

            builder.HasIndex(v => new { v.CategoryId, v.Status })
                .HasDatabaseName("IX_Vehicles_CategoryId_Status");

            builder.HasIndex(v => v.CreatedThisMonth)
                .HasDatabaseName("IX_Vehicles_CreatedThisMonth");
        }
    }
}

