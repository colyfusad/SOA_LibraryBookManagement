using ReportManagementService.Models;

namespace ReportManagementService.Interface
{
    public interface IReportService
    {
        /// <summary>
        /// Lấy danh sách sách được mượn nhiều nhất trong một khoảng thời gian.
        /// </summary>
        /// <param name="startDate">Ngày bắt đầu (optional, mặc định là từ trước đến nay).</param>
        /// <param name="endDate">Ngày kết thúc (optional, mặc định là hiện tại).</param>
        /// <param name="top">Số lượng sách cần lấy (optional, mặc định là top 10).</param>
        /// <returns>Danh sách các sách được mượn nhiều nhất.</returns>
        Task<IEnumerable<MostBorrowedBooksReport>> GetMostBorrowedBooksAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<BookQuantityByCategory>> GetBookQuantityByCategoryAsync();
        Task<int> GetTotalBookQuantityAsync();
    }
}
