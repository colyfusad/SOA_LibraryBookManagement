using System.ComponentModel.DataAnnotations;

namespace CustomerManagementService.DTOs
{
    public class CustomerDto
    {
        [Required]
        [StringLength(12, ErrorMessage = "CCCD must be 12 digits.")]
        public string CCCD { get; set; }

        [Required]
        public string FullName { get; set; }

        public string? StudentId { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PhoneNumber { get; set; }
    }
}
