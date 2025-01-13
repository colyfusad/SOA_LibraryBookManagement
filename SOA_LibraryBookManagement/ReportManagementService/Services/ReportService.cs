using ReportManagementService.Data;
using ReportManagementService.Interface;
using ReportManagementService.Models;

namespace ReportManagementService.Services
{
    public class ReportService : IReportService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ReportDbContext _context;

        public ReportService(IHttpClientFactory httpClientFactory, ReportDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }
        
        public async Task<IEnumerable<MostBorrowedBooksReport>> GetMostBorrowedBooksAsync(DateTime startDate, DateTime endDate)
        {
            var client = _httpClientFactory.CreateClient("BorrowingManagementService");
            string query = $"Borrowings/top-borrowed-books?startDate={startDate}&endDate={endDate}";

            int m = 6;
            var response = await client.GetAsync(query);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                // Ghi log chi tiết lỗi
                Console.WriteLine($"Error: {response.StatusCode}, Content: {errorContent}");
                throw new Exception($"Failed to retrieve top borrowed books. Status: {response.StatusCode}, Message: {errorContent}");
            }

            // Đọc dữ liệu trả về từ BorrowingManagementService
            var topBorrowedBooks = await response.Content.ReadFromJsonAsync<IEnumerable<MostBorrowedBooksReport>>();
            if (topBorrowedBooks == null)
            {
                return Enumerable.Empty<MostBorrowedBooksReport>();
            }

            return topBorrowedBooks;
        }

        public async Task<int> GetTotalBookQuantityAsync()
        {
            var client = _httpClientFactory.CreateClient("BookManagementService");
            var response = await client.GetAsync("Books/total-quantity");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {response.StatusCode}, Content: {errorContent}");
                throw new Exception($"Failed to retrieve total book quantity. Status: {response.StatusCode}, Message: {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<Response<int>>();
            return result?.Data ?? 0; // Trả về giá trị tổng số lượng sách
        }

        public async Task<IEnumerable<BookQuantityByCategory>> GetBookQuantityByCategoryAsync()
        {
            var client = _httpClientFactory.CreateClient("BookManagementService");
            var response = await client.GetAsync("Books/quantity-by-category");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {response.StatusCode}, Content: {errorContent}");
                throw new Exception($"Failed to retrieve book quantities by category. Status: {response.StatusCode}, Message: {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<Response<IEnumerable<BookQuantityByCategory>>>();
            return result?.Data ?? Enumerable.Empty<BookQuantityByCategory>(); // Trả về số lượng sách theo thể loại
        }

    }
}
