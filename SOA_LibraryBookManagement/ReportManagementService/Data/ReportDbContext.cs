using Microsoft.EntityFrameworkCore;
using ReportManagementService.Models;
using System.Collections.Generic;

namespace ReportManagementService.Data
{
    public class ReportDbContext: DbContext
    {
        public ReportDbContext(DbContextOptions<ReportDbContext> options) : base(options) { }

        public DbSet<BookQuantityByCategory> GetBookQuantityByCategoryAsync { get; set; }
        
        public int GetTotalBookQuantityAsync { get; set; }

        public DbSet<MostBorrowedBooksReport> MostBorrowedBooksReports { get; set; }
    }
}
