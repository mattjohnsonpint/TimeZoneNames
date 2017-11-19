using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security;
using Newtonsoft.Json;

namespace TimeZoneNames
{
    internal class TimeZoneData
    {
        public Dictionary<string, string[]> TzdbZoneCountries { get; } = new Dictionary<string, string[]>();

        public Dictionary<string, string[]> CldrZoneCountries { get; } = new Dictionary<string, string[]>();

        public Dictionary<string, string> CldrMetazones { get; } = new Dictionary<string, string>();

        public Dictionary<string, string> CldrPrimaryZones { get; } = new Dictionary<string, string>();

        public Dictionary<string, string> CldrAliases { get; } = new Dictionary<string, string>();

        public Dictionary<string, CldrLanguageData> CldrLanguageData { get; } = new Dictionary<string, CldrLanguageData>();

        public Dictionary<string, Dictionary<string, string>> CldrWindowsMappings { get; } = new Dictionary<string, Dictionary<string, string>>();

        public List<TimeZoneSelectionData> SelectionZones { get; } = new List<TimeZoneSelectionData>();

        [SecuritySafeCritical]
        public static TimeZoneData Load()
        {
            var assembly = typeof(TimeZoneData).GetTypeInfo().Assembly;
            using (var compressedStream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.data.json.gz"))
            using (var stream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var reader = new StreamReader(stream))
            {
                var serializer = JsonSerializer.Create();
                return (TimeZoneData)serializer.Deserialize(reader, typeof(TimeZoneData));
            }
        }
    }

    internal class CldrLanguageData
    {
        public TimeZoneValues Formats { get; set; }

        public string FallbackFormat { get; set; }

        public Dictionary<string, TimeZoneValues> ShortNames { get; } = new Dictionary<string, TimeZoneValues>();

        public Dictionary<string, TimeZoneValues> LongNames { get; } = new Dictionary<string, TimeZoneValues>();

        public Dictionary<string, string> CountryNames { get; } = new Dictionary<string, string>();

        public Dictionary<string, string> CityNames { get; } = new Dictionary<string, string>();
    }

    internal class TimeZoneSelectionData
    {
        public string Id { get; set; }

        public DateTime ThresholdUtc { get; set; }
    }
}
