using System.ComponentModel.DataAnnotations;

namespace AgriStreamRealtime.Models
{
    // Represents service offered by AgriStreamRealtime to their customers.
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        [Required]
        [StringLength(200)]
        public string ServiceTitle { get; set; } = string.Empty;

        //
        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string? CoverImageUrl { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    }
}

