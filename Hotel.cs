class Hotel
{
    public ulong Id { get; }
    public string Name { get; }
    public string Location { get; }
    public uint DistanceToBeach { get; }
    public uint DistanceToCity { get; }
    public decimal Rating { get; }

    public Hotel(ulong id, string name, string location, uint distanceToBeach, uint distanceToCity, decimal rating)
    {
        Id = id;
        Name = name;
        Location = location;
        DistanceToBeach = distanceToBeach;
        DistanceToCity = distanceToCity;
        Rating = rating;
    }

    public override string ToString()
    {
        return $"Name: {Name}, Location: {Location}, DistanceToBeach: {DistanceToBeach}m, DistanceToCity: {DistanceToCity}m, Rating: {Rating}";
    }
}