using System.Text;

namespace TimeZoneNames;

internal partial class TimeZoneData
{
    public static TimeZoneData Load()
    {
        var reader = new Reader(Blob());
        return new TimeZoneData
        {
            TzdbZoneCountries = reader.Dictionary(reader.Array),
            CldrZoneCountries = reader.Dictionary(reader.Array),
            CldrMetazones = reader.Dictionary(reader.String),
            CldrPrimaryZones = reader.Dictionary(reader.String),
            CldrAliases = reader.Dictionary(reader.String),
            CldrLanguageData = reader.Dictionary(() => new CldrLanguageData
            {
                Formats = reader.NullableValues(),
                FallbackFormat = reader.NullableString(),
                ShortNames = reader.Dictionary(reader.Values),
                LongNames = reader.Dictionary(reader.Values),
                CountryNames = reader.Dictionary(reader.String),
                CityNames = reader.Dictionary(reader.String)
            }),
            SelectionZones = reader.List(() => new TimeZoneSelectionData
            {
                Id = reader.String(),
                ThresholdUtc = new DateTime(reader.Long(), DateTimeKind.Utc)
            }),
            DisplayNames = reader.Dictionary<IDictionary<string, string>>(() => reader.Dictionary(reader.String, StringComparer.Ordinal))
        };
    }

    private sealed class Reader
    {
        private readonly byte[] _data;
        private readonly string?[] _strings;
        private int _position;

        public Reader(byte[] data)
        {
            _data = data;
            _strings = new string?[Int() + 1];
            for (var i = 1; i < _strings.Length; i++)
            {
                var length = Int();
                _strings[i] = Encoding.UTF8.GetString(_data, _position, length);
                _position += length;
            }
        }

        public long Long()
        {
            long value = 0;
            for (var shift = 0; ; shift += 7)
            {
                var b = _data[_position++];
                value |= (long)(b & 0x7F) << shift;
                if (b < 0x80)
                {
                    return value;
                }
            }
        }

        public int Int() => (int)Long();

        public string? NullableString() => _strings[Int()];

        public string String() => NullableString() ?? throw new InvalidDataException("Unexpected null string in time zone data.");

        public string[] Array()
        {
            var array = new string[Int()];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = String();
            }

            return array;
        }

        public TimeZoneValues Values() => new()
        {
            Generic = NullableString(),
            Standard = NullableString(),
            Daylight = NullableString()
        };

        public TimeZoneValues? NullableValues() => Int() == 0 ? null : Values();

        public List<T> List<T>(Func<T> item)
        {
            var count = Int();
            var list = new List<T>(count);
            for (var i = 0; i < count; i++)
            {
                list.Add(item());
            }

            return list;
        }

        public Dictionary<string, T> Dictionary<T>(Func<T> value) => Dictionary(value, StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, T> Dictionary<T>(Func<T> value, StringComparer comparer)
        {
            var count = Int();
            var dictionary = new Dictionary<string, T>(count, comparer);
            for (var i = 0; i < count; i++)
            {
                dictionary.Add(String(), value());
            }

            return dictionary;
        }
    }
}
