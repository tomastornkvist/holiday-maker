internal class Program
{
    private const string _connectionString =
    "Server=localhost;Database=holiday_maker;User ID=root;Password=dUckp0ndmYrS!;";
    private Database _db;
    private HolidayMaker _hm;
    private Customer? _currentCustomer = null;

    public Program()
    {
        _db = new(_connectionString);
        _hm = new(_db);
    }

    private static void Main(string[] args)
    {
        new Program().Run();
    }

    public void Run()
    {
        while (true)
        {
            char input = ConsoleUI.Menu("Main menu",
            [
                ('A', "Add customer"),
                ('B', "Add booking"),
                ('S', "Show bookings"),
                ('C', "Change booking"),
                ('D', "Delete booking"),
                ('R', "Restore booking"),
                ('X', "Exit")
            ]);

            switch (input)
            {
                case 'A':
                    AddCustomer();
                    break;

                case 'B':
                    // Change AddBooking() to search for customer
                    // and call AddCustomer() if not found.
                    // If not found...
                    // Then...
                    AddBooking();
                    break;

                case 'S':
                    // Implement ShowBookings().
                    ShowBookings();
                    break;

                case 'C':
                    // Implement ChangeBooking().
                    // ChangeBooking();
                    break;

                case 'D':
                    // Implement RemoveBooking().
                    RemoveBooking();
                    break;

                case 'R':
                    // Implement RestoreBooking().
                    // RestoreBooking();
                    break;

                case 'X':
                    return;
            }
        }
    }

    private Customer? AddCustomer()
    {
        string? name = ConsoleUI.GetStringInput("Enter customer name: ");
        if (name is null)
        {
            ConsoleUI.ShowMessage("Name cannot be omitted. Customer was not added.");
            return null;
        }

        string? address = ConsoleUI.GetStringInput("Enter customer address: ");
        if (address is null)
        {
            ConsoleUI.ShowMessage("Address cannot be omitted. Customer was not added.");
            return null;
        }

        string? email = ConsoleUI.GetStringInput("Enter customer email: ");
        if (email is null)
        {
            ConsoleUI.ShowMessage("Email cannot be omitted. Customer was not added.");
            return null;
        }

        string? phone = ConsoleUI.GetStringInput("Enter customer phone: ");
        if (phone is null)
        {
            ConsoleUI.ShowMessage("Phone cannot be omitted. Customer was not added.");
            return null;
        }

        if (_hm.InsertCustomer(name, address, email, phone))
            ConsoleUI.ShowMessage($"Added customer '{name}'.");
        else
            ConsoleUI.ShowMessage($"Failed to add customer {name}.");

        if (_hm.GetLastInsertedId(out ulong customerNumber))
            return _hm.GetCustomer(customerNumber);

        return null;
    }

    private void ShowCustomers()
    {
        ConsoleUI.ShowList("Customers", _hm.GetCustomers().Select(
            customer => customer.ToString()).ToArray());
    }

    private Customer? SelectCustomer()
    {
        Customer? customer = null;

        ShowCustomers();

        if (ulong.TryParse(
            ConsoleUI.GetStringInput("Select customer: "),
            out ulong customerNumber))
        {
            customer = _hm.GetCustomer(customerNumber);
        }

        return customer;
    }

    private void AddBooking()
    {
        string? input;

        _currentCustomer = SelectCustomer();

        if (_currentCustomer is null)
        {
            ConsoleUI.ShowMessage("No customer selected.");
            input = ConsoleUI.GetStringInput("Do you want to add a new customer? (Y/[N]): ");

            if (input is null || input.ToUpper()[0] != 'Y')
                return;

            _currentCustomer = AddCustomer();

            if (_currentCustomer is null)
            {
                ConsoleUI.ShowMessage("No customer selected. No booking added.");
                return;
            }
        }

        // Select location
        List<(char, string)> locations = [];
        char c = 'A';
        foreach (string location in _hm.GetLocations())
            locations.Add((c++, location));
        locations.Add(('X', "Cancel"));
        c = ConsoleUI.Menu("Locations", locations.ToArray());

        if (c == 'X')
            return;

        string selectedLocation = locations.Find(l => l.Item1 == c).Item2;
        ConsoleUI.ShowMessage($"Location: {selectedLocation}");

        // Select number of people
        if (!byte.TryParse(ConsoleUI.GetStringInput(
            "How many people will you be (1): "),
            out byte occupants))
        {
            occupants = 1;
        }

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
        input = ConsoleUI.GetStringInput(
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
            ('S', "Standard (Location, hotel name and room number)"),
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
            _hm.FindAvailableRooms(
                selectedLocation,
                occupants,
                checkinDate,
                checkoutDate,
                distanceToBeach,
                distanceToCity,
                hasPool,
                hasRestaurant,
                orderBy);

        if (availableRooms.Count < 1)
        {
            ConsoleUI.ShowMessage("There are no available rooms.");
            ConsoleUI.ShowMessage("Please change your search criteria.");
            return;
        }

        // Show available rooms and select one
        string title =
        "      Hotel                          Location              Room Capacity      Price";
        string[] choices = availableRooms.Select(
            r =>
            $"{r.hotel.Name,-30} " +
            $"{r.hotel.Location,-20} " +
            $"{r.room.RoomNumber,5} " +
            $"{r.room.Capacity,8} " +
            $"{r.room.PricePerNight,10}").ToArray();
        int i = ConsoleUI.UnknownLengthMenu(title, choices);

        // Book the room
        i--;
        Hotel hotel = availableRooms[i].hotel;
        Room room = availableRooms[i].room;
        Booking booking = new
        (
            0,
            _currentCustomer.CustomerNumber,
            room.Id,
            checkinDate,
            checkoutDate,
            room.PricePerNight * (checkoutDate.DayNumber - checkinDate.DayNumber),
            false,
            occupants
        );

        if (_hm.InsertBooking(booking, _currentCustomer, hotel, room))
            ConsoleUI.ShowMessage(
                @$"Added booking for {_currentCustomer.Name},
                at {hotel.Name} in {hotel.Location}
                from {checkinDate} to {checkoutDate}.");
        else
            ConsoleUI.ShowMessage($"Failed to add booking for {_currentCustomer.Name}");
    }

    private void ShowBookings()
    {
        if (_currentCustomer is null)
        {
            ConsoleUI.ShowMessage("No customer selected. Go to Customer menu and select a customer first.");
            return;
        }

        // Add some code here...
    }

    private void RemoveBooking()
    {

    }
}