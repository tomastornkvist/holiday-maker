using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using MySqlConnector;

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

        // Add code here...
    }

    private void AddBooking()
    {
        Booking booking = new();

        if (_currentCustomer is null)
        {
            ConsoleUI.ShowMessage("No customer selected. Go to Customer menu and select a customer first.");
            return;
        }

        booking.CustomerNumber = _currentCustomer.CustomerNumber;

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

        // Select dates
        DateOnly startDate, endDate;

        if (!DateOnly.TryParse(ConsoleUI.GetStringInput(
            "Enter start date: "), out startDate))
        {
            ConsoleUI.ShowMessage("Invalid start date.");
            return;
        }

        if (!DateOnly.TryParse(ConsoleUI.GetStringInput(
            "Enter end date: "), out endDate))
        {
            ConsoleUI.ShowMessage("Invalid end date.");
            return;
        }

        booking.StartDate = startDate;
        booking.EndDate = endDate;

        // Select number of people
        byte occupants;
        if (byte.TryParse(ConsoleUI.GetStringInput(
            "How many people will you be: "), out occupants))
        {
            booking.Occupants = occupants;
        }

        // Find available rooms

        // Select a room
        // Book the room
    }

    private void DeleteBooking()
    {

    }

    private string[] GetLocations()
    {
        MySqlDataReader reader = _db.ExecuteReader(Queries.FindLocations, []);
        List<string> locations = [];

        while (reader.Read())
            locations.Add(reader.GetString(0));

        return locations.ToArray();
    }
}