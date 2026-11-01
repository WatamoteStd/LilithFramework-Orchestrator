
namespace Orchestrator.Utils;

public static class Log
{
    
    public static void Info(string message, string sender = null!) => Print(message, ConsoleColor.Gray, sender);
    public static void Success(string message, string sender = null!) => Print(message, ConsoleColor.Green, sender);
    public static void Warning(string message, string sender = null!) => Print(message, ConsoleColor.Yellow, sender);
    public static void Error(string message, string sender = null!) => Print(message, ConsoleColor.Red, sender);

    private static void Print(string message, ConsoleColor color, string sender)
    {
        string header = string.IsNullOrEmpty(sender) ? "" : $"[{sender}]";
        Console.ForegroundColor = color;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {header} : {message}");
        Console.ResetColor();

    }

}