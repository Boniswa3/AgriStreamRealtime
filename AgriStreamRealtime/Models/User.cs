using System.ComponentModel.DataAnnotations;

namespace AgriStreamRealtime.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public int RoleId { get; set; }

        public Role? Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}