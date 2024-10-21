namespace BackgroundWorksRunner;

public static class ConsoleHelpers
{
    public static void WriteLine(string message, int line, int col)
    {
        var (lastCol, lastLine) = (Console.CursorLeft, Console.CursorTop);

        Console.CursorLeft = col;
        Console.CursorTop = line;
        Console.WriteLine(message);

        (Console.CursorLeft, Console.CursorTop) = (lastCol, lastLine);
    }
}