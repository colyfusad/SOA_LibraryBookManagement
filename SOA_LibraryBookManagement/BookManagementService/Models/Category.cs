using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookManagementService.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]    
        public string Name { get; set; }

        public ICollection<Book> Books { get; set; }
    }
}
