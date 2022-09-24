using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TimeZoneNames;

internal class TimeZoneData
{
    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<string[]>))]
    public Dictionary<string, string[]> TzdbZoneCountries { get; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<string[]>))]
    public Dictionary<string, string[]> CldrZoneCountries { get; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<string>))]
    public Dictionary<string, string> CldrMetazones { get; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<string>))]
    public Dictionary<string, string> CldrPrimaryZones { get; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<string>))]
    public Dictionary<string, string> CldrAliases { get; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<CldrLanguageData>))]
    public Dictionary<string, CldrLanguageData> CldrLanguageData { get; } = new(StringComparer.OrdinalIgnoreCase);

    public List<TimeZoneSelectionData> SelectionZones { get; } = new();

    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<Dictionary<string, string>>))]
    public Dictionary<string, Dictionary<string, string>> DisplayNames { get; } = new(StringComparer.OrdinalIgnoreCase);

    [SecuritySafeCritical]
    public static TimeZoneData Load()
    {
        var assembly = typeof(TimeZoneData).GetTypeInfo().Assembly;
        using var compressedStream = assembly.GetManifestResourceStream("TimeZoneNames.data.json.gz");
        using var stream = new GZipStream(compressedStream!, CompressionMode.Decompress);
        return JsonSerializer.Deserialize<TimeZoneData>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}

internal class CldrLanguageData
{
    public TimeZoneValues Formats { get; set; }

    public string FallbackFormat { get; set; }
        
    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<TimeZoneValues>))]
    public Dictionary<string, TimeZoneValues> ShortNames { get; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<TimeZoneValues>))]
    public Dictionary<string, TimeZoneValues> LongNames { get; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<string>))]
    public Dictionary<string, string> CountryNames { get; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<string>))]
    public Dictionary<string, string> CityNames { get; } = new(StringComparer.OrdinalIgnoreCase);
}

internal class TimeZoneSelectionData
{
    public string Id { get; set; }

    public DateTime ThresholdUtc { get; set; }
}