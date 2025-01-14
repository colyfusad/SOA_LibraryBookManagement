using BorrowingManagementService.Enums;
using BorrowingManagementService.Models;

namespace BorrowingManagementService.DTO
{
    public class BorrowingDTO
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public string CustomerFullName { get; set; }

        public String CCCD { get; set; }

        public List<BorrowingDetailDTO> BorrowingDetails { get; set; }

        public BorrowingStatus Status { get; set; }
        public DateTime BorrowDate { get; set; }

        public DateTime ReturnDate { get; set; }
    }
}
