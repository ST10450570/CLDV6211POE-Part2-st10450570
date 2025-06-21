using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class EventType
    {
        [Key]
        public int EventTypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string EventTypeName { get; set; }
    }
}