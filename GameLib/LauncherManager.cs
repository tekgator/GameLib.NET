using GameLib.Core;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;

namespace GameLib;

public class LauncherManager
{
    [ImportMany(typeof(ILauncher))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add read only modifier", Justification = "read only cannot be applied for MEF framework to work")]
    private IEnumerable<ILauncher>? _launchers;

    [Export]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Field is used in MEF framework")]
    private LauncherOptions LauncherOptions { get; }

    public LauncherManager(LauncherOptions? options = null)
    {
        LauncherOptions = options ?? new LauncherOptions();
    }

    private void LoadPlugins()
    {
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
        var catalog = new AggregateCatalog();
        catalog.Catalogs.Add(new DirectoryCatalog(path));

        var container = new CompositionContainer(catalog);
        container.ComposeParts(this);
    }

    public void ClearCache()
    {
        foreach (var launcher in GetLaunchers())
        {
            launcher.ClearCache();
        }
    }

    public IEnumerable<ILauncher> GetLaunchers()
    {
        if (_launchers is null)
        {
            LoadPlugins();
        }
        return _launchers!;
    }

    public IEnumerable<IGame> GetGames(CancellationToken cancellationToken = default)
    {
        return GetLaunchers()
            .AsParallel()
            .WithCancellation(cancellationToken)
            .SelectMany(launcher => launcher.GetGames(cancellationToken))
            .ToList();
    }
}