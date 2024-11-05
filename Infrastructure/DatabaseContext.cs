using Domain.Common;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
            modelBuilder.Entity<Category>().HasData(new Category
            {
                CategoryId = 1,
                CategoryName = "Male weare"
            });
        }

        public  async Task<DbResult> SaveChangesAsyncWithResult(CancellationToken cancellationToken = default)
        {
            try
            {
                // Track changes for auditing
                foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            entry.Entity.CreatedDate = DateTime.Now;
                            break;
                        case EntityState.Modified:
                            entry.Entity.LastModifiedDate = DateTime.Now;
                            break;
                    }
                }

                await base.SaveChangesAsync(cancellationToken);

                return new DbResult { IsSuccess = true };
            }
            catch (Exception exp)
            {
                return new DbResult { IsSuccess = false, ErrorMessage = exp.Message };
            }
        }


    }
}
