class Room
{
    public ulong Id { get; }
    public decimal PricePerNight { get; }
    public byte Capacity { get; }
    public ulong HotelId { get; }
    public uint RoomNumber { get; }

    public Room(ulong id, decimal pricePerNight, byte capacity, ulong hotelId, uint roomNumber)
    {
        Id = id;
        PricePerNight = pricePerNight;
        Capacity = capacity;
        HotelId = hotelId;
        RoomNumber = roomNumber;
    }

    public override string ToString()
    {
        return $"Room number: {RoomNumber}, Capacity: {Capacity}, Price per night: {PricePerNight}";
    }
}
