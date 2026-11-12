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
    (2495.00, 5, 1, 301),
    -- Gamla Staden Hostel (dorms + private rooms)
    (295.00,  8, 2, 1),
    (325.00,  6, 2, 2),
    (395.00,  4, 2, 3),
    (595.00,  2, 2, 4),
    (495.00,  1, 2, 5),
    -- Västra Hamnen B&B
    (895.00,  2, 3, 1),
    (895.00,  2, 3, 2),
    (1195.00, 3, 3, 3),
    -- Grand Hotel Domkyrkan
    (1295.00, 1, 4, 101),
    (1595.00, 2, 4, 102),
    (1595.00, 2, 4, 103),
    (2195.00, 3, 4, 201),
    (2995.00, 4, 4, 301),
    -- Lundagård Vandrarhem (dorms + private rooms)
    (275.00,  6, 5, 1),
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
