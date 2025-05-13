using System.Collections.Generic;
using EventEase.Models;

namespace EventEase.Models
{
    public class HomeViewModel
    {
        public IEnumerable<Event>? UpcomingEvents { get; set; }
        public IEnumerable<Venue>? FeaturedVenues { get; set; }
    }

   
}
