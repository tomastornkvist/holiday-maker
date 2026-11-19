
using MySqlConnector;

class Database : IDisposable
{
    private const string _connectionString =
    "Server=localhost;Database=holiday_maker;User ID=root;Password=dUckp0ndmYrS!;";
    public MySqlConnection Connection { get; }

    public void Dispose()
    {
        Connection.Dispose();
    }

    public Database()
    {
        Connection = new(_connectionString);
        Connection.Open();
    }

    public void EnsureOpen()
    {
        if (Connection.State == System.Data.ConnectionState.Closed)
            Connection.Open();
    }

    // Simplify select queries.
    public MySqlDataReader ExecuteReader(
        string sql,
        (string, object)[] parameters,
        MySqlTransaction? transaction = null)
    {
        using MySqlCommand command = new(sql, Connection, transaction);

        foreach ((string name, object value) in parameters)
            command.Parameters.AddWithValue(name, value);
        command.Prepare();

        return command.ExecuteReader();
    }

    // Simplify non-select queries.
    public int ExecuteNonQuery(
        string sql,
        (string, object)[] parameters,
        MySqlTransaction? transaction = null)
    {
        using MySqlCommand command = new(sql, Connection, transaction);

        foreach ((string name, object value) in parameters)
            command.Parameters.AddWithValue(name, value);
        command.Prepare();

        return command.ExecuteNonQuery();
    }
}
