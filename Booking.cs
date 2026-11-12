enum BookingStatus
{
    Available,
    Booked,
    Cancelled
}

class Booking
{
    public ulong BookingNumber { get; }
    public ulong CustomerNumber { get; }
    public ulong RoomId { get; }
    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }
    public float TotalPrice { get; }
    public BookingStatus Status { get; }
    public byte Occupants { get; }
}