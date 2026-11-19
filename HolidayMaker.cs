using System.Text;

class HolidayMaker(Database database)
{
    private readonly Database _db = database;

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
    // Customer functions

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

    // ---------------------------------------------------------------------------------------
    // Booking functions

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
        DateOnly checkinDate,
        DateOnly checkoutDate,
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
            ("@checkin_date", checkinDate),
            ("@checkout_date", checkoutDate)
        ];

        if (hasPool)
            sql.Append(Queries.FindAvailableRoomsHasPool);

        if (hasRestaurant)
            sql.Append(Queries.FindAvailableRoomsHasRestaurant);

        sql.Append(Queries.FindAvailableRoomsWhere);

        if (distanceToBeach > 0)
        {
            parameters.Add(("@distance_to_beach", distanceToBeach));
            sql.Append(Queries.FindAvailableRoomsDistanceToBeach);
        }

        if (distanceToCity > 0)
        {
            parameters.Add(("@distance_to_city", distanceToCity));
            sql.Append(Queries.FindAvailableRoomsDistanceToCity);
        }

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
                        reader.IsDBNull(reader.GetOrdinal("distance_to_beach")) ? 0 : reader.GetUInt32("distance_to_beach"),
                        reader.IsDBNull(reader.GetOrdinal("distance_to_city")) ? 0 : reader.GetUInt32("distance_to_city"),
                        reader.IsDBNull(reader.GetOrdinal("rating")) ? 0m : reader.GetDecimal("rating")),

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

    public bool InsertBooking(Booking booking, Customer customer, Room room)
    {
        using var transaction = _db.Connection.BeginTransaction();
        bool retval = false;

        try
        {
            (string, object)[] parameters = [("@room_id", room.Id)];
            using (var reader1 = _db.ExecuteReader(Queries.LockRoomForUpdate, parameters, transaction))
            {
                if (!reader1.Read())
                    return false;
            }

            parameters =
            [
                ("@room_id", room.Id),
                ("@checkin_date", booking.CheckinDate),
                ("@checkout_date", booking.CheckoutDate)
            ];
            using (var reader2 = _db.ExecuteReader(Queries.CheckRoomAvailability, parameters, transaction))
            {
                if (reader2.Read())
                    return false;
            }

            parameters =
            [
                ("@customer_number", customer.CustomerNumber),
                ("@room_id", room.Id),
                ("@checkin_date", booking.CheckinDate),
                ("@checkout_date", booking.CheckoutDate),
                ("@total_price", booking.TotalPrice),
                ("@cancelled", booking.Cancelled),
                ("@number_of_occupants", booking.Occupants)
            ];
            retval = _db.ExecuteNonQuery(Queries.InsertBooking, parameters, transaction) == 1;
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        return retval;
    }

    public List<(Booking, Room, Hotel)> GetBookings(ulong customerNumber)
    {
        List<(Booking, Room, Hotel)> bookings = [];

        using var reader = _db.ExecuteReader(
            Queries.SelectCustomerBookings,
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
                        reader.IsDBNull(reader.GetOrdinal("distance_to_beach")) ? 0 : reader.GetUInt32("distance_to_beach"),
                        reader.IsDBNull(reader.GetOrdinal("distance_to_city")) ? 0 : reader.GetUInt32("distance_to_city"),
                        reader.IsDBNull(reader.GetOrdinal("rating")) ? 0m : reader.GetDecimal("rating"))
                ));
        }

        return bookings;
    }

    public (Booking, Room, Hotel)? GetBooking(ulong bookingNumber)
    {
        using var reader = _db.ExecuteReader(
            Queries.SelectBooking, [("@booking_number", bookingNumber)]);

        if (reader.Read())
        {
            (Booking, Room, Hotel) booking = new(

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
                    reader.IsDBNull(reader.GetOrdinal("distance_to_beach")) ? 0 : reader.GetUInt32("distance_to_beach"),
                    reader.IsDBNull(reader.GetOrdinal("distance_to_city")) ? 0 : reader.GetUInt32("distance_to_city"),
                    reader.IsDBNull(reader.GetOrdinal("rating")) ? 0m : reader.GetDecimal("rating"))
            );
            return booking;
        }
        return null;
    }

    public bool UpdateBookingOccupants(ulong bookingNumber, byte occupants)
    {
        (string, object)[] parameters =
        [
            ("@booking_number", bookingNumber),
            ("@number_of_occupants", occupants)
        ];

        return _db.ExecuteNonQuery(Queries.UpdateBookingOccupants, parameters) == 1;
    }

    public bool CancelBooking(ulong bookingNumber)
    {
        return _db.ExecuteNonQuery(
            Queries.CancelBooking, [("@booking_number", bookingNumber)]) == 1;
    }

    public bool IsEmailUnique(string email)
    {
        using var reader = _db.ExecuteReader(Queries.SelectDuplicateEmail, [("@email", email)]);
        return !reader.Read();
    }

    public bool IsPhoneUnique(string phone)
    {
        using var reader = _db.ExecuteReader(Queries.SelectDuplicatePhone, [("@phone", phone)]);
        return !reader.Read();
    }
}
