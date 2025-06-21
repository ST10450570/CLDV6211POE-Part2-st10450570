using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Event Name")]
        public string? EventName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; }

        [Required]
        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Venue")]
        public int VenueId { get; set; }

        [Display(Name = "Event Type")]
        public int? EventTypeId { get; set; }

        [Display(Name = "Event Image")]
        public string? ImageUrl { get; set; }

        // Navigation properties
        [ForeignKey("VenueId")]
        public virtual Venue? Venue { get; set; }

        [ForeignKey("EventTypeId")]
        public virtual EventType? EventType { get; set; }

        public virtual ICollection<Booking>? Bookings { get; set; }
    }
}