using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Security;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TimeZoneNames;

internal class TimeZoneData
{
    [JsonInclude]
    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<string[]>))]
    public Dictionary<string, string[]> TzdbZoneCountries { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonInclude]
    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<string[]>))]
    public Dictionary<string, string[]> CldrZoneCountries { get; init;} = new(StringComparer.OrdinalIgnoreCase);

    [JsonInclude]
    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<string>))]
    public Dictionary<string, string> CldrMetazones { get; init;} = new(StringComparer.OrdinalIgnoreCase);

    [JsonInclude]
    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<string>))]
    public Dictionary<string, string> CldrPrimaryZones { get; init;} = new(StringComparer.OrdinalIgnoreCase);

    [JsonInclude]
    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<string>))]
    public Dictionary<string, string> CldrAliases { get; init;} = new(StringComparer.OrdinalIgnoreCase);

    [JsonInclude]
    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<CldrLanguageData>))]
    public Dictionary<string, CldrLanguageData> CldrLanguageData { get; init;} = new(StringComparer.OrdinalIgnoreCase);
    
    [JsonInclude]
    public List<TimeZoneSelectionData> SelectionZones { get; init;} = new();
    
    [JsonInclude]
    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<Dictionary<string, string>>))]
    public Dictionary<string, Dictionary<string, string>> DisplayNames { get; init;} = new(StringComparer.OrdinalIgnoreCase);

    [SecuritySafeCritical]
    public static TimeZoneData Load()
    {
        var assembly = typeof(TimeZoneData).Assembly;
        using var compressedStream = assembly.GetManifestResourceStream("TimeZoneNames.data.json.gz");
        using var stream = new GZipStream(compressedStream!, CompressionMode.Decompress);
        var timeZoneData = JsonSerializer.Deserialize<TimeZoneData>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        return timeZoneData;
    }
}

internal class CldrLanguageData
{
    [JsonInclude] 
    public TimeZoneValues Formats { get; set; }

    [JsonInclude] 
    public string FallbackFormat { get; set; }

    [JsonInclude]
    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<TimeZoneValues>))]
    public Dictionary<string, TimeZoneValues> ShortNames { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonInclude]
    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<TimeZoneValues>))]
    public Dictionary<string, TimeZoneValues> LongNames { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonInclude]
    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<string>))]
    public Dictionary<string, string> CountryNames { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonInclude]
    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<string>))]
    public Dictionary<string, string> CityNames { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

internal class TimeZoneSelectionData
{
    [JsonInclude]
    public string Id { get; init; }

    [JsonInclude]
    public DateTime ThresholdUtc { get; init; }
}