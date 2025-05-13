using EventEase.Models;
using Microsoft.AspNetCore.Mvc.Rendering; // Add this namespace
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; // Ensure this is present

namespace EventEase.ViewModels
{
    public class EventViewModel
    {
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

        [Display(Name = "Event Image")]
        public IFormFile? ImageFile { get; set; }

        public string? ExistingImageUrl { get; set; }

        public IEnumerable<SelectListItem>? VenueList { get; set; }
    }
}