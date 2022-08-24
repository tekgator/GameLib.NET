using GameLib;
using System.Diagnostics;

var launcherManager = new LauncherManager();

foreach (var launcher in launcherManager.GetLaunchers())
{
    var name = launcher.Name;
    SetConsoleColor(ConsoleColor.White, ConsoleColor.Red);
    Console.WriteLine($"\n{name}:");
    ResetConsoleColor();

    foreach (var item in launcher.GetType().GetProperties().Where(p => p.Name != "Name"))
    {
        Console.WriteLine($"\t{item.Name}: {item.GetValue(launcher)}");
    }

    var games = launcher.GetGames();
    SetConsoleColor(ConsoleColor.Green);
    Console.WriteLine("\n\tGames:");
    ResetConsoleColor();
    foreach (var game in games)
    {
        SetConsoleColor(ConsoleColor.Magenta);
        Console.WriteLine($"\tGame ID: {game.Id}");
        ResetConsoleColor();
        foreach (var item in game.GetType().GetProperties().Where(p => p.Name != "GameId"))
        {
            Console.WriteLine($"\t\t{item.Name}: {item.GetValue(game)}");
        }
    }
}

static void SetConsoleColor(ConsoleColor foregroundColor, ConsoleColor? backgroundColor = null)
{
    Console.ForegroundColor = foregroundColor;
    if (backgroundColor is not null)
        Console.BackgroundColor = (ConsoleColor)backgroundColor;
}

static void ResetConsoleColor()
{
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.BackgroundColor = ConsoleColor.Black;
}

Debugger.Break();