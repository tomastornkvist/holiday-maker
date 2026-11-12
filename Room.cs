class Room
{
    ulong Id { get; }
    float PricePerNight { get; }
    byte Capacity { get; }
    ulong HotelId { get; }
    uint RoomNumber { get; }

    public Room(ulong id, float pricePerNight, byte capacity, ulong hotelId, uint roomNumber)
    {
        Id = id;
        PricePerNight = pricePerNight;
        Capacity = capacity;
        HotelId = hotelId;
        RoomNumber = roomNumber;
    }
}