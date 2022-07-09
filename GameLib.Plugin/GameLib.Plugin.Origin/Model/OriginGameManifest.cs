using System.Xml.Serialization;

namespace GameLib.Plugin.Origin.Model;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Only used for deserialization")]
[XmlRoot("game")]
internal class OriginGameManifest
{
    public class LocaleInfo
    {
        public string? title { get; set; }

        [XmlAttribute()]
        public string? locale { get; set; }
    }

    public class Metadata
    {
        [XmlElement("localeInfo")]
        public LocaleInfo[]? localeInfo { get; set; }
    }

    [XmlArrayItem("contentID")]
    public string[]? contentIDs { get; set; }

    public Metadata? metadata { get; set; }
}