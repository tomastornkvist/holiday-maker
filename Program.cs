using MySqlConnector;

Database db = new("Server=localhost;Database=holiday_maker;User ID=root;Password=dUckp0ndmYrS!;");

HolidayMaker holidayMaker = new(db);
holidayMaker.Run();
