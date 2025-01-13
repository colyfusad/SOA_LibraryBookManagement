namespace ReportManagementService.Models
{
    public class MostBorrowedBooksReport
    {
        public int BookId { get; set; } // ID của sách
        public string Title { get; set; } // Tên sách
        public string? Author { get; set; } // Tác giả (nếu có)
        public int TotalBorrowed { get; set; } // Tổng số lần mượn
    }
}
