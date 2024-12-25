using BorrowingManagementService.Data;
using BorrowingManagementService.DTOs;
using BorrowingManagementService.Interface;
using BorrowingManagementService.Models;
using BorrowingManagementService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BorrowingManagementService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowingController : ControllerBase
    {
        private readonly IBorrowingService _borrowingService;

        public BorrowingController(IBorrowingService borrowingService)
        {
            _borrowingService = borrowingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBorrowings()
        {
            var borrowings = await _borrowingService.GetBorrowingAsync();
            if (borrowings == null || !borrowings.Any())
            {
                return NotFound(new Response
                {
                    Status = "Fail",
                    Message = "No borrowings found.",
                });
            }

            return Ok(new Response
            {
                Status = "Success",
                Message = "Get all borrowings success!",
                Data = FormatBorrowingResponse(borrowings)
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBorrowingById(int id)
        {
            var borrowing = await _borrowingService.GetBorrowingByIdAsync(id);
            if (borrowing == null)
            {
                return NotFound(new Response
                {
                    Status = "Fail",
                    Message = $"Borrowing with ID {id} not found.",
                });
            }

            return Ok(new Response
            {
                Status = "Success",
                Message = $"Get borrowing with ID '{id}' success!",
                Data = FormatBorrowingResponse(borrowing)
            });
        }

        // Cập nhật trạng thái Borrowing
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateBorrowingStatus(int id, [FromBody] BorrowingStatusUpdateDto statusUpdateDto)
        {
            try
            {
                bool result = await _borrowingService.UpdateBorrowingStatusAsync(id, statusUpdateDto.NewStatus);
                if (!result)
                {
                    return NotFound(new Response
                    {
                        Status = "Fail",
                        Message = $"Borrowing with ID {id} not found or invalid transition.",
                    });
                }

                return Ok(new Response
                {
                    Status = "Success",
                    Message = $"Borrowing status updated successfully to {statusUpdateDto.NewStatus}.",
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Xóa Borrowing theo ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBorrowing(int id)
        {
            try
            {
                bool result = await _borrowingService.DeleteBorrowingAsync(id);
                if (!result)
                {
                    return NotFound(new Response
                    {
                        Status = "Fail",
                        Message = $"Borrowing with ID {id} not found.",
                    });
                }

                return Ok(new Response
                {
                    Status = "Success",
                    Message = "Borrowing deleted successfully.",
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateBorrowing([FromBody] BorrowingDto request)
        {
            try
            {
                var borrowing = await _borrowingService.CreateBorrowingAsync(request);

                if (borrowing == null)
                {
                    return NotFound(new Response
                    {
                        Status = "Faild",
                        Message = "Customer not found with CCCD!",
                    });
                }

                return Ok(new Response
                {
                    Status = "Success",
                    Message = "Created borrowing successfully!",
                    Data = FormatBorrowingResponse(borrowing)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new Response
                {
                    Status = "Fail",
                    Message = ex.ToString()
                });
            }
        }

        private object FormatBorrowingResponse(object input)
        {
            if (input is Borrowing borrowing)
            {
                return new
                {
                    borrowing.Id,
                    borrowing.UserId,
                    BorrowDate = borrowing.BorrowDate.ToString("dd/MM/yyyy HH:mm:ss"),
                    ReturnDate = borrowing.ReturnDate.ToString("dd/MM/yyyy HH:mm:ss"),
                    borrowing.Status,
                    borrowing.BorrowingDetails,
                };
            }
            else if (input is IEnumerable<Borrowing> borrowings)
            {
                return borrowings.Select(borrowing => new
                {
                    borrowing.Id,
                    borrowing.UserId,
                    BorrowDate = borrowing.BorrowDate.ToString("dd/MM/yyyy HH:mm:ss"),
                    ReturnDate = borrowing.ReturnDate.ToString("dd/MM/yyyy HH:mm:ss"),
                    borrowing.Status,
                    borrowing.BorrowingDetails,
                }).ToList();
            }

            throw new ArgumentException("Input must be of type Borrowing or IEnumerable<Borrowing>");
        }
    }
}
