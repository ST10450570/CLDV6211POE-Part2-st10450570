using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;


namespace EventEase.Controllers
{
    public class BookingController : Controller
    {
        private readonly EventEaseDBContext _context;

        public BookingController(EventEaseDBContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .ToListAsync();

            return View(bookings);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm)
        {
            var bookingsQuery = _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                bookingsQuery = bookingsQuery.Where(b =>
                    b.BookingId.ToString().Contains(searchTerm) ||
                    b.Event.EventName.Contains(searchTerm));
            }

            return View("Index", await bookingsQuery.ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) // Null check added here
            {
                return NotFound();
            }

            return View(booking); // 'booking' is now guaranteed to be not null here
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName");
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName");
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,EventId,VenueId,BookingDate")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate bookings
                bool duplicateExists = await _context.Bookings
                    .AnyAsync(b => b.VenueId == booking.VenueId &&
                                  b.BookingDate.Date == booking.BookingDate.Date);

                if (duplicateExists)
                {
                    ModelState.AddModelError(string.Empty, "This venue is already booked for the specified date.");
                    ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                    ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
                    return View(booking);
                }

                // Set default status when creating a booking
                booking.Status = "Active"; // Default status

                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null) // Null check added here
            {
                return NotFound();
            }

            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            return View(booking); // 'booking' is now guaranteed to be not null here
        }


        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,EventId,VenueId,BookingDate")] Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Part 2: Check for duplicate bookings (excluding this one)
                    bool duplicateExists = await _context.Bookings
                        .AnyAsync(b => b.BookingId != booking.BookingId &&
                                      b.VenueId == booking.VenueId &&
                                      b.BookingDate.Date == booking.BookingDate.Date);

                    if (duplicateExists)
                    {
                        ModelState.AddModelError(string.Empty, "This venue is already booked for the specified date.");
                        ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                        ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
                        return View(booking);
                    }

                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
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

            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // GET: Bookings/Delete/5
       
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) // Null check added here
            {
                return NotFound();
            }

            // Check if booking is active (future date and status is Active)
            if (booking.BookingDate >= DateTime.Today && booking.Status == "Active")
            {
                ViewBag.ErrorMessage = "This booking cannot be deleted because it is active (future date).";
            }

            return View(booking); // 'booking' is now guaranteed to be not null here
        }


        
        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null) // Null check added here
            {
                return NotFound(); // Or handle the case where the booking doesn't exist
            }

            // Prevent deletion of active (future) bookings
            if (booking.BookingDate >= DateTime.Today && booking.Status == "Active")
            {
                ModelState.AddModelError(string.Empty, "Cannot delete active (future) bookings.");
                return RedirectToAction("Delete", new { id = id });
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // Part 2: Search functionality
        public async Task<IActionResult> Search(string searchTerm, DateTime? startDate, DateTime? endDate, int? venueId, string status)
        {
            var bookingsQuery = _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                bookingsQuery = bookingsQuery.Where(b =>
                    b.Event.EventName.Contains(searchTerm) ||
                    b.Venue.VenueName.Contains(searchTerm));
            }

            if (startDate.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.BookingDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.BookingDate <= endDate.Value);
            }

            if (venueId.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.VenueId == venueId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                bookingsQuery = bookingsQuery.Where(b => b.Status == status);
            }

            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", venueId);
            ViewData["SearchTerm"] = searchTerm;
            ViewData["StartDate"] = startDate;
            ViewData["EndDate"] = endDate;
            ViewData["Status"] = status;

            return View("Index", await bookingsQuery.ToListAsync());
        }


        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}