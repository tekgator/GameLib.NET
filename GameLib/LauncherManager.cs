using GameLib.Core;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;

namespace GameLib;

public class LauncherManager
{
#pragma warning disable 0649
    [ImportMany(typeof(ILauncher))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add read only modifier", Justification = "read only cannot be applied for MEF framework to work")]
    private IEnumerable<ILauncher>? _launchers;
#pragma warning restore 0649

    [Export]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Field is used in MEF framework")]
    private LauncherOptions LauncherOptions { get; }

    public LauncherManager(LauncherOptions? options = null)
    {
        LauncherOptions = options ?? new LauncherOptions();
    }

    private void LoadPlugins()
    {
        if (_launchers is null)
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new DirectoryCatalog(path));

            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }

    /// <summary>
    /// Returns a list of all launcher (plugins)
    /// </summary>
    public IEnumerable<ILauncher> GetLaunchers()
    {
        if (_launchers is null)
        {
            Refresh();
        }
        return _launchers!;
    }

    /// <summary>
    /// Refresh all launcher plugins and returns the refreshed list
    /// </summary>
    public IEnumerable<ILauncher> Refresh(CancellationToken cancellationToken = default)
    {
        LoadPlugins();
        return _launchers!
            .AsParallel()
            .WithCancellation(cancellationToken)
            .Select(l => { l.Refresh(cancellationToken); return l; })
            .ToList();
    }

    /// <summary>
    /// Returns a List of Games from all launcher plugins
    /// </summary>
    public IEnumerable<IGame> GetAllGames()
    {
        return GetLaunchers()
            .SelectMany(launcher => launcher.Games)
            .ToList();
    }
}