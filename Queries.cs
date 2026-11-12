static class Queries
{
    public static string SelectCustomers { get; } = "SELECT * FROM customers";
    public static string SelectCustomer { get; } = "SELECT * FROM customers WHERE customer_number = @customer_number";
    public static string InsertCustomer { get; } = "INSERT INTO customers(name, address, email, phone) VALUES(@name, @address, @email, @phone)";
    public static string DeleteCustomer { get; } = "DELETE FROM customers WHERE customer_number = @customer_number";
    public static string FindLocations { get; } = "SELECT DISTINCT location FROM hotels";
}