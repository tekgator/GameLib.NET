using Newtonsoft.Json;

namespace GameLib.Plugin.RiotGames.Model;

public class YamlData
{
    [JsonProperty("auto_patching_enabled_by_player")]
    public bool AutoPatchingEnabledByPlayer { get; set; }

    [JsonProperty("locale_data")] 
    public LocaleData LocaleData { get; set; } = new();

    [JsonProperty("patching_policy")]
    public string PatchingPolicy { get; set; } = string.Empty;

    [JsonProperty("patchline_patching_ask_policy")]
    public string PatchlinePatchingAskPolicy { get; set; } = string.Empty;

    [JsonProperty("product_install_full_path")]
    public string ProductInstallFullPath { get; set; } = string.Empty;

    [JsonProperty("product_install_root")]
    public string ProductInstallRoot { get; set; } = string.Empty;

    [JsonProperty("settings")] 
    public Settings Settings { get; set; } = new();

    [JsonProperty("shortcut_name")]
    public string ShortcutName { get; set; } = string.Empty;

    [JsonProperty("should_repair")]
    public bool ShouldRepair { get; set; }
}

public class LocaleData
{
    [JsonProperty("available_locales")] 
    public string[] AvailableLocales { get; set; } = Array.Empty<string>();

    [JsonProperty("default_locale")]
    public string DefaultLocale { get; set; } = string.Empty;
}

public class Settings
{
    [JsonProperty("create_shortcut")]
    public bool CreateShortcut { get; set; }

    [JsonProperty("create_uninstall_key")]
    public bool CreateUninstallKey { get; set; }

    [JsonProperty("locale")] 
    public string Locale { get; set; } = string.Empty;
}