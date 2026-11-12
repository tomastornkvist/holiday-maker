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
    `start_date` DATE NOT NULL,
    `end_date` DATE NOT NULL,
    `total_price` DECIMAL(8, 2) NOT NULL,
    `status` ENUM('available', 'booked', 'cancelled') NOT NULL DEFAULT 'cancelled',
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