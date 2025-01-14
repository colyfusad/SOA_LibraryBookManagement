using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BookManagementService.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]    
        public string Name { get; set; }

        [JsonIgnore]
        public ICollection<Book>? Books { get; set; }
    }
}
