
using BorrowingManagementService.DTO;
using BorrowingManagementService.Enums;
using BorrowingManagementService.Models;

namespace BorrowingManagementService.Interface
{
    public interface IBorrowingService
    {
        Task<bool> CheckBookAvailability(int bookId, int quantity);
        Task<CustomerDTO?> GetCustomerByCCCD(String cccd);
        Task<Borrowing?> CreateBorrowingAsync(CreateBorrowingDTO borrowingDto);
        // Thêm phương thức để lấy tất cả Borrowing
        Task<List<BorrowingDTO>> GetBorrowingAsync();

        // Thêm phương thức để lấy Borrowing theo ID
        public Task<BorrowingDTO?> GetBorrowingByIdAsync(int borrowingId);

        // Thêm phương thức để cập nhật trạng thái Borrowing
        Task<bool> UpdateBorrowingStatusAsync(int borrowingId, BorrowingStatus newStatus);

        // Thêm phương thức để xóa Borrowing
        Task<bool> DeleteBorrowingAsync(int borrowingId);
        Task<IEnumerable<BookDTO>> GetTopBorrowedBooksAsync(DateTime start, DateTime to);
    }
}
