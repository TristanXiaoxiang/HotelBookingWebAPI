using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPIPro.Data;
using WebAPIPro.Helper;
using WebAPIPro.Models;

namespace WebAPIPro.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HotelController : ControllerBase
    {
        private readonly HotelBookingContext _context;
        public HotelController(HotelBookingContext context)
        {
            _context = context;
        }
    
        [HttpGet]
        public async Task<ActionResult> GetAllRooms()
        {
            var result = await _context.HotelRoom.ToListAsync();
            return Ok(result);
        }

        [HttpGet("{numberOFNumbers}/{startDateTime}/{endDateTime}")]
        public async Task<ActionResult> SearchAvailableRoom(int numberOFNumbers, DateTime startDateTime, DateTime endDateTime)
        {
            if (startDateTime >= endDateTime)
            {
                return BadRequest($"startDateTime : {startDateTime} is bigger or equal to endDateTime :{endDateTime}");
            }

            var allRooms  = await _context.HotelRoom.Include(b5 => b5.Bookings).ToListAsync();

            var bigEnoughRooms = allRooms.Where(s => s.NumberofBeds >= numberOFNumbers);

            var availableRooms = bigEnoughRooms.Where(s => s.Bookings.Count == 0 || s.Bookings.All(b => endDateTime <= b.BookedFromDate || startDateTime >= b.BookedEndDate)).ToList();

            return Ok(availableRooms);
        }

        [HttpPost("{name}/{startDateTime}/{endDateTime}")]
        public async Task<ActionResult> BookRoom(string roomName, DateTime startDateTime, DateTime endDateTime)
        {
            if (startDateTime >= endDateTime)
            {
                return BadRequest($"startDateTime : {startDateTime} is bigger than or equal endDateTime :{endDateTime}");
            }
            var targetRoom = await _context.HotelRoom.Include(b5 => b5.Bookings)
                .FirstOrDefaultAsync(m => m.RoomName == roomName);
            if (targetRoom == null)
            {
                return NotFound();
            }

            var isAvailable = targetRoom.Bookings.Count == 0 || targetRoom.Bookings.All(b => endDateTime <= b.BookedFromDate || startDateTime >= b.BookedEndDate);
            if (isAvailable)
            {
                var booking = new Booking()
                {
                    BookedFromDate = startDateTime,
                    BookedEndDate = endDateTime,
                    BookingReference = GenerateRandomStringHelper.GenerateRandomString(10),
                };

                targetRoom.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                return Ok($"{booking.BookingReference}");
            }
            else
            {
                return BadRequest($"Room {roomName} had been booked");
            }
        }

        [HttpDelete("{bookingReference}")]
        public async Task<ActionResult> CancellBooking(string bookingReference)
        {
            var booking = await _context.Booking.Include(b5 => b5.HotelRoom)
                .FirstOrDefaultAsync(m => m.BookingReference == bookingReference);
            if (booking == null)
            {
                return NotFound();
            }
            else
            {
                _context.Booking.Remove(booking);
                await _context.SaveChangesAsync();
                return Ok($"Cancel successfully for {bookingReference}");
            }
        }

        [HttpGet("{startDateTime}/{endDateTime}")]
        public async Task<ActionResult> SearchCurrentBookings(DateTime startDateTime, DateTime endDateTime)
        {
            if (startDateTime >= endDateTime)
            {
                return BadRequest($"startDateTime : {startDateTime} is bigger than or equal endDateTime :{endDateTime}");
            }

            var allBookings = await _context.Booking.ToListAsync();
            var bookingsDuringTime = allBookings.Where(s => !(endDateTime <= s.BookedFromDate || startDateTime >= s.BookedEndDate)).ToList();
            if (bookingsDuringTime.Count() == 0)
            {
                return Ok($"No room is booked during the time {startDateTime} to {endDateTime}");
            }
            else
            {
                return Ok(bookingsDuringTime);
            }
        }
    }
}