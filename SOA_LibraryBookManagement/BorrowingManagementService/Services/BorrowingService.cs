using BorrowingManagementService.Enums;
using BorrowingManagementService.Models;
using Microsoft.EntityFrameworkCore;
using BorrowingManagementService.Data;
using BorrowingManagementService.DTO;
using Azure.Core;
using BorrowingManagementService.Interface;
using System.Text.Json;

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
                var book = await response.Content.ReadFromJsonAsync<ResponseModel<BookDTO>>();
                return book != null && book.Data.Quanity >= quantity;
            }

            return false;
        }

        public async Task<CustomerDTO?> GetCustomerByCCCD(String cccd)
        {
            var client = _httpClientFactory.CreateClient("CustomerManagementService");
            var response = await client.GetAsync($"Customers/by-cccd/{cccd}");

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadFromJsonAsync<ResponseModel<CustomerDTO>>();
                return apiResponse?.Data; 
            }
            return null;
        }

        public async Task<CustomerDTO?> GetCustomerById(int id)
        {
            var client = _httpClientFactory.CreateClient("CustomerManagementService");
            var response = await client.GetAsync($"Customers/{id}");

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadFromJsonAsync<ResponseModel<CustomerDTO>>();
                return apiResponse?.Data;
            }
            return null;
        }

        public async Task<BookDTO?> GetBookById(int id)
        {
            var client = _httpClientFactory.CreateClient("BookManagementService");
            var response = await client.GetAsync($"Books/{id}");

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadFromJsonAsync<ResponseModel<BookDTO>>();
                return apiResponse?.Data;
            }
            return null;
        }

        public async Task<Borrowing?> CreateBorrowingAsync(CreateBorrowingDTO borrowingDto)
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
                CustomerId = customer.Id,
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

        public async Task<List<BorrowingDTO>> GetBorrowingAsync()
        {
            var borrowings = await _context.Borrowings
                .Include(b => b.BorrowingDetails)
                .ToListAsync();

            var result = new List<BorrowingDTO>();

            foreach (var borrowing in borrowings)
            {
                CustomerDTO customer = await GetCustomerById(borrowing.CustomerId);
                var borrowingDTO = new BorrowingDTO
                {
                    Id = borrowing.Id,
                    CCCD = customer.CCCD,
                    CustomerFullName = customer?.FullName,
                    Status = borrowing.Status,
                    BorrowDate = borrowing.BorrowDate,
                    ReturnDate = borrowing.ReturnDate,
                    BorrowingDetails = new List<BorrowingDetailDTO>()
                };

                foreach (var detail in borrowing.BorrowingDetails)
                {
                    var book = await GetBookById(detail.BookId);
                    borrowingDTO.BorrowingDetails.Add(new BorrowingDetailDTO
                    {
                        BookId = detail.BookId,
                        BookTitle = book?.Title,
                        Quantity = detail.Quantity
                    });
                }

                result.Add(borrowingDTO);
            }

            return result;
        }


        public async Task<BorrowingDTO?> GetBorrowingByIdAsync(int borrowingId)
        {
            // Lấy thông tin Borrowing từ database
            var borrowing = await _context.Borrowings
                .Include(b => b.BorrowingDetails)
                .FirstOrDefaultAsync(b => b.Id == borrowingId);

            if (borrowing == null)
            {
                return null;
            }

            // Lấy thông tin khách hàng qua GetCustomerById
            var customer = await GetCustomerById(borrowing.CustomerId);
            if (customer == null)
            {
                throw new Exception($"Customer with ID {borrowing.CustomerId} not found.");
            }

            // Lấy thông tin chi tiết sách
            var borrowingDetails = new List<BorrowingDetailDTO>();
            foreach (var detail in borrowing.BorrowingDetails)
            {
                var book = await GetBookById(detail.BookId);
                if (book == null)
                {
                    throw new Exception($"Book with ID {detail.BookId} not found.");
                }

                borrowingDetails.Add(new BorrowingDetailDTO
                {
                    BookId = detail.BookId,
                    BookTitle = book.Title,
                    Quantity = detail.Quantity
                });
            }

            // Tạo BorrowingDTO
            var result = new BorrowingDTO
            {
                Id = borrowing.Id,
                CCCD = customer.CCCD,
                CustomerFullName = customer.FullName,
                Status = borrowing.Status,
                BorrowDate = borrowing.BorrowDate,
                ReturnDate = borrowing.ReturnDate,
                BorrowingDetails = borrowingDetails
            };

            return result;
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

        public async Task<IEnumerable<BookDTO>> GetTopBorrowedBooksAsync(DateTime startDate, DateTime endDate)
        {
            if (endDate == DateTime.MinValue)
            {
                endDate = DateTime.MaxValue;
            }

            // Lấy danh sách top sách được mượn nhiều nhất
            var topBorrowedBooks = await _context.BorrowingDetails
                .Include(bd => bd.Borrowing)
                .Where(bd => bd.Borrowing.BorrowDate >= startDate && bd.Borrowing.BorrowDate <= endDate && bd.Borrowing.Status != BorrowingStatus.Pending)
                .GroupBy(bd => bd.BookId)
                .Select(group => new
                {
                    BookId = group.Key,
                    BorrowedCount = group.Count() // Đếm số lần xuất hiện của BookId
                })
                .OrderByDescending(b => b.BorrowedCount)
                .Take(10)
                .ToListAsync();

            var client = _httpClientFactory.CreateClient("BookManagementService");

            // Tạo danh sách chi tiết sách
            var result = new List<BookDTO>();

            foreach (var book in topBorrowedBooks)
            {
                // Gọi API từ BookManagementService để lấy thông tin chi tiết sách
                var response = await client.GetAsync($"Books/{book.BookId}");
                if (response.IsSuccessStatusCode)
                {
                        var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
                        if (jsonResponse.TryGetProperty("data", out var bookData))
                        {
                            result.Add(new BookDTO
                            {
                                Id = book.BookId,
                                Title = bookData.GetProperty("title").GetString(),
                                Author = bookData.GetProperty("author").GetString(),
                                Quanity = book.BorrowedCount
                            });
                        }
                }
            }
            return result;
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
