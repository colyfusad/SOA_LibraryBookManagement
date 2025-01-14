using BorrowingManagementService.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BorrowingManagementService.Models
{
    public class Borrowing
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; } // Liên kết với bảng User trong UserManagementService

        [Required]
        public DateTime BorrowDate { get; set; } = DateTime.Now;

        public DateTime ReturnDate { get; set; }

        [Required]
        public BorrowingStatus Status { get; set; } // Trạng thái: Pending, Borrowed, Returned, Canceled

        public ICollection<BorrowingDetail> BorrowingDetails { get; set; }
    }
}
