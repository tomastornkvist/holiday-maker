enum BookingStatus
{
    Available,
    Booked,
    Cancelled
}

class Booking
{
    public ulong BookingNumber { get; }
    public ulong CustomerNumber { get; set; }
    public ulong RoomId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public float TotalPrice { get; set; }
    public BookingStatus Status { get; set; }
    public byte Occupants { get; set; }
}