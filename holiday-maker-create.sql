DROP DATABASE IF EXISTS holiday_maker;

CREATE DATABASE holiday_maker;

USE holiday_maker;

CREATE TABLE `hotels`(
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `name` VARCHAR(100) NOT NULL,
    `location` VARCHAR(100) NOT NULL,
    `distance_to_beach` INT UNSIGNED NULL,
    `distance_to_city` INT UNSIGNED NULL,
    `rating` DECIMAL(8, 2) NULL
);
CREATE TABLE `customers`(
    `customer_number` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `name` VARCHAR(100) NOT NULL,
    `address` VARCHAR(255) NOT NULL,
    `email` VARCHAR(100) NULL,
    `phone` VARCHAR(20) NULL
);
ALTER TABLE
    `customers` ADD UNIQUE `customers_email_unique`(`email`);
ALTER TABLE
    `customers` ADD UNIQUE `customers_phone_unique`(`phone`);
CREATE TABLE `rooms`(
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `price_per_night` DECIMAL(8, 2) NOT NULL,
    `capacity` TINYINT UNSIGNED NOT NULL,
    `hotel_id` BIGINT UNSIGNED NOT NULL,
    `room_number` INT NOT NULL
);
CREATE TABLE `bookings`(
    `booking_number` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `customer_number` BIGINT UNSIGNED NOT NULL,
    `room_id` BIGINT UNSIGNED NOT NULL,
    `checkin_date` DATE NOT NULL,
    `checkout_date` DATE NOT NULL,
    `total_price` DECIMAL(8, 2) NOT NULL,
    `cancelled` BOOLEAN NOT NULL DEFAULT FALSE,
    `number_of_occupants` TINYINT NOT NULL
);
CREATE TABLE `amenities`(
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `name` VARCHAR(100) NOT NULL
);
CREATE TABLE `hotels_x_amenities`(
    `hotel_id` BIGINT UNSIGNED NOT NULL,
    `amenities_id` BIGINT UNSIGNED NOT NULL,
    PRIMARY KEY(`hotel_id`, `amenities_id`)
);
ALTER TABLE
    `bookings` ADD CONSTRAINT `bookings_room_id_foreign` FOREIGN KEY(`room_id`) REFERENCES `rooms`(`id`);
ALTER TABLE
    `rooms` ADD CONSTRAINT `rooms_hotel_id_foreign` FOREIGN KEY(`hotel_id`) REFERENCES `hotels`(`id`);
ALTER TABLE
    `bookings` ADD CONSTRAINT `bookings_customer_id_foreign` FOREIGN KEY(`customer_number`) REFERENCES `customers`(`customer_number`);
ALTER TABLE
    `hotels_x_amenities` ADD CONSTRAINT `hotel_x_amenities_amenities_id_foreign` FOREIGN KEY(`amenities_id`) REFERENCES `amenities`(`id`);
ALTER TABLE
    `hotels_x_amenities` ADD CONSTRAINT `hotel_x_amenities_hotel_id_foreign` FOREIGN KEY(`hotel_id`) REFERENCES `hotels`(`id`);


# ------------------------------------------------------------------------------------
# ADD TESTING DATA
#

-- Hotels, hostels and B&Bs (3 in Malmö, 2 in Lund)
-- distance_to_beach / distance_to_city in meters
INSERT INTO `hotels` (`id`, `name`, `location`, `distance_to_beach`, `distance_to_city`, `rating`) VALUES
    (1, 'Hotel Ribersborg',          'Malmö', 150,   2500, 4.40),
    (2, 'Gamla Staden Hostel',       'Malmö', 2800,  200,  3.70),
    (3, 'Västra Hamnen B&B',         'Malmö', 300,   1800, 4.60),
    (4, 'Grand Hotel Domkyrkan',     'Lund',  11000, 300,  4.50),
    (5, 'Lundagård Vandrarhem',      'Lund',  10500, 500,  3.50);

-- Rooms (22 rooms across the 5 places)
INSERT INTO `rooms` (`price_per_night`, `capacity`, `hotel_id`, `room_number`) VALUES
    -- Hotel Ribersborg
    (1095.00, 1, 1, 101),
    (1395.00, 2, 1, 102),
    (1395.00, 2, 1, 103),
    (1895.00, 4, 1, 201),
    (2295.00, 4, 1, 301),
    -- Gamla Staden Hostel (dorms + private rooms)
    (395.00,  4, 2, 1),
    (395.00,  4, 2, 2),
    (395.00,  4, 2, 3),
    (595.00,  2, 2, 4),
    (495.00,  1, 2, 5),
    -- Västra Hamnen B&B
    (895.00,  2, 3, 1),
    (895.00,  2, 3, 2),
    (1395.00, 4, 3, 3),
    -- Grand Hotel Domkyrkan
    (1295.00, 1, 4, 101),
    (1595.00, 2, 4, 102),
    (1595.00, 2, 4, 103),
    (2495.00, 4, 4, 201),
    (2995.00, 4, 4, 301),
    -- Lundagård Vandrarhem (dorms + private rooms)
    (350.00,  4, 5, 1),
    (350.00,  4, 5, 2),
    (550.00,  2, 5, 3),
    (450.00,  1, 5, 4);

-- Customers
INSERT INTO `customers` (`name`, `address`, `email`, `phone`) VALUES
    ('Anna Svensson',   'Storgatan 12, 111 22 Stockholm',      'anna.svensson@example.com',   '070-123 45 67'),
    ('Erik Johansson',  'Kungsgatan 5, 411 19 Göteborg',       'erik.johansson@example.com',  '073-234 56 78'),
    ('Maria Lindqvist', 'Södra Förstadsgatan 8, 211 43 Malmö', 'maria.lindqvist@example.com', '076-345 67 89'),
    ('Johan Berg',      'Drottninggatan 21, 753 10 Uppsala',   'johan.berg@example.com',      '072-456 78 90'),
    ('Sara Nilsson',    'Vasagatan 3, 722 15 Västerås',        'sara.nilsson@example.com',    '070-567 89 01');

-- Aminities
INSERT INTO amenities (name) VALUES
    ('Pool'),
    ('Restaurant'),
    ('Gym'),
    ('Spa'),
    ('Bar'),
    ('Playground');

-- Hotel amenities
INSERT INTO hotels_x_amenities (hotel_id, amenities_id) VALUES
    -- Hotel Ribersborg
    (1, 1),
    (1, 2),
    (1, 4),
    -- Gamla Staden Hostel (dorms + private rooms)
    (2, 3),
    (2, 5),
    -- Västra Hamnen B&B
    (3, 1),
    (3, 4),
    (3, 6),
    -- Grand Hotel Domkyrkan
    (4, 2),
    (4, 3),
    (4, 5),
    -- Lundagård Vandrarhem (dorms + private rooms)
    (5, 3),
    (5, 5);

-- Bookings (25 bookings, total_price = nights * price_per_night)
-- room_id refers to the rooms above in insert order (1-22)
INSERT INTO `bookings` (`customer_number`, `room_id`, `checkin_date`, `checkout_date`, `total_price`, `cancelled`, `number_of_occupants`) VALUES
    (1, 2,  '2026-12-02', '2026-12-04', 2790.00,  FALSE,    2),
    (2, 4,  '2026-12-10', '2026-12-13', 5685.00,  FALSE,    4),
    (3, 11, '2026-12-03', '2026-12-05', 1790.00,  FALSE,    2),
    (4, 6,  '2026-12-08', '2026-12-10', 790.00,   FALSE,    4),
    (5, 15, '2026-12-12', '2026-12-14', 3190.00,  FALSE,    2),
    (1, 18, '2026-12-04', '2026-12-07', 8985.00,  FALSE,    4),
    (2, 1,  '2026-12-15', '2026-12-16', 1095.00,  FALSE,    1),
    (3, 13, '2026-12-11', '2026-12-14', 4185.00,  FALSE,    3),
    (4, 19, '2026-12-01', '2026-12-03', 700.00,   FALSE,    4),
    (5, 5,  '2026-12-26', '2026-12-31', 11475.00, FALSE,    4),
    (1, 9,  '2026-12-19', '2026-12-20', 595.00,   TRUE,     2),
    (2, 17, '2026-12-18', '2026-12-21', 7485.00,  FALSE,    3),
    (3, 3,  '2026-12-05', '2026-12-08', 4185.00,  FALSE,    2),
    (4, 14, '2026-12-21', '2026-12-23', 2590.00,  FALSE,    1),
    (5, 21, '2026-12-05', '2026-12-08', 1650.00,  FALSE,    2),
    (1, 12, '2026-12-23', '2026-12-27', 3580.00,  FALSE,    2),
    (2, 8,  '2026-12-12', '2026-12-14', 790.00,   TRUE,     3),
    (3, 2,  '2026-12-17', '2026-12-19', 2790.00,  FALSE,    2),
    (4, 16, '2026-12-28', '2026-12-31', 4785.00,  FALSE,    2),
    (5, 22, '2026-12-09', '2026-12-12', 1350.00,  FALSE,    1),
    (1, 4,  '2026-12-27', '2026-12-30', 5685.00,  FALSE,    3),
    (2, 20, '2026-12-06', '2026-12-08', 700.00,   FALSE,    4),
    (3, 10, '2026-12-01', '2026-12-04', 1485.00,  TRUE,     1),
    (4, 7,  '2026-12-19', '2026-12-22', 1185.00,  FALSE,    4),
    (5, 11, '2026-12-20', '2026-12-23', 2685.00,  FALSE,    2);
