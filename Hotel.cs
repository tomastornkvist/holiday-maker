class Hotel
{
    ulong Id { get; }
    string Name { get; }
    string Location { get; }
    uint DistanceToBeach { get; }
    uint DistanceToCity { get; }
    float Rating { get; }

    public Hotel(ulong id, string name, string location, uint distanceToBeach, uint distanceToCity, float rating)
    {
        Id = id;
        Name = name;
        Location = location;
        DistanceToBeach = distanceToBeach;
        DistanceToCity = distanceToCity;
        Rating = rating;
    }
}