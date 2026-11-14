using System.Text;

class HolidayMaker(Database db)
{
    private readonly Database _db = db;
    private Customer? _currentCustomer = null;

    public void Run()
    {
        while (true)
        {
            char input = ConsoleUI.Menu("Main menu",
            [
                ('1', "Customer menu"),
                ('2', "Booking menu"),
                ('X', "Exit")
            ]);

            switch (input)
            {
                case '1':
                    CustomerMenu();
                    break;

                case '2':
                    BookingMenu();
                    break;

                case 'X':
                    return;
            }
        }
    }

    private void CustomerMenu()
    {
        while (true)
        {
            char input = ConsoleUI.Menu("Customer menu",
            [
                ('1', "Show customers"),
                ('2', "Select customer"),
                ('3', "Add customer"),
                ('4', "Delete customer"),
                ('X', "Return to previous menu")
            ]);

            switch (input)
            {
                case '1':
                    ShowCustomers();
                    break;

                case '2':
                    SelectCustomer();
                    break;

                case '3':
                    AddCustomer();
                    break;

                case '4':
                    DeleteCustomer();
                    break;

                case 'X':
                    return;
            }
        }
    }

    private void BookingMenu()
    {
        while (true)
        {
            char input = ConsoleUI.Menu("Booking menu",
            [
                ('1', "Show bookings"),
                ('2', "Add booking"),
                ('3', "Delete booking"),
                ('X', "Return to previous menu")
            ]);

            switch (input)
            {
                case '1':
                    ShowBookings();
                    break;

                case '2':
                    AddBooking();
                    break;

                case '3':
                    DeleteBooking();
                    break;

                case 'X':
                    return;
            }
        }
    }

    // ---------------------------------------------------------------------------------------
    // Customer functions

    private List<Customer> GetCustomers()
    {
        using var reader = _db.ExecuteReader(Queries.SelectCustomers, []);

        List<Customer> customers = [];

        while (reader.Read())
            customers.Add(
                new(reader.GetUInt64("customer_number"),
                    reader.GetString("name"),
                    reader.GetString("address"),
                    reader.GetString("email"),
                    reader.GetString("phone")));

        return customers;
    }

    private Customer? GetCustomer(ulong customerNumber)
    {
        using var reader = _db.ExecuteReader(Queries.SelectCustomer,
            [("@customer_number", customerNumber)]);

        Customer? customer = null;

        if (reader.Read())
            customer = new(
                reader.GetUInt64("customer_number"),
                reader.GetString("name"),
                reader.GetString("address"),
                reader.GetString("email"),
                reader.GetString("phone"));

        return customer;
    }

    private void ShowCustomers()
    {
        ConsoleUI.ShowList("Customers", GetCustomers().Select(
            customer => customer.ToString()).ToArray());
    }

    private void SelectCustomer()
    {
        ShowCustomers();

        if (ulong.TryParse(
            ConsoleUI.GetStringInput("Select customer: "),
            out ulong customerNumber))
        {
            _currentCustomer = GetCustomer(customerNumber);
            if (_currentCustomer is null)
                ConsoleUI.ShowMessage($"Customer ({customerNumber}) doesn't exist. No customer selected.");
            else
                ConsoleUI.ShowMessage($"Selected customer {_currentCustomer}.");
        }
        else
            ConsoleUI.ShowMessage("Invalid customer number.");
    }

    private void AddCustomer()
    {
        string? name = ConsoleUI.GetStringInput("Enter customer name: ");
        if (name is null)
        {
            ConsoleUI.ShowMessage("Name cannot be omitted. Customer was not added.");
            return;
        }

        string? address = ConsoleUI.GetStringInput("Enter customer address: ");
        if (address is null)
        {
            ConsoleUI.ShowMessage("Address cannot be omitted. Customer was not added.");
            return;
        }

        string? email = ConsoleUI.GetStringInput("Enter customer email: ");
        if (email is null)
        {
            ConsoleUI.ShowMessage("Email cannot be omitted. Customer was not added.");
            return;
        }

        string? phone = ConsoleUI.GetStringInput("Enter customer phone: ");
        if (phone is null)
        {
            ConsoleUI.ShowMessage("Phone cannot be omitted. Customer was not added.");
            return;
        }

        (string, object)[] parameters =
        [
            ("@name", name),
            ("@address", address),
            ("@email", email),
            ("@phone", phone)
        ];

        if (_db.ExecuteNonQuery(Queries.InsertCustomer, parameters) == 1)
            ConsoleUI.ShowMessage($"Added customer '{name}'.");
        else
            ConsoleUI.ShowMessage($"Failed to add customer {name}.");
    }

    private void DeleteCustomer()
    {
        ShowCustomers();

        if (ulong.TryParse(
            ConsoleUI.GetStringInput(
                "Enter the customer number of the customer you want to delete: "),
            out ulong customerNumber))
        {
            if (GetCustomer(customerNumber) is null)
            {
                ConsoleUI.ShowMessage($"Customer ({customerNumber}) doesn't exist.");
                return;
            }

            (string, object)[] parameters = [("@customer_number", customerNumber)];

            if (_db.ExecuteNonQuery(Queries.DeleteCustomer, parameters) == 1)
                ConsoleUI.ShowMessage($"Customer ({customerNumber}) was deleted.");
            else
                ConsoleUI.ShowMessage($"Failed to delete customer ({customerNumber}).");
        }
    }

    // ---------------------------------------------------------------------------------------
    // Booking functions

    private void ShowBookings()
    {
        if (_currentCustomer is null)
        {
            ConsoleUI.ShowMessage("No customer selected. Go to Customer menu and select a customer first.");
            return;
        }


    }

    private void AddBooking()
    {
        if (_currentCustomer is null)
        {
            ConsoleUI.ShowMessage("No customer selected. Go to Customer menu and select a customer first.");
            return;
        }

        // Select location
        List<(char, string)> locations = [];
        char c = 'A';
        foreach (string location in GetLocations())
            locations.Add((c++, location));
        locations.Add(('X', "Cancel"));
        c = ConsoleUI.Menu("Locations", locations.ToArray());

        if (c == 'X')
            return;

        string selectedLocation = locations.Find(l => l.Item1 == c).Item2;
        ConsoleUI.ShowMessage($"Location: {selectedLocation}");

        // Select number of people
        if (!byte.TryParse(ConsoleUI.GetStringInput("How many people will you be (1): "), out byte occupants))
            occupants = 1;

        // Select dates
        if (!DateOnly.TryParse(ConsoleUI.GetStringInput(
            "Enter checkin date: "), out DateOnly checkinDate))
        {
            ConsoleUI.ShowMessage("Invalid checkin date.");
            return;
        }
        if (!DateOnly.TryParse(ConsoleUI.GetStringInput(
            "Enter checkout date: "), out DateOnly checkoutDate))
        {
            ConsoleUI.ShowMessage("Invalid checkout date.");
            return;
        }
        if (checkoutDate <= checkinDate)
        {
            ConsoleUI.ShowMessage($"Checkout date ({checkoutDate}) must be after checkin date ({checkinDate}).");
            return;
        }

        // Select distance to the beach
        if (!uint.TryParse(ConsoleUI.GetStringInput(
            "Select maximum distance to the beach (0 or no entry ignores it): "),
            out uint distanceToBeach))
        {
            distanceToBeach = 0;
        }

        // Select distance to the city
        if (!uint.TryParse(ConsoleUI.GetStringInput(
            "Select maximum distance to the city centre (0 or no entry ignores it): "),
            out uint distanceToCity))
        {
            distanceToCity = 0;
        }

        // Should the hotel have a pool?
        bool hasPool = false;
        string? input = ConsoleUI.GetStringInput(
            "Should the hotel have a pool? (Yes/[No]): ");

        if (input is not null && input.ToUpper()[0] == 'Y')
            hasPool = true;

        // Should the hotel have a restaurant?
        bool hasRestaurant = false;
        input = ConsoleUI.GetStringInput(
            "Should the hotel have a restaurant? (Yes/[No]): ");

        if (input is not null && input.ToUpper()[0] == 'Y')
            hasRestaurant = true;

        // How should the results be sorted?
        c = ConsoleUI.Menu("How do you want the results sorted?",
        [
            ('S', "Standard (City, hotel name and room number)"),
            ('P', "Price/night"),
            ('R', "Hotel rating")
        ]);

        Queries.OrderByFindAvailableRooms orderBy;
        switch (c)
        {
            case 'P':
                orderBy = Queries.OrderByFindAvailableRooms.PricePerNight;
                break;

            case 'R':
                orderBy = Queries.OrderByFindAvailableRooms.Rating;
                break;

            default:
                orderBy = Queries.OrderByFindAvailableRooms.Standard;
                break;
        }

        // Find available rooms
        List<(Hotel hotel, Room room)> availableRooms =
            FindAvailableRooms(
                selectedLocation,
                occupants,
                checkinDate,
                checkoutDate,
                distanceToBeach,
                distanceToCity,
                hasPool,
                hasRestaurant,
                orderBy);

        // Show available rooms and select one
        string title =
        "       Hotel                          Location             Room  Capacity Price      ";
        string[] choices = availableRooms.Select(
            r =>
            $"|{r.hotel.Name,-30}|" +
            $"{r.hotel.Location,-20}|" +
            $"{r.room.RoomNumber,5}|" +
            $"{r.room.Capacity,8}|" +
            $"{r.room.PricePerNight,10}|").ToArray();
        int i = ConsoleUI.UnknownLengthMenu(title, choices);

        // Book the room
        i--;
        Hotel hotel = availableRooms[i].hotel;
        Room room = availableRooms[i].room;

        (string, object)[] parameters =
        [
            ("@customer_number", _currentCustomer.CustomerNumber),
            ("@room_id", room.Id),
            ("@checkin_date", checkinDate),
            ("@checkout_date", checkoutDate),
            ("@total_price", room.PricePerNight * (checkoutDate.DayNumber - checkinDate.DayNumber)),
            ("@cancelled", false),
            ("@number_of_occupants", occupants)
        ];

        if (_db.ExecuteNonQuery(Queries.InsertBooking, parameters) == 1)
            ConsoleUI.ShowMessage(
                @$"Added booking for {_currentCustomer.Name},
                at {hotel.Name} in {hotel.Location}
                from {checkinDate} to {checkoutDate}.");
        else
            ConsoleUI.ShowMessage($"Failed to add booking for {_currentCustomer.Name}");
    }

    private void DeleteBooking()
    {

    }

    private string[] GetLocations()
    {
        using var reader = _db.ExecuteReader(Queries.FindLocations, []);
        List<string> locations = [];

        while (reader.Read())
            locations.Add(reader.GetString(0));

        return locations.ToArray();
    }

    private List<(Hotel hotel, Room room)> FindAvailableRooms(
        string location,
        byte occupants,
        DateOnly check_in_date,
        DateOnly check_out_date,
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
            ("@check_in_date", check_in_date),
            ("@check_out_date", check_out_date)
        ];

        if (distanceToBeach > 0)
        {
            parameters.Add(("@distance_to_beach", distanceToBeach));
            sql.Append(Queries.FindAvailableRoomsAndDistanceToBeach);
        }

        if (distanceToCity > 0)
        {
            parameters.Add(("@distance_to_city", distanceToCity));
            sql.Append(Queries.FindAvailableRoomsAndDistanceToCity);
        }

        if (hasPool)
            sql.Append(Queries.FindAvailableRoomsHasPool);

        if (hasRestaurant)
            sql.Append(Queries.FindAvailableRoomsHasRestaurant);

        sql.Append(Queries.FindAvailableRoomsWhere);

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
                new(new(reader.GetUInt64("hotel_id"),
                        reader.GetString("name"),
                        reader.GetString("location"),
                        reader.GetUInt32("distance_to_beach"),
                        reader.GetUInt32("distance_to_city"),
                        reader.GetDecimal("rating")),
                    new(reader.GetUInt64("room_id"),
                        reader.GetDecimal("price_per_night"),
                        reader.GetByte("capacity"),
                        reader.GetUInt64("hotel_id"),
                        reader.GetUInt32("room_number"))
                ));
        }
        return rooms;
    }
}