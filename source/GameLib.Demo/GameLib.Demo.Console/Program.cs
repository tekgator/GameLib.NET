using GameLib;
using GameLib.Plugin.Steam;

var launcherManager = new LauncherManager();

foreach (var launcher in launcherManager.Launchers)
{
    var name = launcher.Name;
    SetConsoleColor(ConsoleColor.White, ConsoleColor.Red);
    Console.WriteLine($"{name}:\n");
    ResetConsoleColor();

    var isInstalled = launcher.IsInstalled;
    Console.WriteLine($"Is installed: {isInstalled}");

    var isRunning = launcher.IsRunning;
    Console.WriteLine($"Is running: {isRunning}");

    var installDir = launcher.InstallDir;
    Console.WriteLine($"Install dir: {installDir}");

    var executablePath = launcher.ExecutablePath;
    Console.WriteLine($"Executable path: {executablePath}");

    var executable = launcher.Executable;
    Console.WriteLine($"Executable: {executable}");

    var libs = (launcher as SteamLauncher)?.GetLibraries();
    if (libs is not null)
    {
        SetConsoleColor(ConsoleColor.Blue);
        Console.WriteLine("\nLibraries:");
        ResetConsoleColor();
        foreach (var lib in libs)
        {
            Console.WriteLine($"Name: {lib.Name}");
            foreach (var item in lib.GetType().GetProperties().Where(p => p.Name != "Name"))
            {
                Console.WriteLine($"\t{item.Name}: {item.GetValue(lib)}");
            }
        }
    }

    var games = launcher.GetGames();
    SetConsoleColor(ConsoleColor.Green);
    Console.WriteLine("\nGames:");
    ResetConsoleColor();
    foreach (var game in games)
    {
        SetConsoleColor(ConsoleColor.Magenta);
        Console.WriteLine($"Game ID: {game.GameId}");
        ResetConsoleColor();
        foreach (var item in game.GetType().GetProperties().Where(p => p.Name != "GameId"))
        {
            Console.WriteLine($"\t{item.Name}: {item.GetValue(game)}");
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

Console.ReadKey();