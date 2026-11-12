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

SELECT * FROM hotels h
    JOIN hotels_x_amenities hxa1 ON h.id = hxa1.hotel_id
    JOIN amenities a1 ON hxa1.amenities_id = a1.id
    JOIN hotels_x_amenities hxa2 ON h.id = hxa2.hotel_id
    JOIN amenities a2 ON hxa2.amenities_id = a2.id
    JOIN rooms r ON h.id = r.hotel_id
    LEFT JOIN bookings b ON r.id = b.room_id
WHERE a1.name = 'Pool'
    AND a2.name = 'Restaurant'
    AND h.distance_to_beach <= @distance_to_beach
    AND h.distance_to_city <= @distance_to_city
    AND h.location = @location
    AND r.capacity >= @capacity
    AND (
        b.booking_number IS NULL
            OR  (
                b.end_date <= @check_in_date
                    OR b.start_date >= @check_out_date
                )
        )
