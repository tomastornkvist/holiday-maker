
using MySqlConnector;

class Database
{
    private string _connectionString;
    public MySqlConnection Connection { get; }

    public Database(string connectionString)
    {
        _connectionString = connectionString;
        Connection = new(_connectionString);
        Connection.Open();
    }

    // Simplify select queries.
    public MySqlDataReader ExecuteReader(string sql, (string, object)[] parameters)
    {
        using MySqlCommand command = new(sql, Connection);

        foreach ((string name, object value) in parameters)
            command.Parameters.AddWithValue(name, value);
        command.Prepare();

        return command.ExecuteReader();
    }

    // Simplify non-select queries.
    public int ExecuteNonQuery(string sql, (string, object)[] parameters)
    {
        using MySqlCommand command = new(sql, Connection);

        foreach ((string name, object value) in parameters)
            command.Parameters.AddWithValue(name, value);
        command.Prepare();

        return command.ExecuteNonQuery();
    }
}