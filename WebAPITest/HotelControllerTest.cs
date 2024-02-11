using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WebAPIPro.Controllers;
using WebAPIPro.Data;
using WebAPIPro.Models;
using Xunit;

namespace WebAPITest
{
    public class HotelControllerTest
    {
        private readonly DbContextOptions<HotelBookingContext> _contextOptions;

        #region Constructor
        public HotelControllerTest()
        {
            _contextOptions = new DbContextOptionsBuilder<HotelBookingContext>()
                .UseInMemoryDatabase("HotelControllerTest")
                .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
        }
        #endregion

        #region  GetAvailableRooms

        [Fact]
        public void GetAllRooms()
        {
            var controller = new HotelController(CreateContext());
            var actionResult = controller.GetAllRooms().Result as OkObjectResult;
            Assert.Equal((actionResult.Value as List<HotelRoom>).Count, 4);
        }

        [Fact]
        public void GetAvailableRooms()
        {
            var controller = new HotelController(CreateContext());
            var actionResult = controller.SearchAvailableRoom(3, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2)).Result as OkObjectResult;
            Assert.Equal((actionResult.Value as List<HotelRoom>).Count, 2);
        }

        [Fact]
        public void GetAvailableRoomsWithBooked()
        {
            var controller = new HotelController(CreateContext());
            controller.BookRoom("ThisRoom3", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2)).Wait();
            var actionResult = controller.SearchAvailableRoom(3, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2)).Result as OkObjectResult;
            Assert.Equal((actionResult.Value as List<HotelRoom>).Count, 1);
        }

        [Fact]
        public void GetAvailableRoomsBookTimeInvalid()
        {
            var controller = new HotelController(CreateContext());
            var startDateTime = DateTime.UtcNow.AddDays(2);
            var endDateTime = DateTime.UtcNow.AddDays(1);
            var actionResult = controller.BookRoom("ThisRoom3", DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(1)).Result as BadRequestObjectResult;
            Assert.Equal($"startDateTime : {startDateTime} is bigger than or equal endDateTime :{endDateTime}", actionResult.Value as string);
        }

        #endregion

        #region Booking
        [Fact]
        public void BookingHotelsReturnReference()
        {
            var controller = new HotelController(CreateContext());
            var bookingResult = controller.BookRoom("ThisRoom3", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2)).Result as OkObjectResult;
            Assert.NotNull(bookingResult.Value as string);
        }
        [Fact]
        public void BookingHotelsOverlap()
        {
            var controller = new HotelController(CreateContext());
            controller.BookRoom("ThisRoom3", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(10)).Wait();
            var secondBooking = controller.BookRoom("ThisRoom3", DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(12)).Result as BadRequestObjectResult;
            Assert.Equal("Room ThisRoom3 had been booked", secondBooking.Value as string);
        }
        [Fact]
        public void BookingHotelsDuplicate()
        {
            var controller = new HotelController(CreateContext());
            controller.BookRoom("ThisRoom3", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2)).Wait();
            var secondBooking = controller.BookRoom("ThisRoom3", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2)).Result as BadRequestObjectResult;
            Assert.Equal("Room ThisRoom3 had been booked", secondBooking.Value as string);
        }

        #endregion

        #region  SearchCurrentBookings

        [Fact]
        public void SearchCurrentBookings()
        {
            var controller = new HotelController(CreateContext());

            controller.BookRoom("ThisRoom1", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2)).Wait();
            controller.BookRoom("ThisRoom2", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2)).Wait();
            controller.BookRoom("ThisRoom3", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2)).Wait();

            var searchResult = controller.SearchCurrentBookings(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2)).Result as OkObjectResult;
            Assert.Equal((searchResult.Value as List<Booking>).Count, 3);
        }

        #endregion

        #region Cancell Booking
       
        [Fact]
        public void CancellBooking()
        {
            var controller = new HotelController(CreateContext());

            var firstBookingResult = controller.BookRoom("ThisRoom3", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2)).Result as OkObjectResult;
            var firstBookingReference = (firstBookingResult.Value as string);
            Assert.NotNull(firstBookingReference);

            var firstSearchResult = controller.SearchCurrentBookings(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2)).Result as OkObjectResult;
            Assert.Equal((firstSearchResult.Value as List<Booking>).Count, 1);

            var cancelResult = controller.CancellBooking(firstBookingReference).Result as OkObjectResult; ;
            Assert.Equal($"Cancel successfully for {firstBookingReference}", cancelResult.Value as string);

            var secondSerachResult = controller.SearchCurrentBookings(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2)).Result as OkObjectResult;
            Assert.StartsWith("No room is booked during the time", secondSerachResult.Value as string);
        }

        #endregion

        private HotelBookingContext CreateContext()
        {
            var context = new HotelBookingContext(_contextOptions);

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            context.HotelRoom.AddRange(
            new HotelRoom { RoomId = 1, RoomName = "ThisRoom1", Description = "ThisRoom1", Price = 230, NumberofBeds = 1 },
            new HotelRoom { RoomId = 2, RoomName = "ThisRoom2", Description = "ThisRoom2", Price = 230, NumberofBeds = 2 },
            new HotelRoom { RoomId = 3, RoomName = "ThisRoom3", Description = "ThisRoom3", Price = 230, NumberofBeds = 3 },
            new HotelRoom { RoomId = 4, RoomName = "ThisRoom4", Description = "ThisRoom4", Price = 230, NumberofBeds = 4 }
            );
            context.SaveChanges();
            return context;
        }
    }

}
