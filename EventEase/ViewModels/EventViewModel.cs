using EventEase.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

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

        [Display(Name = "Event Type")]
        public int? EventTypeId { get; set; }

        [Display(Name = "Event Image")]
        public IFormFile? ImageFile { get; set; }

        public string? ExistingImageUrl { get; set; }

        public IEnumerable<SelectListItem>? VenueList { get; set; }
        public IEnumerable<SelectListItem>? EventTypeList { get; set; }
    }
}