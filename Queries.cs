static class Queries
{
    public enum OrderByFindAvailableRooms
    {
        Standard,
        PricePerNight,
        Rating
    }

    // ----------------------------------------------------------------------------------
    // SELECT queries
    public const string SelectCustomers = "SELECT * FROM customers";

    public const string SelectCustomer =
    "SELECT * FROM customers WHERE customer_number = @customer_number";

    public const string FindCustomer =
    "SELECT * FROM customers WHERE name LIKE CONCAT('%', @name, '%')";

    public const string FindLocations = "SELECT DISTINCT location FROM hotels";

    public const string FindAvailableRoomsSelectFrom =
    @"SELECT
        h.id AS hotel_id,
        h.name,
        h.location,
        h.distance_to_beach,
        h.distance_to_city,
        h.rating,
        r.id AS room_id,
        r.price_per_night,
        r.capacity,
        r.hotel_id,
        r.room_number
    FROM rooms r
        JOIN hotels h ON h.id = r.hotel_id";
    public const string FindAvailableRoomsHasPool =
    @" JOIN hotels_x_amenities hxa1 ON h.id = hxa1.hotel_id
        JOIN amenities a1 ON hxa1.amenities_id = a1.id AND a1.name = 'Pool'";
    public const string FindAvailableRoomsHasRestaurant =
    @" JOIN hotels_x_amenities hxa2 ON h.id = hxa2.hotel_id
        JOIN amenities a2 ON hxa2.amenities_id = a2.id AND a2.name = 'Restaurant'";
    public const string FindAvailableRoomsWhere =
    @" WHERE h.location = @location
        AND r.capacity >= @capacity
        AND NOT EXISTS (
            SELECT 1
            FROM bookings b
            WHERE b.room_id = r.id
              AND b.cancelled = FALSE
              AND b.checkin_date < @check_out_date
              AND b.checkout_date > @check_in_date
            )";
    public const string FindAvailableRoomsDistanceToBeach = " AND h.distance_to_beach <= @distance_to_beach";
    public const string FindAvailableRoomsDistanceToCity = " AND h.distance_to_city <= @distance_to_city";
    public const string FindAvailableRoomsOrderByStandard = " ORDER BY h.location, h.name, r.room_number";
    public const string FindAvailableRoomsOrderByPricePerNight = " ORDER BY r.price_per_night";
    public const string FindAvailableRoomsOrderByRating = " ORDER BY h.rating";

    public const string SelectBooking =
    @"SELECT
        b.*,
        r.id AS room_id,
        r.price_per_night,
        r.capacity,
        r.hotel_id,
        r.room_number,
        h.id AS hotel_id,
        h.name,
        h.location,
        h.distance_to_beach,
        h.distance_to_city,
        h.rating
    FROM bookings b
        JOIN rooms r ON b.room_id = r.id
        JOIN hotels h ON r.hotel_id = h.id
    WHERE b.booking_number = @booking_number AND cancelled = FALSE";

    public const string SelectCustomerBookings =
    @"SELECT
        b.*,
        r.id AS room_id,
        r.price_per_night,
        r.capacity,
        r.hotel_id,
        r.room_number,
        h.id AS hotel_id,
        h.name,
        h.location,
        h.distance_to_beach,
        h.distance_to_city,
        h.rating
    FROM bookings b
        JOIN rooms r ON b.room_id = r.id
        JOIN hotels h ON r.hotel_id = h.id
    WHERE b.customer_number = @customer_number AND cancelled = FALSE
    ORDER BY b.checkin_date";

    // ----------------------------------------------------------------------------------
    // INSERT queries
    public const string InsertCustomer =
    @"INSERT INTO customers(name, address, email, phone) VALUES(@name, @address, @email, @phone)";

    public const string InsertBooking =
    @"INSERT INTO bookings (
        customer_number,
        room_id,
        checkin_date,
        checkout_date,
        total_price,
        cancelled,
        number_of_occupants)
    VALUES (
        @customer_number,
        @room_id,
        @checkin_date,
        @checkout_date,
        @total_price,
        @cancelled,
        @number_of_occupants)";

    // ----------------------------------------------------------------------------------
    // UPDATE queries
    public const string UpdateBookingOccupants =
    @"UPDATE bookings b JOIN rooms r ON b.room_id = r.id SET number_of_occupants = @number_of_occupants
    WHERE booking_number = @booking_number AND @number_of_occupants BETWEEN 1 AND r.capacity";

    public const string CancelBooking =
    "UPDATE bookings SET cancelled = TRUE WHERE booking_number = @booking_number";
}
