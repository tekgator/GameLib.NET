using GameLib;
using System.Diagnostics;

var launcherManager = new LauncherManager();

foreach (var launcher in launcherManager.GetLaunchers())
{
    SetConsoleColor(ConsoleColor.White, ConsoleColor.Red);
    Console.WriteLine($"\n{launcher.Name}:");
    ResetConsoleColor();

    foreach (var property in launcher.GetType().GetProperties().Where(p => p.Name != "Name"))
    {
        Console.WriteLine($"\t{property.Name}: {property.GetValue(launcher)}");
    }

    SetConsoleColor(ConsoleColor.Green);
    Console.WriteLine("\n\tGames:");
    ResetConsoleColor();
    foreach (var game in launcher.Games)
    {
        SetConsoleColor(ConsoleColor.Magenta);
        Console.WriteLine($"\tGame ID: {game.Id}");
        ResetConsoleColor();
        foreach (var property in game.GetType().GetProperties().Where(p => p.Name != "GameId"))
        {
            Console.WriteLine($"\t\t{property.Name}: {property.GetValue(game)}");
        }
    }
}

static void SetConsoleColor(ConsoleColor foregroundColor, ConsoleColor? backgroundColor = null)
{
    Console.ForegroundColor = foregroundColor;
    if (backgroundColor is not null)
    {
        Console.BackgroundColor = (ConsoleColor)backgroundColor;
    }
}

static void ResetConsoleColor()
{
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.BackgroundColor = ConsoleColor.Black;
}

Debugger.Break();