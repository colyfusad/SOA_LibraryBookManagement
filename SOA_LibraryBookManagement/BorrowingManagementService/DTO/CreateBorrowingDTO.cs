namespace BorrowingManagementService.DTO
{
    public class CreateBorrowingDTO
    {
        public String CCCD { get; set; }

        public List<CreateBorrowingDetailDTO> BorrowingDetails { get; set; }

        public DateTime ReturnDate { get; set; }
    }
}
