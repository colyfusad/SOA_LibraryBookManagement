namespace BorrowingManagementService.DTO
{
    public class BorrowingDetailDTO
    {
        public int Id {  get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public int Quantity { get; set; }
    }
}
