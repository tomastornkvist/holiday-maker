USE holiday_maker;

# Register and select customer
INSERT INTO customers (name, address, email, phone) VALUES (@name, @address, @email, @phone);
SELECT * FROM customers;
SET @customer_number = 2;
SELECT * FROM customers WHERE customer_number = @customer_number;

# Find available rooms
SET @location = 'Malmö';
SET @check_in_date = '2026-11-13';
SET @check_out_date = '2026-11-15';
SET @capacity = 2;
SET @distance_to_beach = 1000;
SET @distance_to_city = 5000;

SELECT h.*, r.* FROM rooms r
    JOIN hotels h ON h.id = r.hotel_id
    LEFT JOIN bookings b ON r.id = b.room_id
WHERE (b.booking_number IS NULL
             OR (
                 b.end_date <= @check_in_date
                     OR b.start_date >= @check_out_date
                )
        )
