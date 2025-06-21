using System;
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

            // GET: Bookings with advanced filtering
            public async Task<IActionResult> Index(string searchTerm, int? eventTypeId,
                DateTime? startDate, DateTime? endDate, bool? venueAvailability)
            {
                var bookingsQuery = _context.Bookings
                    .Include(b => b.Event)
                        .ThenInclude(e => e.EventType)
                    .Include(b => b.Venue)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    bookingsQuery = bookingsQuery.Where(b =>
                        b.BookingId.ToString().Contains(searchTerm) ||
                        b.Event.EventName.Contains(searchTerm) ||
                        b.Venue.VenueName.Contains(searchTerm));
                }

                if (eventTypeId.HasValue)
                {
                    bookingsQuery = bookingsQuery.Where(b => b.Event.EventTypeId == eventTypeId.Value);
                }

                if (startDate.HasValue)
                {
                    bookingsQuery = bookingsQuery.Where(b => b.BookingDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    bookingsQuery = bookingsQuery.Where(b => b.BookingDate <= endDate.Value);
                }

                if (venueAvailability.HasValue)
                {
                    bookingsQuery = bookingsQuery.Where(b => b.Venue.Availability == venueAvailability.Value);
                }

                ViewData["EventTypes"] = new SelectList(_context.EventTypes, "EventTypeId", "EventTypeName", eventTypeId);
                ViewData["SearchTerm"] = searchTerm;
                ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
                ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
                ViewData["VenueAvailability"] = venueAvailability;

                return View(await bookingsQuery.ToListAsync());
            }


            // GET: Bookings/Details/5
            public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
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

                booking.Status = "Active";

                _context.Add(booking);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Booking created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,EventId,VenueId,BookingDate")] Booking booking)
        {
            if (id != booking.BookingId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
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
                    TempData["Message"] = "Booking updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
                        return NotFound();
                    else
                        throw;
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
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Booking deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Booking not found.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}
