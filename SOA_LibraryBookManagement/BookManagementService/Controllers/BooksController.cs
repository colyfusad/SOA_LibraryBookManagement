using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookManagementService.Data;
using BookManagementService.Models;
using BookManagementService.DTO;
using Microsoft.AspNetCore.Authorization;

namespace BookManagementService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BooksController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            var books = await _context.Books.Include(b => b.Category).ToListAsync();

            return Ok(new Response
            {
                Status = "Success",
                Message = "Books retrieved successfully",
                Data = FormatBookResponse(books)
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetBook(int id)
        {
            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return FormatBookResponse(book);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(int id, BookDTO bookDto)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound(new Response
                {
                    Status = "Fail",
                    Message = "Id not found"
                });

            book.Title = bookDto.Title;
            book.Author = bookDto.Author;
            book.ISBN = bookDto.ISBN;
            book.PublishYear = bookDto.PublishYear;
            book.Quanity = bookDto.Quanity;
            book.UpdateAt = DateTime.Now;
            book.CategoryId = bookDto.CategoryId;

            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                return Ok(new Response
                {
                    Status = "Success",
                    Message = "Book updated successfully",
                    Data = FormatBookResponse(book)
                });
            }

            return StatusCode(500, new Response
            {
                Status = "Fail",
                Message = "Failed to update book"
            });
        }

        [HttpPost]
        public async Task<ActionResult<Book>> PostBook(BookDTO bookDto)
        {
            var book = new Book
            {
                Title = bookDto.Title,
                Author = bookDto.Author,
                ISBN = bookDto.ISBN,
                PublishYear = bookDto.PublishYear,
                Quanity = bookDto.Quanity,
                CategoryId = bookDto.CategoryId,
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now
            };
            _context.Books.Add(book);
            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                return Ok(new Response
                {
                    Status = "Success",
                    Message = "Book is created!",
                    Data = FormatBookResponse(book)
                });
            }

            return StatusCode(500, new Response
            {
                Status = "Fail",
                Message = "Failed to created book"
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound(new Response
                {
                    Status = "Fail",
                    Message  = "IdBook NotFound in system"
                });
            }

            _context.Books.Remove(book);
            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                return Ok(new Response
                {
                    Status = "Success",
                    Message = "Book is deleted!"
                });
            }

            return StatusCode(500, new Response
            {
                Status = "Fail",
                Message = "Failed to created book"
            });
        }

        [HttpPut("{bookId}/quantity")]
        public async Task<IActionResult> UpdateBookQuantity(int bookId, [FromBody] UpdateBookQuantityDto request)
        {
            if (bookId != request.BookId)
            {
                return BadRequest(new Response 
                { 
                    Status = "Fail",
                    Message = "Book ID in URL does not match ID in request body." 
                });
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null)
            {
                return NotFound(new Response
                {
                    Status = "Fail",
                    Message = $"Book with ID {bookId} not found." 
                });
            }

            // Cập nhật số lượng sách
            book.Quanity += request.QuantityChange;

            // Kiểm tra số lượng sách có hợp lệ hay không
            if (book.Quanity < 0)
            {
                return BadRequest(new Response
                {
                    Status = "Fail",
                    Message = "Book quantity cannot be negative." 
                });
            }

            _context.Books.Update(book);
            await _context.SaveChangesAsync();

            return Ok(new Response
            {
                Status = "Success",
                Message = "Book quantity updated successfully."
            });
        }

        [HttpGet("total-quantity")]
        public async Task<IActionResult> GetTotalBookQuantity()
        {
            var totalQuantity = await _context.Books.CountAsync();

            return Ok(new Response
            {
                Status = "Success",
                Message = "Total book quantity retrieved successfully.",
                Data = totalQuantity
            });
        }

        [HttpGet("quantity-by-category")]
        public async Task<IActionResult> GetBookQuantityByCategory()
        {
            var quantitiesByCategory = await _context.Books
                .GroupBy(b => b.Category)
                .Select(group => new
                {
                    CategoryName = group.Key.Name,
                    BookCount = group.Count() // Đếm số lượng sách theo từng thể loại
                })
                .ToListAsync();

            return Ok(new Response
            {
                Status = "Success",
                Message = "Book quantities by category retrieved successfully.",
                Data = quantitiesByCategory
            });
        }


        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStockBooks()
        {
            var lowStockBooks = await _context.Books
                .Where(b => b.Quanity <= 2)
                .Include(b => b.Category)
                .Select(b => new
                {
                    b.Id,
                    b.Title,
                    b.Author,
                    b.ISBN,
                    b.PublishYear,
                    b.Quanity,
                    CategoryName = b.Category.Name
                })
                .ToListAsync();

            return Ok(new Response
            {
                Status = "Success",
                Message = "Low-stock books retrieved successfully.",
                Data = lowStockBooks
            });
        }

        private object FormatBookResponse(object input)
        {
            if (input is Book book)
            {
                return new
                {
                    book.Id,
                    book.Title,
                    book.Author,
                    book.ISBN,
                    book.PublishYear,
                    book.Quanity,
                    book.CategoryId,
                    CreatAt = book.CreateAt.ToString("dd/MM/yyyy HH:mm:ss"),
                    UpdateAt = book.UpdateAt.ToString("dd/MM/yyyy HH:mm:ss"),
                    CategoryName = book.Category?.Name
                };
            }
            else if (input is IEnumerable<Book> books)
            {
                return books.Select(b => new
                {
                    b.Id,
                    b.Title,
                    b.Author,
                    b.ISBN,
                    b.PublishYear,
                    b.Quanity,
                    b.CategoryId,
                    CreatAt = b.CreateAt.ToString("dd/MM/yyyy HH:mm:ss"),
                    UpdateAt = b.UpdateAt.ToString("dd/MM/yyyy HH:mm:ss"),
                    CategoryName = b.Category?.Name
                }).ToList();
            }

            throw new ArgumentException("Input must be of type Book or IEnumerable<Book>");
        }

    }
}
