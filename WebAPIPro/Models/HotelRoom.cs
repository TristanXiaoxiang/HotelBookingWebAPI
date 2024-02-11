namespace WebAPIPro.Models
{
    public class HotelRoom
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public string Description { get; set; }
        public int NumberofBeds { get; set; }
        public decimal Price { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; }
    }

    public class Booking
    {
        public int BookingID { get; set; }

        public int RoomId { get; set; }
        public virtual HotelRoom HotelRoom { get; set; }

        public DateTime BookedFromDate { get; set; }
        public DateTime BookedEndDate { get; set; }
        public string BookingReference { get; set; }
    } 
}
