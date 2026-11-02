namespace TimeZoneNames;

internal partial class TimeZoneData
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
}

internal class CldrLanguageData
{
    public TimeZoneValues? Formats { get; set; }

    public string? FallbackFormat { get; set; }

    public Dictionary<string, TimeZoneValues> ShortNames { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, TimeZoneValues> LongNames { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string> CountryNames { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string> CityNames { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

internal class TimeZoneSelectionData
{
    public string Id { get; set; } = null!;

    public DateTime ThresholdUtc { get; set; }
}
