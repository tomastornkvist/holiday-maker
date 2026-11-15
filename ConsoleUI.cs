static class ConsoleUI
{
    // Show a list.
    public static void ShowList(string title, string[] lines)
    {
        Console.WriteLine(title);
        Console.WriteLine(new string('-', title.Length));

        foreach (string line in lines)
            Console.WriteLine(line);
    }

    // Show a menu and get the users response.
    public static char Menu(string title, (char, string)[] choices)
    {
        while (true)
        {
            ShowList(title, choices.Select(c => $"{c.Item1}. {c.Item2}").ToArray());
            Console.Write("Make your choice: ");

            if (char.TryParse(Console.ReadLine(), out char input))
            {
                input = char.ToUpper(input);
                if (choices.Any(c => c.Item1 == input))
                    return input;
            }
        }
    }

    // Show a menu and get the users response.
    public static int UnknownLengthMenu(string title, string[] choices)
    {
        while (true)
        {
            int i = 1;
            ShowList(title, choices.Select(c => $"{i++,4}. {c}").ToArray());
            Console.Write("Make your choice: ");

            if (int.TryParse(Console.ReadLine(), out int input))
            {
                if (input > 0 && input <= choices.Length)
                    return input;
            }
        }
    }

    // Show a message and get a reply to it.
    public static string? GetStringInput(string message)
    {
        Console.Write(message);

        string? input = Console.ReadLine();

        return string.IsNullOrWhiteSpace(input) ? null : input.Trim();
    }

    // Show a message.
    public static void ShowMessage(string message)
    {
        Console.WriteLine(message);
    }
}
