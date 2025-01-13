
using BorrowingManagementService.DTOs;
using BorrowingManagementService.Enums;
using BorrowingManagementService.Models;

namespace BorrowingManagementService.Interface
{
    public interface IBorrowingService
    {
        Task<bool> CheckBookAvailability(int bookId, int quantity);
        Task<Customer?> GetCustomerByCCCD(String cccd);
        Task<Borrowing?> CreateBorrowingAsync(BorrowingDto borrowingDto);
        // Thêm phương thức để lấy tất cả Borrowing
        Task<IEnumerable<Borrowing>?> GetBorrowingAsync();

        // Thêm phương thức để lấy Borrowing theo ID
        Task<Borrowing?> GetBorrowingByIdAsync(int borrowingId);

        // Thêm phương thức để cập nhật trạng thái Borrowing
        Task<bool> UpdateBorrowingStatusAsync(int borrowingId, BorrowingStatus newStatus);

        // Thêm phương thức để xóa Borrowing
        Task<bool> DeleteBorrowingAsync(int borrowingId);
        Task<IEnumerable<Book>> GetTopBorrowedBooksAsync(DateTime start, DateTime to);
    }
}
