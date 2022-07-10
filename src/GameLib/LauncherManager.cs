using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;

namespace GameLib;

public class LauncherManager
{
    [ImportMany(typeof(ILauncher))]
    public IEnumerable<ILauncher> Launchers { get; init; } = Enumerable.Empty<ILauncher>();

    [Export]
#pragma warning disable IDE0052 // Remove unread private members
    private LauncherOptions LauncherOptions { get; }
#pragma warning restore IDE0052 // Remove unread private members

    public LauncherManager(LauncherOptions? options = null)
    {
        LauncherOptions = options ?? new LauncherOptions();

        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
        var catalog = new AggregateCatalog();
        catalog.Catalogs.Add(new DirectoryCatalog(path));

        var container = new CompositionContainer(catalog);
        container.ComposeParts(this);
    }

    public void ClearCache()
    {
        foreach (var launcher in Launchers)
        {
            launcher.ClearCache();
        }
    }

    public IEnumerable<IGame> GetGames(CancellationToken cancellationToken = default)
    {
        return Launchers
            .AsParallel()
            .WithCancellation(cancellationToken)
            .SelectMany(launcher => launcher.GetGames(cancellationToken))
            .ToList();
    }
}