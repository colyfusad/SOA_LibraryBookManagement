using BorrowingManagementService.Enums;
using BorrowingManagementService.Models;
using Microsoft.EntityFrameworkCore;
using BorrowingManagementService.Data;
using BorrowingManagementService.DTOs;
using Azure.Core;
using BorrowingManagementService.Interface;

namespace BorrowingManagementService.Services
{
    public class BorrowingService: IBorrowingService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly BorrowingDbContext _context;

        public BorrowingService(IHttpClientFactory httpClientFactory, BorrowingDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        public async Task<bool> CheckBookAvailability(int bookId, int quantity)
        {
            var client = _httpClientFactory.CreateClient("BookManagementService");
            var response = await client.GetAsync($"Books/{bookId}");

            if (response.IsSuccessStatusCode)
            {
                var book = await response.Content.ReadFromJsonAsync<Book>();
                return book != null && book.Quanity >= quantity;
            }

            return false;
        }

        public async Task<Customer?> GetCustomerByCCCD(String cccd)
        {
            var client = _httpClientFactory.CreateClient("CustomerManagementService");
            var response = await client.GetAsync($"Customers/by-cccd/{cccd}");

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadFromJsonAsync<ResponseModel<Customer>>();
                return apiResponse?.Data; 
            }
            return null;
        }

        public async Task<Borrowing?> CreateBorrowingAsync(BorrowingDto borrowingDto)
        {
            // Tạo danh sách BorrowingDetail từ BorrowingDetailInputDto
            var borrowingDetails = borrowingDto.BorrowingDetails.Select(detail => new BorrowingDetail
            {
                BookId = detail.BookId,
                Quantity = detail.Quantity
                // BorrowingId sẽ được EF tự động thêm sau
            }).ToList();

            foreach (var detail in borrowingDetails)
            {
                bool isAvailable = await CheckBookAvailability(detail.BookId, detail.Quantity);
                if (!isAvailable)
                {
                    throw new Exception($"Book ID {detail.BookId} is not available in the required quantity.");
                }
            }

            var customer = await GetCustomerByCCCD(borrowingDto.CCCD);
            if (customer == null)
            {
                return null;
            }

            var borrowing = new Borrowing
            {
                UserId = customer.Id,
                BorrowDate = DateTime.Now,
                ReturnDate = borrowingDto.ReturnDate,
                Status = BorrowingStatus.Pending,
                BorrowingDetails = borrowingDetails
            };

            _context.Borrowings.Add(borrowing);
            await _context.SaveChangesAsync();

            return borrowing;
        }

        public async Task<bool> UpdateBorrowingStatusAsync(int borrowingId, BorrowingStatus newStatus)
        {
            var borrowing = await _context.Borrowings
                .Include(b => b.BorrowingDetails)
                .FirstOrDefaultAsync(b => b.Id == borrowingId);

            if (borrowing == null)
            {
                return false;
            }

            // Kiểm tra xem trạng thái mới có hợp lệ không
            if (!IsValidStatusTransition(borrowing.Status, newStatus))
            {
                throw new Exception($"Cannot transition from {borrowing.Status} to {newStatus}.");
            }

            if (newStatus == BorrowingStatus.Borrowed)
            {
                // Giảm số lượng sách trong kho
                foreach (var detail in borrowing.BorrowingDetails)
                {
                    bool success = await UpdateBookQuantity(detail.BookId, -detail.Quantity);
                    if (!success)
                    {
                        throw new Exception($"Failed to update book quantity for Book ID {detail.BookId}.");
                    }
                }
            }
            else if (newStatus == BorrowingStatus.Returned)
            {
                // Tăng số lượng sách trong kho
                foreach (var detail in borrowing.BorrowingDetails)
                {
                    bool success = await UpdateBookQuantity(detail.BookId, detail.Quantity);
                    if (!success)
                    {
                        throw new Exception($"Failed to update book quantity for Book ID {detail.BookId}.");
                    }
                }
            }

            borrowing.Status = newStatus;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Borrowing>?> GetBorrowingAsync()
        {
            var borrowing = await _context.Borrowings
                .Include(b => b.BorrowingDetails)
                .ToListAsync();

            return borrowing;
        }

        public async Task<Borrowing?> GetBorrowingByIdAsync(int borrowingId)
        {
            var borrowing = await _context.Borrowings
                .Include(b => b.BorrowingDetails)
                .FirstOrDefaultAsync(b => b.Id == borrowingId);

            return borrowing;
        }

        public async Task<bool> DeleteBorrowingAsync(int borrowingId)
        {
            var borrowing = await _context.Borrowings.FindAsync(borrowingId);

            if (borrowing == null)
            {
                return false; 
            }

            if (borrowing.Status == BorrowingStatus.Borrowed)
            {
                throw new Exception("Cannot delete a borrowing that is already returned or canceled.");
            }

            _context.Borrowings.Remove(borrowing);
            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<bool> UpdateBookQuantity(int bookId, int quantityChange)
        {
            var client = _httpClientFactory.CreateClient("BookManagementService");

            var requestContent = new
            {
                BookId = bookId,
                QuantityChange = quantityChange
            };

            var response = await client.PutAsJsonAsync($"Books/{bookId}/quantity", requestContent);

            return response.IsSuccessStatusCode;
        }

        private bool IsValidStatusTransition(BorrowingStatus currentStatus, BorrowingStatus newStatus)
        {
            switch (currentStatus)
            {
                case BorrowingStatus.Pending:
                    return newStatus == BorrowingStatus.Borrowed || newStatus == BorrowingStatus.Returned || newStatus == BorrowingStatus.Canceled;
                case BorrowingStatus.Borrowed:
                    return newStatus == BorrowingStatus.Returned || newStatus == BorrowingStatus.Canceled;
                case BorrowingStatus.Returned:
                    return newStatus == BorrowingStatus.Canceled;
                default:
                    return false;
            }
        }

    }
}
