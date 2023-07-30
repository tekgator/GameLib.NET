using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameLib.Plugin.RiotGames.Model
{
    public partial class YamlData
    {
        [JsonProperty("auto_patching_enabled_by_player")]
        public bool AutoPatchingEnabledByPlayer { get; set; }

        [JsonProperty("dependencies")]
        public Dependencies Dependencies { get; set; }

        [JsonProperty("locale_data")]
        public LocaleData LocaleData { get; set; }

        [JsonProperty("patching_policy")]
        public string PatchingPolicy { get; set; }

        [JsonProperty("patchline_patching_ask_policy")]
        public string PatchlinePatchingAskPolicy { get; set; }

        [JsonProperty("product_install_full_path")]
        public string ProductInstallFullPath { get; set; }

        [JsonProperty("product_install_root")]
        public string ProductInstallRoot { get; set; }

        [JsonProperty("settings")]
        public Settings Settings { get; set; }

        [JsonProperty("shortcut_name")]
        public string ShortcutName { get; set; }

        [JsonProperty("should_repair")]
        public bool ShouldRepair { get; set; }
    }

    public partial class Dependencies
    {
    }

    public partial class LocaleData
    {
        [JsonProperty("available_locales")]
        public string[] AvailableLocales { get; set; }

        [JsonProperty("default_locale")]
        public string DefaultLocale { get; set; }
    }

    public partial class Settings
    {
        [JsonProperty("create_shortcut")]
        public bool CreateShortcut { get; set; }

        [JsonProperty("create_uninstall_key")]
        public bool CreateUninstallKey { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }
    }
}
