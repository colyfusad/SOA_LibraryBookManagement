namespace BookManagementService.DTO
{
    public class BookDTO
    {
        public string Title { get; set; }
        public string? Author { get; set; }
        public string? ISBN { get; set; }
        public string? PublishYear { get; set; }
        public int Quanity { get; set; }
        public int CategoryId { get; set; }
    }
}
