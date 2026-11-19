using MySqlConnector;

internal class Program : IDisposable
{
    private HolidayMaker _hm;
    private Database _db;

    private static void Main(string[] args)
    {
        Program? program = null;
        try
        {
            program = new();
            program.Run();
        }
        catch (MySqlException ex)
        {
            ConsoleUI.ShowMessage(ex.Message);
            switch (ex.ErrorCode)
            {
                case MySqlErrorCode.AccessDenied:
                    ConsoleUI.ShowMessage("Could not log in to the database. Check user name/password.");
                    break;

                case MySqlErrorCode.UnknownDatabase:
                    ConsoleUI.ShowMessage("Database 'holiday_maker' does not exist. Run holiday-maker-create.sql first.");
                    break;

                case MySqlErrorCode.UnableToConnectToHost:
                    ConsoleUI.ShowMessage("Cannot reach the MySQL server. Is it running?");
                    break;
            }
        }
        catch (Exception e)
        {
            ConsoleUI.ShowMessage(e.Message);
        }
        finally
        {
            program?.Dispose();
        }
    }

    public Program()
    {
        _db = new();
        _hm = new(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    public void Run()
    {
        while (true)
        {
            ConsoleUI.ShowMessage("");
            char input = ConsoleUI.Menu("Main menu",
            [
                ('A', "Add customer"),
                ('B', "Add booking"),
                ('S', "Show bookings"),
                ('C', "Change booking"),
                ('D', "Delete booking"),
                ('X', "Exit")
            ]);

            try
            {
                switch (input)
                {
                    case 'A':
                        AddCustomer();
                        break;

                    case 'B':
                        AddBooking();
                        break;

                    case 'S':
                        ShowBookings();
                        break;

                    case 'C':
                        ChangeBooking();
                        break;

                    case 'D':
                        RemoveBooking();
                        break;

                    case 'X':
                        return;
                }

            }
            catch (MySqlException ex)
            {
                ConsoleUI.ShowMessage($"Database error: {ex.Message}. The operation was cancelled.");
                _db.EnsureOpen();
            }
            catch (Exception e)
            {
                ConsoleUI.ShowMessage(e.Message);
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
        if (!_hm.IsEmailUnique(email))
        {
            ConsoleUI.ShowMessage("There is already a customer with that email. Customer was not added.");
            return null;
        }

        string? phone = ConsoleUI.GetStringInput("Enter customer phone: ");
        if (phone is null)
        {
            ConsoleUI.ShowMessage("Phone cannot be omitted. Customer was not added.");
            return null;
        }
        if (!_hm.IsPhoneUnique(phone))
        {
            ConsoleUI.ShowMessage("There is already a customer with that phone number. Customer was not added.");
            return null;
        }

        try
        {
            if (_hm.InsertCustomer(name, address, email, phone))
            {
                ConsoleUI.ShowMessage($"Added customer '{name}'.");

                if (_hm.GetLastInsertedId(out ulong customerNumber))
                    return _hm.GetCustomer(customerNumber);
            }
            else
                ConsoleUI.ShowMessage($"Failed to add customer {name}.");
        }
        catch (MySqlException ex)
        {
            ConsoleUI.ShowMessage($"Failed to add customer. {ex.Message}");
        }
        return null;
    }

    private void AddBooking()
    {
        string? input;

        Customer? customer = SelectCustomer();

        if (customer is null)
        {
            ConsoleUI.ShowMessage("No customer selected.");
            input = ConsoleUI.GetStringInput("Do you want to add a new customer? (Y/[N]): ");

            if (input is null || input.ToUpper()[0] != 'Y')
                return;

            customer = AddCustomer();

            if (customer is null)
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
            "How many people will you be [1]: "),
            out byte occupants) || occupants < 1)
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
        if (checkinDate < DateOnly.FromDateTime(DateTime.Now))
        {
            ConsoleUI.ShowMessage("Checkin date can not be in the past.");
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
            customer.CustomerNumber,
            room.Id,
            checkinDate,
            checkoutDate,
            room.PricePerNight * (checkoutDate.DayNumber - checkinDate.DayNumber),
            false,
            occupants
        );

        if (_hm.InsertBooking(booking, customer, room))
            ConsoleUI.ShowMessage(
                @$"Added booking for {customer.Name},
                at {hotel.Name} in {hotel.Location}
                from {checkinDate} to {checkoutDate}.");
        else
            ConsoleUI.ShowMessage($"Failed to add booking for {customer.Name}");
    }

    private Customer? SelectCustomer()
    {
        string? customerName = ConsoleUI.GetStringInput("Enter customer name: ");

        if (customerName is null)
        {
            ConsoleUI.ShowMessage("Invalid name.");
            return null;
        }

        List<Customer> customers = _hm.FindCustomers(customerName);

        if (customers.Count == 0) return null;
        if (customers.Count == 1) return customers[0];

        int index = ConsoleUI.UnknownLengthMenu(
            "Select a customer",
            customers.Select(c => c.Name).ToArray());

        return customers[--index];
    }

    private Customer? ShowBookings()
    {
        Customer? customer = SelectCustomer();

        if (customer is null)
        {
            ConsoleUI.ShowMessage("No customer selected.");
            return null;
        }

        List<(Booking booking, Room room, Hotel hotel)> bookings =
        _hm.GetBookings(customer.CustomerNumber);

        string title =
            "Booking number " +
            "Hotel                          " +
            "Location             " +
            "Room  " +
            "Capacity " +
            "Checkin    " +
            "Checkout   " +
            "Occupants " +
            "Price     ";
        string[] bookingRows = bookings.Select(
            b =>
            $"{b.booking.BookingNumber,14} " +
            $"{b.hotel.Name,-30} " +
            $"{b.hotel.Location,-20} " +
            $"{b.room.RoomNumber,5} " +
            $"{b.room.Capacity,8} " +
            $"{b.booking.CheckinDate} " +
            $"{b.booking.CheckoutDate} " +
            $"{b.booking.Occupants,9} " +
            $"{b.booking.TotalPrice,10}").ToArray();

        ConsoleUI.ShowMessage(
            $"Bookings for {customer.Name}");
        ConsoleUI.ShowList(title, bookingRows);

        return customer;
    }

    private void ChangeBooking()
    {
        Customer? customer = ShowBookings();

        if (customer is null)
            return;

        if (!ulong.TryParse(
            ConsoleUI.GetStringInput("Enter booking number: "),
            out ulong bookingNumber))
        {
            ConsoleUI.ShowMessage("Invalid booking number.");
            return;
        }

        (Booking booking, Room room, Hotel hotel)? booking = _hm.GetBooking(bookingNumber);
        if (booking is null)
        {
            ConsoleUI.ShowMessage($"There's no booking with number: {bookingNumber}.");
            return;
        }

        if (booking.Value.booking.CustomerNumber != customer.CustomerNumber)
        {
            ConsoleUI.ShowMessage("Invalid booking number.");
            return;
        }

        if (!byte.TryParse(
            ConsoleUI.GetStringInput(
                $"How many occupants will you be? ({booking.Value.booking.Occupants}) " +
                $"(max {booking.Value.room.Capacity}) "),
                out byte occupants))
        {
            ConsoleUI.ShowMessage("Invalid number of occupants.");
            return;
        }

        if (occupants < 1)
        {
            ConsoleUI.ShowMessage("You are not allowed to have a room booked for nobody.");
            return;
        }

        if (occupants > booking.Value.room.Capacity)
        {
            ConsoleUI.ShowMessage($"{occupants} exceeds the capacity for the room.");
            return;
        }

        if (_hm.UpdateBookingOccupants(bookingNumber, occupants))
            ConsoleUI.ShowMessage(
                $"Changed number of occupants to {occupants} for booking: {bookingNumber}");
        else
            ConsoleUI.ShowMessage($"Failed to update booking: {bookingNumber}");
    }

    private void RemoveBooking()
    {
        Customer? customer = ShowBookings();

        if (customer is null)
            return;

        if (!ulong.TryParse(
            ConsoleUI.GetStringInput("Enter booking number: "),
            out ulong bookingNumber))
        {
            ConsoleUI.ShowMessage("Invalid booking number.");
            return;
        }

        (Booking booking, Room room, Hotel hotel)? booking = _hm.GetBooking(bookingNumber);
        if (booking is null)
        {
            ConsoleUI.ShowMessage($"There's no booking with number: {bookingNumber}.");
            return;
        }

        if (booking.Value.booking.CustomerNumber != customer.CustomerNumber)
        {
            ConsoleUI.ShowMessage("Invalid booking number.");
            return;
        }

        if (_hm.CancelBooking(bookingNumber))
            ConsoleUI.ShowMessage(
                $"Cancelled booking: {bookingNumber}");
        else
            ConsoleUI.ShowMessage($"Failed to cancel booking: {bookingNumber}");
    }
}
