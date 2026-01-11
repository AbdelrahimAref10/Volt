using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfiguration
{
    public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> builder)
        {
            builder.ToTable("VO_Vehicle");

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

            builder.Property(v => v.VehicleCode)
                .HasColumnName("VehicleCode")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(v => v.ImageUrl)
                .HasColumnName("ImageUrl")
                .HasColumnType("nvarchar(max)");

            builder.Property(v => v.Status)
                .HasColumnName("Status")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(v => v.CreatedThisMonth)
                .HasColumnName("CreatedThisMonth");

            builder.Property(v => v.SubCategoryId)
                .HasColumnName("SubCategoryId")
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
            builder.HasOne(v => v.SubCategory)
                .WithMany(sc => sc.Vehicles)
                .HasForeignKey(v => v.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Configure indexes
            builder.HasIndex(v => v.SubCategoryId)
                .HasDatabaseName("IX_VO_Vehicle_SubCategoryId");

            builder.HasIndex(v => v.Status)
                .HasDatabaseName("IX_VO_Vehicle_Status");

            builder.HasIndex(v => new { v.SubCategoryId, v.Status })
                .HasDatabaseName("IX_VO_Vehicle_SubCategoryId_Status");

            builder.HasIndex(v => v.CreatedThisMonth)
                .HasDatabaseName("IX_VO_Vehicle_CreatedThisMonth");

            builder.HasIndex(v => v.VehicleCode)
                .HasDatabaseName("IX_VO_Vehicle_VehicleCode")
                .IsUnique();
        }
    }
}

