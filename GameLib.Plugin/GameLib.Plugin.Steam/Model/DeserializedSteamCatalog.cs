using System.Collections.ObjectModel;

namespace GameLib.Plugin.Steam.Model;

internal class DeserializedSteamCatalog
{
    public uint AppID { get; set; }
    public uint Size { get; set; }
    public uint InfoState { get; set; }
    public DateTime LastUpdated { get; set; }
    public ulong Token { get; set; }
    public ReadOnlyCollection<byte>? Hash { get; set; }
    public uint ChangeNumber { get; set; }
    public DeserializedData Data { get; set; } = new();

    public class DeserializedData
    {
        public class DscdCommon
        {
            public string? Name { get; set; }
            public string? Type { get; set; }
            public string? OsList { get; set; }
            public string? OsArch { get; set; }
        }

        public class DscdExtended
        {
            public string? Developer { get; set; }
            public string? Developer_URL { get; set; }
            public string? Publisher { get; set; }
            public string? Homepage { get; set; }
            public string? GameManualUrl { get; set; }
        }

        public class DscdConfig
        {
            public string? ContentType { get; set; }
            public string? Installdir { get; set; }

            /* Used to be a list: public List<DscdLaunch>? Launch { get; set; }
             * According to the issue https://github.com/SteamDatabase/ValveKeyValue/issues/55 if the index of an
             * list/array cannot be guaranteed a dictionary should be used
             */
            public Dictionary<string, DscdLaunch>? Launch { get; set; }
        }
        public class DscdLaunch
        {
            public string? Executable { get; set; }
            public string? WorkingDir { get; set; }
            public string? Type { get; set; }
            public DscdLaunchConfig? Config { get; set; }
        }

        public class DscdLaunchConfig
        {
            public string? OsList { get; set; }
            public string? OsArch { get; set; }
        }

        public string? AppID { get; set; }
        public DscdCommon? Common { get; set; }
        public DscdExtended? Extended { get; set; }
        public DscdConfig? Config { get; set; }
    }
}
