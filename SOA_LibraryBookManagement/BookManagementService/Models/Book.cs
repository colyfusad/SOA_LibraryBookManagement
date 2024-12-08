using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookManagementService.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        public string? Author { get; set; }

        public string? ISBN { get; set; }

        public string? PublishYear { get; set; }

        [Required] 
        [Range(0, 13, ErrorMessage = "Quanity must be a positive integer.")]
        public int Quanity { get; set; }
        
        public int CategoryId { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdateAt { get; set; } = DateTime.UtcNow;

        public Category Category { get; set; }
    }
}
