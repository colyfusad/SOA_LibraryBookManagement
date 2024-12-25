using BorrowingManagementService.Models;

namespace BorrowingManagementService.DTOs
{
    public class BorrowingDto
    {
        public String CCCD { get; set; }
        public List<BorrowingDetailDto> BorrowingDetails { get; set; }

        public DateTime ReturnDate { get; set; }
    }
}
