using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfiguration
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            // Configure primary key
            builder.HasKey(c => c.CategoryId);

            // Configure properties
            builder.Property(c => c.CategoryId)
                .HasColumnName("CategoryId")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(c => c.Name)
                .HasColumnName("Name")
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(c => c.Description)
                .HasColumnName("Description")
                .HasMaxLength(1000);

            builder.Property(c => c.ImageUrl)
                .HasColumnName("ImageUrl")
                .HasColumnType("nvarchar(max)");

            builder.Property(c => c.IsActive)
                .HasColumnName("IsActive")
                .HasDefaultValue(true)
                .IsRequired();

            // Configure audit properties
            builder.Property(c => c.CreatedBy)
                .HasColumnName("CreatedBy")
                .HasMaxLength(256);

            builder.Property(c => c.CreatedDate)
                .HasColumnName("CreatedDate")
                .IsRequired();

            builder.Property(c => c.LastModifiedBy)
                .HasColumnName("LastModifiedBy")
                .HasMaxLength(256);

            builder.Property(c => c.LastModifiedDate)
                .HasColumnName("LastModifiedDate")
                .IsRequired();

            // Configure relationships
            builder.HasMany(c => c.Vehicles)
                .WithOne(v => v.Category)
                .HasForeignKey(v => v.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete to preserve data integrity

            // Configure indexes
            builder.HasIndex(c => c.Name)
                .HasDatabaseName("IX_Categories_Name")
                .IsUnique();

            builder.HasIndex(c => c.IsActive)
                .HasDatabaseName("IX_Categories_IsActive");
        }
    }
}

