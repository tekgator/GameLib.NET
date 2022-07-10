using Gamelib.Util;
using Newtonsoft.Json;

namespace GameLib.Plugin.Epic.Model;

public class DeserializedEpicGame
{
    [JsonProperty("FormatVersion")]
    public long FormatVersion { get; set; }

    [JsonProperty("bIsIncompleteInstall")]
    public bool IsIncompleteInstall { get; set; }

    [JsonProperty("LaunchCommand")]
    public string LaunchCommand { get; set; } = string.Empty;

    [JsonProperty("LaunchExecutable")]
    public string LaunchExecutable { get; set; } = string.Empty;

    [JsonProperty("ManifestLocation")]
    public string ManifestLocation { get; set; } = string.Empty;

    [JsonProperty("bIsApplication")]
    public bool IsApplication { get; set; }

    [JsonProperty("bIsExecutable")]
    public bool IsExecutable { get; set; }

    [JsonProperty("bIsManaged")]
    public bool IsManaged { get; set; }

    [JsonProperty("bNeedsValidation")]
    public bool NeedsValidation { get; set; }

    [JsonProperty("bRequiresAuth")]
    public bool RequiresAuth { get; set; }

    [JsonProperty("bAllowMultipleInstances")]
    public bool AllowMultipleInstances { get; set; }

    [JsonProperty("bCanRunOffline")]
    public bool CanRunOffline { get; set; }

    [JsonProperty("bAllowUriCmdArgs")]
    public bool AllowUriCmdArgs { get; set; }

    [JsonProperty("BaseURLs")]
    public List<string> BaseUrLs { get; set; } = new();

    [JsonProperty("BuildLabel")]
    public string BuildLabel { get; set; } = string.Empty;

    [JsonProperty("AppCategories")]
    public List<string> AppCategories { get; set; } = new();

    [JsonProperty("DisplayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonProperty("InstallationGuid")]
    public string InstallationGuid { get; set; } = string.Empty;

    [JsonProperty("InstallLocation")]
    public string InstallLocation { get; set; } = string.Empty;

    [JsonProperty("InstallSessionId")]
    public string InstallSessionId { get; set; } = string.Empty;

    [JsonProperty("HostInstallationGuid")]
    public string HostInstallationGuid { get; set; } = string.Empty;

    [JsonProperty("StagingLocation")]
    public string StagingLocation { get; set; } = string.Empty;

    [JsonProperty("TechnicalType")]
    public string TechnicalType { get; set; } = string.Empty;

    [JsonProperty("VaultThumbnailUrl")]
    public string VaultThumbnailUrl { get; set; } = string.Empty;

    [JsonProperty("VaultTitleText")]
    public string VaultTitleText { get; set; } = string.Empty;

    [JsonProperty("InstallSize")]
    public long InstallSize { get; set; }

    [JsonProperty("MainWindowProcessName")]
    public string MainWindowProcessName { get; set; } = string.Empty;

    [JsonProperty("ProcessNames")]
    public List<string> ProcessNames { get; set; } = new();

    [JsonProperty("BackgroundProcessNames")]
    public List<string> BackgroundProcessNames { get; set; } = new();

    [JsonProperty("MandatoryAppFolderName")]
    public string MandatoryAppFolderName { get; set; } = string.Empty;

    [JsonProperty("OwnershipToken")]
    public string OwnershipToken { get; set; } = string.Empty;

    [JsonProperty("CatalogNamespace")]
    public string CatalogNamespace { get; set; } = string.Empty;

    [JsonProperty("CatalogItemId")]
    public string CatalogItemId { get; set; } = string.Empty;

    [JsonProperty("AppName")]
    public string AppName { get; set; } = string.Empty;

    [JsonProperty("AppVersionString")]
    public string AppVersionString { get; set; } = string.Empty;

    [JsonProperty("MainGameCatalogNamespace")]
    public string MainGameCatalogNamespace { get; set; } = string.Empty;

    [JsonProperty("MainGameCatalogItemId")]
    public string MainGameCatalogItemId { get; set; } = string.Empty;

    [JsonProperty("MainGameAppName")]
    public string MainGameAppName { get; set; } = string.Empty;

    public EpicGame EpicGameBuilder() => new()
    {
        Id = AppName,
        Name = DisplayName,
        InstallDir = InstallLocation,
        Executable = PathUtil.IsExecutable(MainWindowProcessName) ? MainWindowProcessName : LaunchExecutable,
        WorkingDir = InstallLocation,
        InstallSize = InstallSize,
        Version = AppVersionString,
    };
}
