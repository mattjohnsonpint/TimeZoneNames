using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TimeZoneNames
{
    public static class TZNames
    {
        private static readonly TimeZoneData Data = TimeZoneData.Load();

        private static readonly ConcurrentDictionary<string, IComparer<string>> Comparers =
            new ConcurrentDictionary<string, IComparer<string>>(StringComparer.OrdinalIgnoreCase);

        public static string[] GetTimeZoneIdsForCountry(string countryCode)
        {
            return GetTimeZoneIdsForCountry(countryCode, DateTimeOffset.MinValue);
        }

        public static string[] GetTimeZoneIdsForCountry(string countryCode, DateTimeOffset threshold)
        {
            var zones = Data.TzdbZoneCountries
                .Where(x => x.Value.Contains(countryCode, StringComparer.OrdinalIgnoreCase))
                .Select(x => x.Key)
                .ToArray();

            if (threshold == DateTimeOffset.MinValue)
                return zones;

            var withinThreshold = Data.SelectionZones.Where(x => x.ThresholdUtc >= threshold.UtcDateTime).Select(x => x.Id);
            var selectionZones = zones.Intersect(withinThreshold).ToArray();
            return selectionZones.Length > 0 ? selectionZones : zones;
        }

        public static IDictionary<string, string> GetTimeZonesForCountry(string countryCode, string languageCode)
        {
            return GetTimeZonesForCountry(countryCode, languageCode, DateTimeOffset.MinValue);
        }

        public static IDictionary<string, string> GetTimeZonesForCountry(string countryCode, string languageCode, DateTimeOffset threshold)
        {
            var langKey = GetLanguageKey(languageCode);
            if (langKey == null)
                throw new ArgumentException("Invalid Language Code", nameof(languageCode));

            var zones = GetTimeZoneIdsForCountry(countryCode, threshold);
            var results = zones.Select(
                x => new
                {
                    Id = x,
                    Name = GetNames(x, langKey, false).Generic
                })
                .ToDictionary(x => x.Id, x => x.Name);

            // Append city names only when needed to differentiate zones with the same name
            foreach (var group in results.GroupBy(x => x.Value).Where(x => x.Count() > 1).ToArray())
            {
                foreach (var item in group)
                {
                    results[item.Key] = item.Value.AppendCity(GetCityName(item.Key, langKey));
                }
            }

            return results;
        }

        public static IList<string> GetFixedTimeZoneIds()
        {
            var zones = new List<string>();
            for (int i = -12; i <= 14; i++)
            {
                if (i == 0)
                    zones.Add("Etc/UTC");
                else
                    zones.Add("Etc/GMT" + (i < 0 ? "+" : "-") + Math.Abs(i));
            }

            return zones.ToArray();
        }

        public static IDictionary<string, string> GetFixedTimeZoneNames(string languageCode)
        {
            return GetFixedTimeZoneNames(languageCode, false);
        }

        public static IDictionary<string, string> GetFixedTimeZoneAbbreviations(string languageCode)
        {
            return GetFixedTimeZoneNames(languageCode, true);
        }

        private static IDictionary<string, string> GetFixedTimeZoneNames(string languageCode, bool abbreviations)
        {
            var langKey = GetLanguageKey(languageCode);
            if (langKey == null)
                throw new ArgumentException("Invalid Language Code", nameof(languageCode));

            var zones = GetFixedTimeZoneIds();
            var results = zones.Select(
                x => new
                {
                    Id = x,
                    Name = GetNames(x, langKey, abbreviations).Generic
                })
                .ToDictionary(x => x.Id, x => x.Name);

            return results;
        }

        private static string AppendCity(this string name, string city)
        {
            return string.IsNullOrWhiteSpace(city) ? name : $"{name} ({city})";
        }

        public static TimeZoneValues GetNamesForTimeZone(string timeZoneId, string languageCode)
        {
            var langKey = GetLanguageKey(languageCode);
            if (langKey == null)
                throw new ArgumentException("Invalid Language Code", nameof(languageCode));

            return GetNames(timeZoneId, langKey, false);
        }

        public static TimeZoneValues GetAbbreviationsForTimeZone(string timeZoneId, string languageCode)
        {
            var langKey = GetLanguageKey(languageCode);
            if (langKey == null)
                throw new ArgumentException("Invalid Language Code", nameof(languageCode));

            return GetNames(timeZoneId, langKey, true);
        }

        public static IDictionary<string, string> GetCountryNames(string languageCode)
        {
            var langKey = GetLanguageKey(languageCode);
            if (langKey == null)
                throw new ArgumentException("Invalid Language Code", nameof(languageCode));

            var comparer = GetComparer(langKey);

            var results = new Dictionary<string, string>();
            while (langKey != null)
            {
                var countryNames = Data.CldrLanguageData[langKey].CountryNames;
                foreach (var name in countryNames.Where(x => !results.ContainsKey(x.Key)))
                    results.Add(name.Key, name.Value);

                langKey = GetLanguageSubkey(langKey);
            }

            return results.OrderBy(x => x.Value, comparer)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public static ICollection<string> GetLanguageCodes()
        {
            return Data.CldrLanguageData.Keys.OrderBy(x => x).ToArray();
        }

        private static IComparer<string> GetComparer(string langKey)
        {
            return Comparers.GetOrAdd(langKey, key =>
            {
                var culture = new CultureInfo(langKey.Replace('_', '-'));
                return new CultureAwareStringComparer(culture, CompareOptions.IgnoreCase);
            });
        }

        private static string GetLanguageKey(string languageCode)
        {
            var key = languageCode.ToLowerInvariant().Replace('-', '_');
            while (true)
            {
                if (Data.CldrLanguageData.ContainsKey(key))
                    return key;

                key = GetLanguageSubkey(key);
            }
        }

        private static string GetLanguageSubkey(string languageKey)
        {
            var keyParts = languageKey.Split('_');
            if (keyParts.Length == 1)
                return null;

            return string.Join("_", keyParts, 0, keyParts.Length - 1);
        }

        private static void SearchLanguages(string languageKey, TimeZoneValues values, Action<string> action)
        {
            while (languageKey != null && (values.Generic == null || values.Standard == null || values.Daylight == null))
            {
                action(languageKey);
                languageKey = GetLanguageSubkey(languageKey);
            }
        }

        private static string GetCldrCanonicalId(string timeZoneId)
        {
            string id;
            return Data.CldrAliases.TryGetValue(timeZoneId.ToLowerInvariant(), out id) ? id : timeZoneId;
        }

        private static string GetCityName(string timeZoneId, string languageKey)
        {
            while (languageKey != null)
            {
                var data = Data.CldrLanguageData[languageKey];
                string cityName;
                if (data.CityNames.TryGetValue(timeZoneId, out cityName))
                    return cityName;
                languageKey = GetLanguageSubkey(languageKey);
            }

            return timeZoneId.Split('/').Last().Replace("_", " ");
        }

        private static TimeZoneValues GetNames(string timeZoneId, string languageKey, bool abbreviations)
        {
            if (!timeZoneId.Contains("/"))
                timeZoneId = ConvertWindowsToIana(timeZoneId);

            timeZoneId = GetCldrCanonicalId(timeZoneId);
            if (timeZoneId == null)
                throw new ArgumentException("Invalid Time Zone", nameof(timeZoneId));

            if (abbreviations && timeZoneId == "Etc/GMT")
            {
                return new TimeZoneValues { Generic = "UTC", Standard = "UTC", Daylight = "UTC" };
            }

            var metaZone = GetMetazone(timeZoneId);
            var values = new TimeZoneValues();

            // First try for direct values
            bool found = false;
            SearchLanguages(languageKey, values, langKey =>
            {
                var b = PopulateDirectValues(langKey, values, timeZoneId, metaZone, abbreviations);
                if (b) found = true;
            });

            var countries = new List<string>();
            string[] c;
            if (Data.CldrZoneCountries.TryGetValue(timeZoneId, out c))
                countries.AddRange(c);

            if (Data.TzdbZoneCountries.TryGetValue(timeZoneId, out c))
                countries.AddRange(c);

            foreach (var alias in Data.CldrAliases.Where(x => x.Value == timeZoneId))
            {
                foreach (var item in Data.TzdbZoneCountries
                    .Where(item => string.Equals(item.Key, alias.Key, StringComparison.OrdinalIgnoreCase)))
                    countries.AddRange(item.Value);
            }

            var country = countries.FirstOrDefault(x => x != "001");

            if (abbreviations && country != null)
            {
                // try using the specific locale for the zone
                var lang = languageKey.Split('_', '-')[0] + "_" + country.ToLowerInvariant();
                var b = PopulateDirectValues(lang, values, timeZoneId, metaZone, true);
                if (b) found = true;

                // try english as a last resort
                if (values.Generic == null || values.Standard == null || values.Daylight == null)
                {
                    b = PopulateDirectValues("en_" + country.ToLowerInvariant(), values, timeZoneId, metaZone, true);
                    if (b) found = true;
                }
            }

            if (country == "RU")
            {
                // special case for Russia to force city names in all time zones
                found = false;
            }

            if (found)
            {
                // apply type fallback rules
                values.Generic = values.Generic ?? (values.Daylight == null ? values.Standard : null);
                values.Daylight = values.Daylight ?? values.Generic;

                // return whatever we have for abbreviations
                if (abbreviations)
                    return values;

                // return names if everything is complete
                if (values.Generic != null && values.Standard != null && values.Daylight != null)
                    return values;
            }

            string regionName = null;
            if (country != null)
            {
                SearchLanguages(languageKey, values, key =>
                {
                    if (regionName != null) return;

                    var langData = Data.CldrLanguageData[key];
                    if (langData != null && langData.CountryNames.ContainsKey(country))
                        regionName = langData.CountryNames[country];
                });
            }

            if (country == "RU")
            {
                // special case for Russia to force city names in all time zones
                regionName = null;
                values = new TimeZoneValues();
            }

            if (regionName == null)
            {
                SearchLanguages(languageKey, values, key =>
                {
                    if (regionName != null) return;

                    var langData = Data.CldrLanguageData[key];
                    if (langData != null && langData.CityNames.ContainsKey(timeZoneId))
                        regionName = langData.CityNames[timeZoneId];
                });
            }

            if (regionName == null)
            {
                regionName = timeZoneId.Split('/').Last().Replace("_", " ");
            }

            if (timeZoneId.StartsWith("Etc/GMT+") || timeZoneId.StartsWith("Etc/GMT-"))
            {
                values = GetNames("UTC", languageKey, abbreviations);

                var sign = timeZoneId[7] == '+' ? '-' : '+';
                var num = timeZoneId.Substring(8);
                var s = (abbreviations ? "" : " ") + sign + num;
                values.Generic += s;
                values.Standard += s;
                values.Daylight += s;

                return values;
            }


            SearchLanguages(languageKey, values, key =>
            {
                var langData = Data.CldrLanguageData[key];
                if (langData == null) return;

                var genericFormat = langData.Formats?.Generic;
                if (genericFormat != null)
                    values.Generic = values.Generic ?? genericFormat.Replace("{0}", regionName);

                var standardFormat = langData.Formats?.Standard;
                if (standardFormat != null)
                    values.Standard = values.Standard ?? standardFormat.Replace("{0}", regionName);

                var daylightFormat = langData.Formats?.Daylight;
                if (daylightFormat != null)
                    values.Daylight = values.Daylight ?? daylightFormat.Replace("{0}", regionName);
            });

            if (values.Generic == null && values.Standard == null && languageKey != "en")
            {
                // when all else fails, return the English values
                return GetNames(timeZoneId, "en", abbreviations);
            }

            return values;
        }

        private static string GetMetazone(string timeZoneId)
        {
            string metaZone;
            return Data.CldrMetazones.TryGetValue(timeZoneId, out metaZone) ? metaZone : null;
        }

        private static string ConvertWindowsToIana(string windowsId, string countryCode = null)
        {
            if (windowsId.Equals("UTC", StringComparison.OrdinalIgnoreCase))
                return "Etc/UTC";

            var territory = countryCode != null && Data.CldrWindowsMappings.ContainsKey(countryCode)
                ? countryCode
                : "001";

            string ianaId;
            return Data.CldrWindowsMappings[territory].TryGetValue(windowsId, out ianaId) ? ianaId : windowsId;
        }

        private static bool PopulateDirectValues(string langKey, TimeZoneValues values, string timeZoneId, string metaZone, bool abbreviations)
        {
            if (!Data.CldrLanguageData.ContainsKey(langKey))
                return false;

            var langData = Data.CldrLanguageData[langKey];
            var langNames = abbreviations ? langData.ShortNames : langData.LongNames;

            bool found = false;

            if (langNames.ContainsKey(timeZoneId))
            {
                found = true;
                var names = langNames[timeZoneId];
                values.Generic = values.Generic ?? names.Generic;
                values.Standard = values.Standard ?? names.Standard;
                values.Daylight = values.Daylight ?? names.Daylight;
            }

            if (metaZone != null && langNames.ContainsKey(metaZone))
            {
                found = true;
                var names = langNames[metaZone];
                values.Generic = values.Generic ?? names.Generic;
                values.Standard = values.Standard ?? names.Standard;
                values.Daylight = values.Daylight ?? names.Daylight;
            }

            return found;
        }
    }
}
