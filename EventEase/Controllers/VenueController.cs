using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;
using EventEase.ViewModels;


namespace EventEase.Controllers
{
    public class VenueController : Controller
    {
        private readonly EventEaseDBContext _context;
        private readonly IImageService _imageService;

        public VenueController(EventEaseDBContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        // GET: Venues
        public async Task<IActionResult> Index()
        {
            return View(await _context.Venues.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Index(string search)
        {
            var venues = from v in _context.Venues
                         select v;

            if (!string.IsNullOrEmpty(search))
            {
                venues = venues.Where(v =>
                    v.VenueName.Contains(search) ||
                    v.Location.Contains(search));
            }

            return View(await venues.ToListAsync());
        }

        // GET: Venues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .Include(v => v.Events)
                .FirstOrDefaultAsync(m => m.VenueId == id);

            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // GET: Venues/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Venues/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VenueViewModel venueViewModel)
        {
            if (ModelState.IsValid)
            {
                var venue = new Venue
                {
                    VenueName = venueViewModel.VenueName,
                    Location = venueViewModel.Location,
                    Capacity = venueViewModel.Capacity
                };

                // Initially, use placeholder URL
                venue.ImageUrl = "/images/placeholder-venue.jpg";

                // In Part 2, we'll replace with:
                 if (venueViewModel.ImageFile != null)
                 {
                     venue.ImageUrl = await _imageService.UploadImageAsync(venueViewModel.ImageFile, "venue");
                 }

                _context.Add(venue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(venueViewModel);
        }

        // GET: Venues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }

            var venueViewModel = new VenueViewModel
            {
                VenueId = venue.VenueId,
                VenueName = venue.VenueName,
                Location = venue.Location,
                Capacity = venue.Capacity,
                ExistingImageUrl = venue.ImageUrl
            };

            return View(venueViewModel);
        }

        // POST: Venues/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VenueViewModel venueViewModel)
        {
            if (id != venueViewModel.VenueId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var venue = await _context.Venues.FindAsync(id);

                    venue.VenueName = venueViewModel.VenueName;
                    venue.Location = venueViewModel.Location;
                    venue.Capacity = venueViewModel.Capacity;

                    // In Part 2, we'll use:
                     if (venueViewModel.ImageFile != null)
                     {
                         // Delete old image if it exists and is not the placeholder
                         if (!string.IsNullOrEmpty(venue.ImageUrl) && !venue.ImageUrl.Contains("placeholder"))
                         {
                             await _imageService.DeleteImageAsync(venue.ImageUrl);
                         }
                         venue.ImageUrl = await _imageService.UploadImageAsync(venueViewModel.ImageFile, "venue");
                     }

                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venueViewModel.VenueId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(venueViewModel);
        }

        // GET: Venues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueId == id);

            if (venue == null)
            {
                return NotFound();
            }

            // Part 2: Check if venue has active bookings
            var hasActiveBookings = await _context.Bookings
                .AnyAsync(b => b.VenueId == id && b.Event.EventDate >= DateTime.Today);

            if (hasActiveBookings)
            {
                ViewBag.ErrorMessage = "This venue cannot be deleted because it has active bookings.";
            }

            return View(venue);
        }

        // POST: Venues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues.FindAsync(id);

            // Part 2: Check if venue has active bookings
            var hasAnyBookings = await _context.Bookings
     .AnyAsync(b => b.VenueId == id);

            if (hasAnyBookings)
            {
                ModelState.AddModelError(string.Empty, "Cannot delete this venue because it is associated with bookings.");
                return View(venue);
            }


            // In Part 2, we'll add:
            if (!string.IsNullOrEmpty(venue.ImageUrl) && !venue.ImageUrl.Contains("placeholder"))
             {
                 await _imageService.DeleteImageAsync(venue.ImageUrl);
             }

            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venues.Any(e => e.VenueId == id);
        }
    }
}