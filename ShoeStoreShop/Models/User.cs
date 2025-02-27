using System.ComponentModel.DataAnnotations;

namespace ShoeStore.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }  // Mã hóa trước khi lưu

        [StringLength(100)]
        public string FullName { get; set; }

        [StringLength(15)]
        public string? PhoneNumber { get; set; }

        [StringLength(20)]
        public string Role { get; set; } = "User";

        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }
    }
}
