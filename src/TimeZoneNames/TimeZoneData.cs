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
        public Dictionary<string, string[]> TzdbZoneCountries { get; } = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, string[]> CldrZoneCountries { get; } = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, string> CldrMetazones { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, string> CldrPrimaryZones { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, string> CldrAliases { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, CldrLanguageData> CldrLanguageData { get; } = new Dictionary<string, CldrLanguageData>(StringComparer.OrdinalIgnoreCase);

        public List<TimeZoneSelectionData> SelectionZones { get; } = new List<TimeZoneSelectionData>();

        public Dictionary<string, Dictionary<string, string>> DisplayNames = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        [SecuritySafeCritical]
        public static TimeZoneData Load()
        {

#if NET35 || NET40
            var assembly = typeof(TimeZoneData).Assembly;
#else
            var assembly = typeof(TimeZoneData).GetTypeInfo().Assembly;
#endif
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

        public Dictionary<string, TimeZoneValues> ShortNames { get; } = new Dictionary<string, TimeZoneValues>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, TimeZoneValues> LongNames { get; } = new Dictionary<string, TimeZoneValues>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, string> CountryNames { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, string> CityNames { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    internal class TimeZoneSelectionData
    {
        public string Id { get; set; }

        public DateTime ThresholdUtc { get; set; }
    }
}
