class Customer
{
    public ulong CustomerNumber { get; }
    public string Name { get; }
    public string Address { get; }
    public string Email { get; }
    public string Phone { get; }

    public Customer(ulong customerNumber, string name, string address, string email, string phone)
    {
        CustomerNumber = customerNumber;
        Name = name;
        Address = address;
        Email = email;
        Phone = phone;
    }

    public override string ToString()
    {
        return $"({CustomerNumber}, {Name}, {Address}, {Email}, {Phone})";
    }
}