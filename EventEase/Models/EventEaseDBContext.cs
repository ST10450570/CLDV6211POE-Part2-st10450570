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

        public DbSet<EventEase.Models.Venue> Venues { get; set; }
        public DbSet<EventEase.Models.Event> Events { get; set; }
        public DbSet<EventEase.Models.Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuring many-to-many relationship via Booking
            modelBuilder.Entity<Booking>()
                .HasKey(b => b.BookingId);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Event)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EventId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Venue)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VenueId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // For validation: add a unique constraint for Event + Venue + Date combination
            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.EventId, b.VenueId, b.BookingDate })
                .IsUnique();
        }
    }
}
