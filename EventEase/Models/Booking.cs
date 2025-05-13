using EventEase.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public int VenueId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Booking Date")]
        public DateTime BookingDate { get; set; }

        // Make sure Status is properly defined
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Default value

        // Navigation properties
        [ForeignKey("EventId")]
        public virtual Event? Event { get; set; }

        [ForeignKey("VenueId")]
        public virtual Venue? Venue { get; set; }
    }
}