using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

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

        var catalog = new AggregateCatalog();
        catalog.Catalogs.Add(new DirectoryCatalog("."));

        var container = new CompositionContainer(catalog);
        container.ComposeParts(this);
    }
}