using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Venue
    {
        [Key]
        public int VenueId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Venue Name")]
        public string? VenueName { get; set; }

        [Required]
        [StringLength(200)]
        public string? Location { get; set; }

        [Range(1, 100000)]
        public int Capacity { get; set; }

        [Display(Name = "Venue Image")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Available")]
        public bool Availability { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Event>? Events { get; set; }
        public virtual ICollection<Booking>? Bookings { get; set; }
    }
}