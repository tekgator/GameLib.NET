using Newtonsoft.Json;

namespace GameLib.Origin.Model;

internal class OriginOnlineManifest
{
    [JsonProperty("itemId")]
    public string? ItemId { get; set; }

    [JsonProperty("baseAttributes")]
    public BaseAttribute? BaseAttributes { get; set; }

    [JsonProperty("customAttributes")]
    public CustomAttribute? CustomAttributes { get; set; }

    [JsonProperty("localizableAttributes")]
    public LocalizableAttribute? LocalizableAttributes { get; set; }

    [JsonProperty("publishing")]
    public Publish? Publishing { get; set; }

    [JsonProperty("itemName")]
    public string? ItemName { get; set; }

    [JsonProperty("offerType")]
    public string? OfferType { get; set; }

    [JsonProperty("offerId")]
    public string? OfferId { get; set; }

    [JsonProperty("projectNumber")]
    public string? ProjectNumber { get; set; }

    public class BaseAttribute
    {
        [JsonProperty("platform")]
        public string? Platform { get; set; }
    }

    public class CustomAttribute
    {
        [JsonProperty("imageServer")]
        public string? ImageServer { get; set; }

        [JsonProperty("gameEditionTypeFacetKeyRankDesc")]
        public string? GameEditionTypeFacetKeyRankDesc { get; set; }
    }

    public class LocalizableAttribute
    {
        [JsonProperty("longDescription")]
        public string? LongDescription { get; set; }

        [JsonProperty("displayName")]
        public string? DisplayName { get; set; }

        [JsonProperty("shortDescription")]
        public string? ShortDescription { get; set; }

        [JsonProperty("packArtSmall")]
        public string? PackArtSmall { get; set; }

        [JsonProperty("packArtMedium")]
        public string? PackArtMedium { get; set; }

        [JsonProperty("packArtLarge")]
        public string? PackArtLarge { get; set; }
    }

    public class Publish
    {
        [JsonProperty("softwareList")]
        public SoftwareList? SoftwareList { get; set; }
    }

    public class SoftwareList
    {
        [JsonProperty("software")]
        public List<Software>? Software { get; set; }
    }

    public class Software
    {
        [JsonProperty("softwareId")]
        public string? SoftwareId { get; set; }

        [JsonProperty("fulfillmentAttributes")]
        public FulfillmentAttribute? FulfillmentAttributes { get; set; }

        [JsonProperty("softwarePlatform")]
        public string? SoftwarePlatform { get; set; }
    }

    public class FulfillmentAttribute
    {
        [JsonProperty("achievementSetOverride")]
        public string? AchievementSetOverride { get; set; }

        [JsonProperty("cloudSaveConfigurationOverride")]
        public string? CloudSaveConfigurationOverride { get; set; }

        [JsonProperty("downloadPackageType")]
        public string? DownloadPackageType { get; set; }

        [JsonProperty("enableDLCuninstall")]
        public bool EnableDlCuninstall { get; set; }

        [JsonProperty("executePathOverride")]
        public string? ExecutePathOverride { get; set; }

        [JsonProperty("installationDirectory")]
        public string? InstallationDirectory { get; set; }

        [JsonProperty("installCheckOverride")]
        public string? InstallCheckOverride { get; set; }

        [JsonProperty("monitorInstall")]
        public bool MonitorInstall { get; set; }

        [JsonProperty("monitorPlay")]
        public bool MonitorPlay { get; set; }

        [JsonProperty("multiPlayerId")]
        public string? MultiPlayerId { get; set; }

        [JsonProperty("showKeyDialogOnInstall")]
        public bool ShowKeyDialogOnInstall { get; set; }

        [JsonProperty("showKeyDialogOnPlay")]
        public bool ShowKeyDialogOnPlay { get; set; }

        [JsonProperty("processorArchitecture")]
        public string? ProcessorArchitecture { get; set; }
    }
}