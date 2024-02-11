using Microsoft.EntityFrameworkCore;
using WebAPIPro.Models;

namespace WebAPIPro.Data
{
    public class HotelBookingContext : DbContext
    {
        public DbSet<HotelRoom> HotelRoom { get; set; }

        public DbSet<Booking> Booking { get; set; }

        public HotelBookingContext(DbContextOptions<HotelBookingContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableDetailedErrors();
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HotelRoom>()
                .HasKey(e => e.RoomId);

            modelBuilder.Entity<Booking>()
                .HasKey(e => e.BookingID);


            modelBuilder.Entity<Booking>()
                .HasOne(sa => sa.HotelRoom)
                .WithMany(e => e.Bookings)
                .HasForeignKey(sa => sa.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
