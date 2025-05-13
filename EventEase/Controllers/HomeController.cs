using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using EventEase.Models;

namespace EventEase.Controllers
{
    public class HomeController : Controller
    {
        private readonly EventEaseDBContext _context;

        public HomeController(EventEaseDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Ensure future events only and include venue info
            var upcomingEvents = await _context.Events
                .Include(e => e.Venue)
                .Where(e => e.EventDate >= DateTime.Today)
                .OrderBy(e => e.EventDate)
                .Take(5)
                .ToListAsync();

            // Get random featured venues or top 5 venues
            var featuredVenues = await _context.Venues
                .OrderBy(v => Guid.NewGuid())
                .Take(5)
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                UpcomingEvents = upcomingEvents,
                FeaturedVenues = featuredVenues
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
