using System;
using System.Collections.Generic;
using ProtoBuf;

namespace TimeZoneNames
{
    [ProtoContract]
    public class TimeZoneData
    {
        [ProtoMember(1)]
        public Dictionary<string, string[]> TzdbZoneCountries { get; } = new Dictionary<string, string[]>();

        [ProtoMember(2)]
        public Dictionary<string, string[]> CldrZoneCountries { get; } = new Dictionary<string, string[]>();

        [ProtoMember(3)]
        public Dictionary<string, string> CldrMetazones { get; } = new Dictionary<string, string>();

        [ProtoMember(4)]
        public Dictionary<string, string> CldrPrimaryZones { get; } = new Dictionary<string, string>();

        [ProtoMember(6)]
        public Dictionary<string, string> CldrAliases { get; } = new Dictionary<string, string>();

        [ProtoMember(7)]
        public Dictionary<string, CldrLanguageData> CldrLanguageData { get; } = new Dictionary<string, CldrLanguageData>();

        [ProtoMember(8)]
        public Dictionary<string, Dictionary<string, string>> CldrWindowsMappings { get; } = new Dictionary<string, Dictionary<string, string>>();

        [ProtoMember(9)]
        public List<TimeZoneSelectionData> SelectionZones { get; } = new List<TimeZoneSelectionData>();

        public static TimeZoneData Load()
        {
            var assembly = typeof(TimeZoneData).Assembly;
            using (var stream = assembly.GetManifestResourceStream("TimeZoneNames.tz.dat"))
            {
                return Serializer.Deserialize<TimeZoneData>(stream);
            }
        }
    }

    [ProtoContract]
    public class CldrLanguageData
    {
        [ProtoMember(1)]
        public TimeZoneValues Formats { get; set; }

        [ProtoMember(2)]
        public string FallbackFormat { get; set; }

        [ProtoMember(3)]
        public Dictionary<string, TimeZoneValues> ShortNames { get; } = new Dictionary<string, TimeZoneValues>();

        [ProtoMember(4)]
        public Dictionary<string, TimeZoneValues> LongNames { get; } = new Dictionary<string, TimeZoneValues>();

        [ProtoMember(5)]
        public Dictionary<string, string> CountryNames { get; } = new Dictionary<string, string>();

        [ProtoMember(6)]
        public Dictionary<string, string> CityNames { get; } = new Dictionary<string, string>();
    }

    [ProtoContract]
    public class TimeZoneValues
    {
        [ProtoMember(1)]
        public string Generic { get; set; }

        [ProtoMember(2)]
        public string Standard { get; set; }

        [ProtoMember(3)]
        public string Daylight { get; set; }
    }

    [ProtoContract]
    public class TimeZoneSelectionData
    {
        [ProtoMember(1)]
        public string Id { get; set; }

        [ProtoMember(2)]
        public DateTime ThresholdUtc { get; set; }
    }
}
