using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using MySqlConnector;

class HolidayMaker(Database database)
{
    private readonly Database _database = database;
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
        using MySqlDataReader reader = _database.ExecuteReader(Queries.SelectCustomers, []);

        List<Customer> customers = [];

        while (reader.Read())
            customers.Add(
                new Customer(
                    reader.GetUInt64("customer_number"),
                    reader.GetString("name"),
                    reader.GetString("address"),
                    reader.GetString("email"),
                    reader.GetString("phone")));

        reader.DisposeAsync();

        return customers;
    }

    private Customer? GetCustomer(ulong customerNumber)
    {
        using MySqlDataReader reader = _database.ExecuteReader(
            Queries.SelectCustomer,
            [("@customer_number", customerNumber)]);

        if (reader.Read())
            return new Customer(
                reader.GetUInt64("customer_number"),
                reader.GetString("name"),
                reader.GetString("address"),
                reader.GetString("email"),
                reader.GetString("phone"));

        reader.DisposeAsync();

        return null;
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
            ConsoleUI.ShowMessage($"Invalid customer number.");
    }

    private void AddCustomer()
    {
        string? name = ConsoleUI.GetStringInput("Enter customer name: ");

        if (name is not null)
        {
            string? address = ConsoleUI.GetStringInput("Enter customer address: ");

            if (address is not null)
            {
                string? email = ConsoleUI.GetStringInput("Enter customer email: ");

                if (email is not null)
                {
                    string? phone = ConsoleUI.GetStringInput("Enter customer phone: ");

                    if (phone is not null)
                    {
                        (string, object)[] parameters =
                        [
                            ("@name", name),
                            ("@address", address),
                            ("@email", email),
                            ("@phone", phone)
                        ];

                        if (_database.ExecuteNonQuery(Queries.InsertCustomer, parameters) == 1)
                        {
                            ConsoleUI.ShowMessage($"Added customer '{name}'.");
                            return;
                        }
                    }
                    else
                        ConsoleUI.ShowMessage("Phone cannot be omitted. Customer was not added.");
                }
                else
                    ConsoleUI.ShowMessage("Email cannot be omitted. Customer was not added.");
            }
            else
                ConsoleUI.ShowMessage("Address cannot be omitted. Customer was not added.");
        }
        else
            ConsoleUI.ShowMessage("Name cannot be omitted. Customer was not added.");
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

            if (_database.ExecuteNonQuery(Queries.DeleteCustomer, parameters) == 1)
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

        // Select dates
        DateOnly startDate, endDate;

        if (!DateOnly.TryParse(ConsoleUI.GetStringInput("Enter start date: "), out startDate))
        {
            ConsoleUI.ShowMessage("Invalid start date.");
            return;
        }

        if (!DateOnly.TryParse(ConsoleUI.GetStringInput("Enter end date: "), out endDate))
        {
            ConsoleUI.ShowMessage("Invalid end date.");
            return;
        }

        // Select number of people

        // Find available rooms
        // Select a room
        // Book the room
    }

    private void DeleteBooking()
    {

    }

    private string[] GetLocations()
    {
        MySqlDataReader reader = _database.ExecuteReader(Queries.FindLocations, []);
        List<string> locations = [];

        while (reader.Read())
            locations.Add(reader.GetString(0));

        reader.DisposeAsync();

        return locations.ToArray();
    }
}