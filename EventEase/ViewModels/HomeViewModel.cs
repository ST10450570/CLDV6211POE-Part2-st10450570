using System.Collections.Generic;
using EventEase.Models;

namespace EventEase.Models
{
    public class HomeViewModel
    {
        public IEnumerable<Event>? FutureEvents { get; set; }
        public IEnumerable<Venue>? FeaturedVenues { get; set; }
    }

   
}
