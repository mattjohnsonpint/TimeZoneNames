using System.Collections.Generic;
using ProtoBuf;

namespace TimeZoneNames
{
    [ProtoContract]
    internal class TimeZoneData
    {
        private Dictionary<string, string> _tzdbZoneCountries = new Dictionary<string, string>();
        private Dictionary<string, string> _cldrZoneCountries = new Dictionary<string, string>();
        private Dictionary<string, string> _cldrMetazones = new Dictionary<string, string>();
        private Dictionary<string, string> _cldrAliases = new Dictionary<string, string>();
        private Dictionary<string, CldrLanguageData> _cldrLanguageData = new Dictionary<string, CldrLanguageData>();

        [ProtoMember(1)]
        public Dictionary<string, string> TzdbZoneCountries
        {
            get { return _tzdbZoneCountries; }
            protected set { _tzdbZoneCountries = value; }
        }

        [ProtoMember(2)]
        public Dictionary<string, string> CldrZoneCountries
        {
            get { return _cldrZoneCountries; }
            protected set { _cldrZoneCountries = value; }
        }

        [ProtoMember(3)]
        public Dictionary<string, string> CldrMetazones
        {
            get { return _cldrMetazones; }
            protected set { _cldrMetazones = value; }
        }

        [ProtoMember(4)]
        public Dictionary<string, string> CldrAliases
        {
            get { return _cldrAliases; }
            protected set { _cldrAliases = value; }
        }


        [ProtoMember(5)]
        public Dictionary<string, CldrLanguageData> CldrLanguageData
        {
            get { return _cldrLanguageData; }
            protected set { _cldrLanguageData = value; }
        }

        public static TimeZoneData Load()
        {
            var assembly = typeof (TimeZoneData).Assembly;
            using (var stream = assembly.GetManifestResourceStream("TimeZoneNames.tz.dat"))
            {
                return Serializer.Deserialize<TimeZoneData>(stream);
            }
        }
    }

    [ProtoContract]
    internal class CldrLanguageData
    {
        private Dictionary<string, TimeZoneValues> _shortNames = new Dictionary<string, TimeZoneValues>();
        private Dictionary<string, TimeZoneValues> _longNames = new Dictionary<string, TimeZoneValues>();
        private Dictionary<string, string> _countryNames = new Dictionary<string, string>();

        [ProtoMember(1)]
        public TimeZoneValues Formats { get; set; }

        [ProtoMember(2)]
        public Dictionary<string, TimeZoneValues> ShortNames
        {
            get { return _shortNames; }
            protected set { _shortNames = value; }
        }

        [ProtoMember(3)]
        public Dictionary<string, TimeZoneValues> LongNames
        {
            get { return _longNames; }
            protected set { _longNames = value; }
        }

        [ProtoMember(4)]
        public Dictionary<string, string> CountryNames
        {
            get { return _countryNames; }
            protected set { _countryNames = value; }
        }
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
}
