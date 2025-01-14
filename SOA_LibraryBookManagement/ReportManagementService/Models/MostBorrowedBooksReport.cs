using System.Text.Json.Serialization;

namespace ReportManagementService.Models
{
    public class MostBorrowedBooksReport
    {
        [JsonPropertyName("id")]
        public int BookId { get; set; } // ID của sách

        [JsonPropertyName("title")]
        public string Title { get; set; } // Tên sách

        [JsonPropertyName("author")]
        public string? Author { get; set; } // Tác giả (nếu có)

        [JsonPropertyName("quanity")]
        public int TotalBorrowed { get; set; } // Tổng số lần mượn
    }
}
