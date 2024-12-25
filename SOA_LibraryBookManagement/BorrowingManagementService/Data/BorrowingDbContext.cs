using Microsoft.EntityFrameworkCore;
using BorrowingManagementService.Models;
using System.Collections.Generic;

namespace BorrowingManagementService.Data
{
    public class BorrowingDbContext : DbContext
    {
        public BorrowingDbContext(DbContextOptions<BorrowingDbContext> options) : base(options) { }

        public DbSet<Borrowing> Borrowings { get; set; }
        public DbSet<BorrowingDetail> BorrowingDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Thiết lập quan hệ Borrowing - BorrowingDetail
            modelBuilder.Entity<Borrowing>()
                        .HasMany(b => b.BorrowingDetails)
                        .WithOne()
                        .HasForeignKey(d => d.BorrowingId)
                        .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
