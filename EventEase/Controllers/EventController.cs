using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;
using EventEase.ViewModels;

namespace EventEase.Controllers
{
    public class EventController : Controller
    {
        private readonly EventEaseDBContext _context;
        private readonly IImageService _imageService;

        public EventController(EventEaseDBContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events.Include(e => e.Venue).ToListAsync();
            return View(events);
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events.Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (@event == null) return NotFound();

            return View(@event);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            var eventViewModel = new EventViewModel
            {
                VenueList = _context.Venues.Select(v => new SelectListItem
                {
                    Value = v.VenueId.ToString(),
                    Text = v.VenueName
                }).ToList(),
                EventDate = DateTime.Today
            };

            return View(eventViewModel);
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel eventViewModel)
        {
            if (ModelState.IsValid)
            {
                var @event = new Event
                {
                    EventName = eventViewModel.EventName,
                    EventDate = eventViewModel.EventDate,
                    Description = eventViewModel.Description,
                    VenueId = eventViewModel.VenueId
                };

                if (eventViewModel.ImageFile != null && eventViewModel.ImageFile.Length > 0)
                {
                    @event.ImageUrl = await _imageService.UploadImageAsync(eventViewModel.ImageFile, "events");
                }

                _context.Events.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            eventViewModel.VenueList = _context.Venues.Select(v => new SelectListItem
            {
                Value = v.VenueId.ToString(),
                Text = v.VenueName
            }).ToList();

            return View(eventViewModel);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events.FindAsync(id);
            if (@event == null) return NotFound();

            var eventViewModel = new EventViewModel
            {
                EventId = @event.EventId,
                EventName = @event.EventName,
                EventDate = @event.EventDate,
                Description = @event.Description,
                VenueId = @event.VenueId,
                ExistingImageUrl = @event.ImageUrl,
                VenueList = _context.Venues.Select(v => new SelectListItem
                {
                    Value = v.VenueId.ToString(),
                    Text = v.VenueName,
                    Selected = v.VenueId == @event.VenueId
                }).ToList()
            };

            return View(eventViewModel);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventViewModel eventViewModel)
        {
            if (id != eventViewModel.EventId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var @event = await _context.Events.FindAsync(id);
                    @event.EventName = eventViewModel.EventName;
                    @event.EventDate = eventViewModel.EventDate;
                    @event.Description = eventViewModel.Description;
                    @event.VenueId = eventViewModel.VenueId;

                    if (eventViewModel.ImageFile != null && eventViewModel.ImageFile.Length > 0)
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(@event.ImageUrl))
                        {
                            await _imageService.DeleteImageAsync(@event.ImageUrl);
                        }
                        @event.ImageUrl = await _imageService.UploadImageAsync(eventViewModel.ImageFile, "events");
                    }

                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(eventViewModel.EventId))
                        return NotFound();
                    else
                        throw;
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException?.Message.Contains("duplicate") == true ||
                        ex.InnerException?.Message.Contains("UNIQUE") == true)
                    {
                        ModelState.AddModelError(string.Empty, "This venue is already booked for the specified date.");
                        eventViewModel.VenueList = _context.Venues.Select(v => new SelectListItem
                        {
                            Value = v.VenueId.ToString(),
                            Text = v.VenueName
                        }).ToList();
                        return View(eventViewModel);
                    }
                    throw;
                }
            }

            eventViewModel.VenueList = _context.Venues.Select(v => new SelectListItem
            {
                Value = v.VenueId.ToString(),
                Text = v.VenueName
            }).ToList();

            return View(eventViewModel);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events.Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (@event == null) return NotFound();

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventToDelete = await _context.Events
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventToDelete == null)
            {
                return NotFound();
            }

            if (eventToDelete.Bookings.Any())
            {
                TempData["ErrorMessage"] = $"Event \"{eventToDelete.EventName}\" cannot be deleted because it has existing bookings.";
                return RedirectToAction(nameof(Index));
            }

            // Delete associated image if exists
            if (!string.IsNullOrEmpty(eventToDelete.ImageUrl))
            {
                await _imageService.DeleteImageAsync(eventToDelete.ImageUrl);
            }

            _context.Events.Remove(eventToDelete);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Event \"{eventToDelete.EventName}\" was successfully deleted.";
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }
    }
}