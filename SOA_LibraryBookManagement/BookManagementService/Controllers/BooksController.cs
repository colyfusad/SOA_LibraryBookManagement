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
            return await _context.Books.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            return book;
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
            book.CategoryId = bookDto.CategoryId;

            var affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                return Ok(new Response
                {
                    Status = "Success",
                    Message = "Book updated successfully",
                    Data = book
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
                    Data = book
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
                return NotFound();
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
    }
}
