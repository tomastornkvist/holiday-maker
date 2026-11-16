USE holiday_maker;

# Register and select customer
INSERT INTO customers (name, address, email, phone) VALUES (@name, @address, @email, @phone);
SELECT * FROM customers;
SET @customer_number = 2;
SELECT * FROM customers WHERE customer_number = @customer_number;
SET @name = 'Tomas';
SELECT * FROM customers WHERE name LIKE CONCAT('%', @name, '%');

# Find available rooms
SET @location = 'Malmö';
SET @check_in_date = '2026-12-01';
SET @check_out_date = '2026-12-03';
SET @capacity = 1;
SET @distance_to_beach = 1000;
SET @distance_to_city = 2000;
SELECT
    h.id AS hotel_id,
    h.name,
    h.location,
    h.distance_to_beach,
    h.distance_to_city,
    h.rating,
    r.id AS room_id,
    r.room_number,
    r.price_per_night,
    r.capacity
FROM rooms r
    JOIN hotels h ON h.id = r.hotel_id
#    JOIN hotels_x_amenities hxa1 ON h.id = hxa1.hotel_id
#    JOIN amenities a1 ON hxa1.amenities_id = a1.id AND a1.name = 'Pool'
#    JOIN hotels_x_amenities hxa2 ON h.id = hxa2.hotel_id
#    JOIN amenities a2 ON hxa2.amenities_id = a2.id AND a2.name = 'Restaurant'
WHERE h.location = @location
    AND r.capacity >= @capacity
    AND NOT EXISTS (
        SELECT 1
        FROM bookings b
        WHERE b.room_id = r.id
          AND b.cancelled = FALSE
          AND b.checkin_date < @check_out_date
          AND b.checkout_date > @check_in_date
        )
    AND h.distance_to_beach <= @distance_to_beach
#    AND h.distance_to_city <= @distance_to_city
ORDER BY h.location, h.name, r.room_number
#ORDER BY r.price_per_night
#ORDER BY h.rating

# Select bookings
SELECT
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
WHERE b.booking_number = @booking_number AND cancelled = FALSE;

SET @customer_number = 3;
SELECT
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
ORDER BY b.checkin_date;

# Update/remove bookings
SET @booking_number = 1;
SET @number_of_occupants = 1;
UPDATE bookings b JOIN rooms r ON b.room_id = r.id
SET number_of_occupants = @number_of_occupants
WHERE booking_number = @booking_number
    AND @number_of_occupants BETWEEN 1 AND r.capacity;

SET @booking_number = 1;
UPDATE bookings SET cancelled = TRUE WHERE booking_number = @booking_number;
