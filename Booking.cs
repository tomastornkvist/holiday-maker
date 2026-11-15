class Booking
{
    public ulong BookingNumber { get; }
    public ulong CustomerNumber { get; }
    public ulong RoomId { get; }
    public DateOnly CheckinDate { get; }
    public DateOnly CheckoutDate { get; }
    public decimal TotalPrice { get; }
    public bool Cancelled { get; }
    public byte Occupants { get; }

    public Booking(
        ulong bookingNumber,
        ulong customerNumber,
        ulong roomId,
        DateOnly checkinDate,
        DateOnly checkoutDate,
        decimal totalPrice,
        bool cancelled,
        byte occupants)
    {
        BookingNumber = bookingNumber;
        CustomerNumber = customerNumber;
        RoomId = roomId;
        CheckinDate = checkinDate;
        CheckoutDate = checkoutDate;
        TotalPrice = totalPrice;
        Cancelled = cancelled;
        Occupants = occupants;
    }
}
