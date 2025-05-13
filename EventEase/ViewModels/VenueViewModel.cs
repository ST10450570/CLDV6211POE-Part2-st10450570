using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EventEase.ViewModels
{
    public class VenueViewModel
    {
        public int VenueId { get; set; }

        [Required]
        public string? VenueName { get; set; }

        [Required]
        public string? Location { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0")]
        public int Capacity { get; set; }

        public string? ExistingImageUrl { get; set; }

        [Display(Name = "Venue Image")]
        public IFormFile? ImageFile { get; set; }  // ✅ This is required for binding the uploaded image
    }
}
