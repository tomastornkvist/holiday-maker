using System.Text;

class HolidayMaker(Database db)
{
    private readonly Database _db = db;

    // ---------------------------------------------------------------------------------------
    // Customer functions

    public List<Customer> GetCustomers()
    {
        using var reader = _db.ExecuteReader(Queries.SelectCustomers, []);

        List<Customer> customers = [];

        while (reader.Read())
        {
            customers.Add(
                new(reader.GetUInt64("customer_number"),
                    reader.GetString("name"),
                    reader.GetString("address"),
                    reader.GetString("email"),
                    reader.GetString("phone")));
        }

        return customers;
    }

    public Customer? GetCustomer(ulong customerNumber)
    {
        using var reader = _db.ExecuteReader(Queries.SelectCustomer,
            [("@customer_number", customerNumber)]);

        Customer? customer = null;

        if (reader.Read())
        {
            customer = new(
                reader.GetUInt64("customer_number"),
                reader.GetString("name"),
                reader.GetString("address"),
                reader.GetString("email"),
                reader.GetString("phone"));
        }

        return customer;
    }

    public List<Customer> FindCustomers(string name)
    {
        List<Customer> customers = [];

        using var reader = _db.ExecuteReader(Queries.FindCustomer, [("@name", name)]);

        while (reader.Read())
        {
            customers.Add(
                new(reader.GetUInt64("customer_number"),
                    reader.GetString("name"),
                    reader.GetString("address"),
                    reader.GetString("email"),
                    reader.GetString("phone")));
        }
        return customers;
    }

    public bool InsertCustomer(string name, string address, string email, string phone)
    {
        (string, object)[] parameters =
        [
            ("@name", name),
            ("@address", address),
            ("@email", email),
            ("@phone", phone)
        ];

        return _db.ExecuteNonQuery(Queries.InsertCustomer, parameters) == 1;
    }

    public bool GetLastInsertedId(out ulong id)
    {
        id = 0;

        using var reader = _db.ExecuteReader("SELECT LAST_INSERT_ID()", []);

        if (!reader.Read())
            return false;

        id = reader.GetUInt64(0);
        return true;
    }

    // ---------------------------------------------------------------------------------------
    // Booking functions

    private void DeleteBooking()
    {

    }

    public string[] GetLocations()
    {
        using var reader = _db.ExecuteReader(Queries.FindLocations, []);
        List<string> locations = [];

        while (reader.Read())
            locations.Add(reader.GetString(0));

        return locations.ToArray();
    }

    public List<(Hotel hotel, Room room)> FindAvailableRooms(
        string location,
        byte occupants,
        DateOnly check_in_date,
        DateOnly check_out_date,
        uint distanceToBeach,
        uint distanceToCity,
        bool hasPool,
        bool hasRestaurant,
        Queries.OrderByFindAvailableRooms orderBy)
    {
        List<(Hotel hotel, Room room)> rooms = [];
        StringBuilder sql = new(Queries.FindAvailableRoomsSelectFrom);

        List<(string, object)> parameters =
        [
            ("@location", location),
            ("@capacity", occupants),
            ("@check_in_date", check_in_date),
            ("@check_out_date", check_out_date)
        ];

        if (distanceToBeach > 0)
        {
            parameters.Add(("@distance_to_beach", distanceToBeach));
            sql.Append(Queries.FindAvailableRoomsAndDistanceToBeach);
        }

        if (distanceToCity > 0)
        {
            parameters.Add(("@distance_to_city", distanceToCity));
            sql.Append(Queries.FindAvailableRoomsAndDistanceToCity);
        }

        if (hasPool)
            sql.Append(Queries.FindAvailableRoomsHasPool);

        if (hasRestaurant)
            sql.Append(Queries.FindAvailableRoomsHasRestaurant);

        sql.Append(Queries.FindAvailableRoomsWhere);

        switch (orderBy)
        {
            case Queries.OrderByFindAvailableRooms.PricePerNight:
                sql.Append(Queries.FindAvailableRoomsOrderByPricePerNight);
                break;

            case Queries.OrderByFindAvailableRooms.Rating:
                sql.Append(Queries.FindAvailableRoomsOrderByRating);
                break;

            default:
                sql.Append(Queries.FindAvailableRoomsOrderByStandard);
                break;
        }

        using var reader = _db.ExecuteReader(sql.ToString(), parameters.ToArray());

        while (reader.Read())
        {
            rooms.Add(
                new(
                    // Hotel
                    new(reader.GetUInt64("hotel_id"),
                        reader.GetString("name"),
                        reader.GetString("location"),
                        reader.GetUInt32("distance_to_beach"),
                        reader.GetUInt32("distance_to_city"),
                        reader.GetDecimal("rating")),
                    // Room
                    new(reader.GetUInt64("room_id"),
                        reader.GetDecimal("price_per_night"),
                        reader.GetByte("capacity"),
                        reader.GetUInt64("hotel_id"),
                        reader.GetUInt32("room_number"))
                ));
        }
        return rooms;
    }

    public bool InsertBooking(Booking booking, Customer customer, Hotel hotel, Room room)
    {
        (string, object)[] parameters =
        [
            ("@customer_number", customer.CustomerNumber),
            ("@room_id", room.Id),
            ("@checkin_date", booking.CheckinDate),
            ("@checkout_date", booking.CheckoutDate),
            ("@total_price", booking.TotalPrice),
            ("@cancelled", booking.Cancelled),
            ("@number_of_occupants", booking.Occupants)
        ];

        return _db.ExecuteNonQuery(Queries.InsertBooking, parameters) == 1;
    }

    public List<(Booking, Room, Hotel)> GetBookings(ulong customerNumber)
    {
        List<(Booking, Room, Hotel)> bookings = [];

        using var reader = _db.ExecuteReader(
            Queries.SelectBookings,
            [("@customer_number", customerNumber)]);

        while (reader.Read())
        {
            bookings.Add
            (
                new(
                    // Booking
                    new(reader.GetUInt64("booking_number"),
                        reader.GetUInt64("customer_number"),
                        reader.GetUInt64("room_id"),
                        reader.GetDateOnly("checkin_date"),
                        reader.GetDateOnly("checkout_date"),
                        reader.GetDecimal("total_price"),
                        reader.GetBoolean("cancelled"),
                        reader.GetByte("number_of_occupants")),

                    // Room
                    new(reader.GetUInt64("room_id"),
                        reader.GetDecimal("price_per_night"),
                        reader.GetByte("capacity"),
                        reader.GetUInt64("hotel_id"),
                        reader.GetUInt32("room_number")),

                    // Hotel
                    new(reader.GetUInt64("hotel_id"),
                        reader.GetString("name"),
                        reader.GetString("location"),
                        reader.GetUInt32("distance_to_beach"),
                        reader.GetUInt32("distance_to_city"),
                        reader.GetDecimal("rating"))
                ));
        }

        return bookings;
    }
}
