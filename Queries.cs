static class Queries
{
    public static string SelectCustomers { get; } = "SELECT * FROM customers";
    public static string SelectCustomer { get; } = "SELECT * FROM customers WHERE customer_number = @customer_number";
    public static string InsertCustomer { get; } = "INSERT INTO customers(name, address, email, phone) VALUES(@name, @address, @email, @phone)";
    public static string DeleteCustomer { get; } = "DELETE FROM customers WHERE customer_number = @customer_number";
    public static string FindLocations { get; } = "SELECT DISTINCT location FROM hotels";
    public static string FindAvailableRooms { get; } =
    @"SELECT h.*, r.*
    FROM rooms r
        JOIN hotels h ON h.id = r.hotel_id
        LEFT JOIN bookings b ON r.id = b.room_id
    WHERE (
        b.booking_number IS NULL
        OR (
            b.end_date <= @check_in_date
            OR b.start_date >= @check_out_date
        )
    )";
}
