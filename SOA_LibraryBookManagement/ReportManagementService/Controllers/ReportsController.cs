using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ReportManagementService.Interface;

namespace ReportManagementService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }
        [HttpGet("total-book-quantity")]
        public async Task<IActionResult> GetTotalBookQuantity()
        {
            try
            {
                var result = await _reportService.GetTotalBookQuantityAsync();
                return Ok(new
                {
                    Status = "Success",
                    Message = "Total book quantity retrieved successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Fail",
                    Message = ex.Message
                });
            }
        }

        [HttpGet("book-quantity-by-category")]
        public async Task<IActionResult> GetBookQuantityByCategory()
        {
            try
            {
                var result = await _reportService.GetBookQuantityByCategoryAsync();
                return Ok(new
                {
                    Status = "Success",
                    Message = "Book quantities by category retrieved successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Fail",
                    Message = ex.Message
                });
            }
        }


        // Endpoint để lấy danh sách sách được mượn nhiều nhất
        [HttpGet("most-borrowed-books")]
        public async Task<IActionResult> GetTopBorrowedBooks(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Gọi phương thức trong service
                var result = await _reportService.GetMostBorrowedBooksAsync(startDate, endDate);
                // Trả về kết quả dưới dạng JSON
                return Ok(new
                {
                    Status = "Success",
                    Message = "Most borrowed books retrieved successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và trả về thông báo lỗi
                return StatusCode(500, new
                {
                    Status = "Fail",
                    Message = ex.Message
                });
            }

        }

    }
}
