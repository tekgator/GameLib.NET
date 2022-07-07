using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace GameLib;

public class LauncherManager
{
    private readonly LauncherOptions _options;

    [ImportMany(typeof(ILauncher))]
    public IEnumerable<ILauncher> Launchers { get; init; } = Enumerable.Empty<ILauncher>();

    [Export]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Field is used in MEF plugin framework")]
    private LauncherOptions LauncherOptions => _options;

    public LauncherManager(LauncherOptions? options = null)
    {
        _options = options ?? new LauncherOptions();

        var catalog = new AggregateCatalog();
        catalog.Catalogs.Add(new DirectoryCatalog("."));

        var container = new CompositionContainer(catalog);
        container.ComposeParts(this);
    }
}