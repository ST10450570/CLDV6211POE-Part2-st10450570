using EventEase.Models;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Data
{
    public class EventEaseDBContext : DbContext
    {
        public EventEaseDBContext(DbContextOptions<EventEaseDBContext> options)
            : base(options)
        {
        }

        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<EventType> EventTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuring many-to-many relationship via Booking
            modelBuilder.Entity<Booking>()
                .HasKey(b => b.BookingId);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Event)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Venue)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.EventType)
                .WithMany()
                .HasForeignKey(e => e.EventTypeId)
                .OnDelete(DeleteBehavior.SetNull);

            // For validation: add a unique constraint for Event + Venue + Date combination
            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.EventId, b.VenueId, b.BookingDate })
                .IsUnique();
        }
    }
}