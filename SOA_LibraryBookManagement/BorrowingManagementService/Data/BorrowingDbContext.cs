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

            modelBuilder.Entity<Borrowing>()
                       .HasMany(b => b.BorrowingDetails)
                       .WithOne(bd => bd.Borrowing)
                       .HasForeignKey(bd => bd.BorrowingId)
                       .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
