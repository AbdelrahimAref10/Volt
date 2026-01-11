using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfiguration
{
    public class CompanyTreasuryConfiguration : IEntityTypeConfiguration<CompanyTreasury>
    {
        public void Configure(EntityTypeBuilder<CompanyTreasury> builder)
        {
            builder.ToTable("VO_CompanyTreasury");

            // Configure primary key
            builder.HasKey(ct => ct.Id);

            // Configure properties
            builder.Property(ct => ct.Id)
                .HasColumnName("Id")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(ct => ct.TotalRevenue)
                .HasColumnName("TotalRevenue")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(ct => ct.TotalCancellationFees)
                .HasColumnName("TotalCancellationFees")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(ct => ct.LastUpdated)
                .HasColumnName("LastUpdated")
                .IsRequired();

            // Configure audit properties
            builder.Property(ct => ct.CreatedBy)
                .HasColumnName("CreatedBy")
                .HasMaxLength(256);

            builder.Property(ct => ct.CreatedDate)
                .HasColumnName("CreatedDate")
                .IsRequired();

            builder.Property(ct => ct.LastModifiedBy)
                .HasColumnName("LastModifiedBy")
                .HasMaxLength(256);

            builder.Property(ct => ct.LastModifiedDate)
                .HasColumnName("LastModifiedDate")
                .IsRequired();
        }
    }
}

