USE holiday_maker;

# Insert customer
SET @name = 'Tomas Törnkvist';
SET @address = 'Min gata 1A';
SET @email = 'tomas.tornkvist@student.nbi-handelsakademin.se';
SET @phone = '0701234567';
INSERT INTO customers (name, address, email, phone)
VALUES (@name, @address, @email, @phone);

# Select customer
SET @customer_number = 2;
SELECT * FROM customers
WHERE customer_number = @customer_number;

# Find customer
SET @name = 'Maria';
SELECT * FROM customers
WHERE name LIKE CONCAT('%', @name, '%');

# Check for duplicate email and phone
SET @email = 'maria.lindqvist@example.com'
SELECT 1 FROM customers WHERE email = @email;
SET @phone = '073-234 56 78'
SELECT 1 FROM customers WHERE phone = @phone;

# Find locations
SELECT DISTINCT location FROM hotels;

# Find available rooms
SET @location = 'Malmö';
SET @checkin_date = '2026-12-01';
SET @checkout_date = '2026-12-03';
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
    r.price_per_night,
    r.capacity,
    r.hotel_id,
    r.room_number
FROM rooms r
    JOIN hotels h ON h.id = r.hotel_id
--    JOIN hotels_x_amenities hxa1 ON h.id = hxa1.hotel_id
--    JOIN amenities a1 ON hxa1.amenities_id = a1.id
--        AND a1.name = 'Pool'
--    JOIN hotels_x_amenities hxa2 ON h.id = hxa2.hotel_id
--    JOIN amenities a2 ON hxa2.amenities_id = a2.id
--        AND a2.name = 'Restaurant'
WHERE h.location = @location
    AND r.capacity >= @capacity
    AND NOT EXISTS
    (
        SELECT *
        FROM bookings b
        WHERE b.room_id = r.id
            AND cancelled = FALSE
            AND b.checkin_date < @checkout_date
            AND b.checkout_date > @checkin_date
    )
--    AND h.distance_to_beach <= @distance_to_beach
--    AND h.distance_to_city <= @distance_to_city
-- ORDER BY h.location, h.name, r.room_number;
ORDER BY r.price_per_night;
-- ORDER BY h.rating DESC;

# Check room availability
SET @room_id = 2;
SELECT id FROM rooms WHERE id = @room_id FOR UPDATE;
SET @room_id = 2;
SET @checkin_date = '2026-12-01';
SET @checkout_date = '2026-12-03';
SELECT 1 FROM bookings
WHERE room_id = @room_id
    AND cancelled = FALSE
    AND b.checkin_date < @checkout_date
    AND b.checkout_date > @checkin_date
FOR UPDATE;

# Select booking
SET @booking_number = 1;
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
WHERE b.booking_number = @booking_number
    AND cancelled = FALSE;

# Select customer bookings
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
WHERE b.customer_number = @customer_number
    AND cancelled = FALSE
ORDER BY b.checkin_date;

# Insert booking
SET @customer_number = 1;
SET @room_id = 1;
SET @checkin_date = '2026-12-05';
SET @checkout_date = '2026-12-07';
SET @total_price = 1095;
SET @cancelled = FALSE;
SET @number_of_occupants = 1;
INSERT INTO bookings (
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
    @number_of_occupants);

# Update booking occupants
SET @booking_number = 1;
SET @number_of_occupants = 1;
UPDATE bookings b JOIN rooms r ON b.room_id = r.id
    SET number_of_occupants = @number_of_occupants
WHERE booking_number = @booking_number
    AND @number_of_occupants BETWEEN 1 AND r.capacity;

# Cancel booking
SET @booking_number = 1;
UPDATE bookings SET cancelled = TRUE
WHERE booking_number = @booking_number;
