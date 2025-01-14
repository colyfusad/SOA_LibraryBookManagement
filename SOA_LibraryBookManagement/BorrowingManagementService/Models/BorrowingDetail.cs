using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BorrowingManagementService.Models
{
    public class BorrowingDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BorrowingId { get; set; } // Liên kết với bảng Borrowing

        [Required]
        public int BookId { get; set; }             // Liên kết với bảng Book trong BookManagementService

        [Required]
        [Range(1, 5, ErrorMessage = "You can borrow between 1 and 5 bo oks.")]
        public int Quantity { get; set; }

        public Borrowing Borrowing { get; set; }
    }
}
