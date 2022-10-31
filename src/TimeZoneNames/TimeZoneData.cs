using System.IO.Compression;
using System.Security;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TimeZoneNames;

internal class TimeZoneData
{
    public static readonly string[] ObsoleteWindowsZones =
    {
        "Mid-Atlantic Standard Time",
        "Kamchatka Standard Time"
    };

    public Dictionary<string, string[]> TzdbZoneCountries { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string[]> CldrZoneCountries { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string> CldrMetazones { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string> CldrPrimaryZones { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string> CldrAliases { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, CldrLanguageData> CldrLanguageData { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public List<TimeZoneSelectionData> SelectionZones { get; set; } = new();

    public Dictionary<string, IDictionary<string, string>> DisplayNames { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    [SecuritySafeCritical]
    public static TimeZoneData Load()
    {
        var assembly = typeof(TimeZoneData).Assembly;
        using var compressedStream = assembly.GetManifestResourceStream("TimeZoneNames.data.json.gz");
        using var stream = new GZipStream(compressedStream!, CompressionMode.Decompress);
        var timeZoneData = JsonSerializer.Deserialize(stream, TimeZoneDataContext.Default.TimeZoneData)!
                .ToCaseInsensitive();

        return timeZoneData;
    }

    private TimeZoneData ToCaseInsensitive() => new()
    {
        CldrAliases = new(CldrAliases, StringComparer.OrdinalIgnoreCase),
        CldrLanguageData = CldrLanguageData.ToDictionary(e => e.Key, e => e.Value.ToCaseInsensitive(), StringComparer.OrdinalIgnoreCase),
        CldrMetazones = new(CldrMetazones, StringComparer.OrdinalIgnoreCase),
        CldrPrimaryZones = new(CldrPrimaryZones, StringComparer.OrdinalIgnoreCase),
        CldrZoneCountries = new(CldrZoneCountries, StringComparer.OrdinalIgnoreCase),
        DisplayNames = new(DisplayNames, StringComparer.OrdinalIgnoreCase),
        TzdbZoneCountries = new(TzdbZoneCountries, StringComparer.OrdinalIgnoreCase),
        SelectionZones = SelectionZones
    };
}

internal class CldrLanguageData
{
    public TimeZoneValues Formats { get; set; }

    public string FallbackFormat { get; set; }

    public Dictionary<string, TimeZoneValues> ShortNames { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, TimeZoneValues> LongNames { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string> CountryNames { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string> CityNames { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    internal CldrLanguageData ToCaseInsensitive() => new()
    {
        Formats = Formats,
        FallbackFormat = FallbackFormat,
        ShortNames = new(ShortNames, StringComparer.OrdinalIgnoreCase),
        CityNames = new(CityNames, StringComparer.OrdinalIgnoreCase),
        CountryNames = new(CountryNames, StringComparer.OrdinalIgnoreCase),
        LongNames = new(LongNames, StringComparer.OrdinalIgnoreCase)
    };
}

internal class TimeZoneSelectionData
{
    public string Id { get; set; }

    public DateTime ThresholdUtc { get; set; }
}

[JsonSerializable(typeof(TimeZoneData), GenerationMode = JsonSourceGenerationMode.Metadata)]
internal partial class TimeZoneDataContext : JsonSerializerContext
{
}