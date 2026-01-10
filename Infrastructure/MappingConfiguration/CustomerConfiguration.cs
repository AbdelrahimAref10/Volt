using Domain.Models;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfiguration
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("VO_Customer");

            // Configure primary key
            builder.HasKey(c => c.CustomerId);

            // Configure properties
            builder.Property(c => c.CustomerId)
                .HasColumnName("CustomerId")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(c => c.MobileNumber)
                .HasColumnName("MobileNumber")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(c => c.UserName)
                .HasColumnName("UserName")
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(c => c.FullName)
                .HasColumnName("FullName")
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(c => c.Gender)
                .HasColumnName("Gender")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(c => c.PersonalImage)
                .HasColumnName("PersonalImage");

            builder.Property(c => c.RegisterAs)
                .HasColumnName("RegisterAs")
                .HasConversion<int>()
                .IsRequired();

            builder.Property(c => c.VerificationBy)
                .HasColumnName("VerificationBy")
                .HasConversion<int>()
                .IsRequired();

            builder.Property(c => c.FullAddress)
                .HasColumnName("FullAddress")
                .HasMaxLength(500);

            builder.Property(c => c.State)
                .HasColumnName("State")
                .HasConversion<int>()
                .HasDefaultValue(CustomerState.InActive)
                .IsRequired();

            builder.Property(c => c.InvitationCode)
                .HasColumnName("InvitationCode")
                .HasMaxLength(10);

            builder.Property(c => c.InvitationCodeExpiry)
                .HasColumnName("InvitationCodeExpiry");

            builder.Property(c => c.IsInvitationCodeUsed)
                .HasColumnName("IsInvitationCodeUsed")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(c => c.PasswordHash)
                .HasColumnName("PasswordHash")
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(c => c.CityId)
                .HasColumnName("CityId")
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
            builder.HasOne(c => c.City)
                .WithMany(c => c.Customers)
                .HasForeignKey(c => c.CityId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.HasIndex(c => new { c.MobileNumber, c.State })
                .HasDatabaseName("IX_Customer_MobileNumber_State");
        }
    }
}

