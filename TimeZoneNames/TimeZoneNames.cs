using System;
using System.Collections.Generic;
using System.Linq;

namespace TimeZoneNames
{
    public class TimeZoneNames
    {
        private static readonly TimeZoneData Data = TimeZoneData.Load();

        public static string[] GetTimeZoneIdsForCountry(string countryCode)
        {
            return Data.TzdbZoneCountries
                .Where(x => x.Value.Equals(countryCode, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Key)
                .ToArray();
        }

        public static IDictionary<string, TimeZoneValues> GetTimeZonesForCountry(string countryCode, string languageCode)
        {
            var langKey = GetLanguageKey(languageCode);
            if (langKey == null)
                throw new ArgumentException("Invalid Language Code", "languageCode");

            var zones = GetTimeZoneIdsForCountry(countryCode);
            return zones.Select(x => new { Id = x, Names = GetNames(x, langKey, false) })
                .ToDictionary(x => x.Id, x => x.Names);
        }

        public static TimeZoneValues GetNamesForTimeZone(string timeZoneId, string languageCode)
        {
            var langKey = GetLanguageKey(languageCode);
            if (langKey == null)
                throw new ArgumentException("Invalid Language Code", "languageCode");

            return GetNames(timeZoneId, langKey, false);
        }

        public static TimeZoneValues GetAbbreviationsForTimeZone(string timeZoneId, string languageCode)
        {
            var langKey = GetLanguageKey(languageCode);
            if (langKey == null)
                throw new ArgumentException("Invalid Language Code", "languageCode");

            return GetNames(timeZoneId, langKey, true);
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

        private static TimeZoneValues GetNames(string timeZoneId, string languageKey, bool abbreviations)
        {
            if (!timeZoneId.Contains("/"))
                timeZoneId = ConvertWindowsToIana(timeZoneId);

            timeZoneId = GetCldrCanonicalId(timeZoneId);
            if (timeZoneId == null)
                throw new ArgumentException("Invalid Time Zone", "timeZoneId");

            var metaZone = GetMetazone(timeZoneId);
            var values = new TimeZoneValues();

            // First try for direct values
            bool found = false;
            SearchLanguages(languageKey, values, langKey =>
            {
                var b = PopulateDirectValues(langKey, values, timeZoneId, metaZone, abbreviations);
                if (b) found = true;
            });

            string country;
            if (!Data.CldrZoneCountries.TryGetValue(timeZoneId, out country))
            {
                // search tzdb zones
                if (!Data.TzdbZoneCountries.TryGetValue(timeZoneId, out country))
                {
                    foreach (var alias in Data.CldrAliases.Where(x => x.Value == timeZoneId))
                    {
                        foreach (var item in Data.TzdbZoneCountries.Where(item => string.Equals(item.Key, alias.Key, StringComparison.OrdinalIgnoreCase)))
                        {
                            country = item.Value;
                            break;
                        }
                    }
                }
            }


            if (abbreviations && country != null)
            {
                // try using the specific locale for the zone
                var lang = languageKey.Split('_', '-')[0] + "_" + country.ToLower();
                var b = PopulateDirectValues(lang, values, timeZoneId, metaZone, true);
                if (b) found = true;

                // try english as a last resort
                if (values.Generic == null || values.Standard == null || values.Daylight == null)
                {
                    b = PopulateDirectValues("en_" + country.ToLower(), values, timeZoneId, metaZone, true);
                    if (b) found = true;
                }
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


            SearchLanguages(languageKey, values, key =>
            {
                var langData = Data.CldrLanguageData[key];
                if (langData == null) return;

                var genericFormat = langData.Formats.Generic;
                if (genericFormat != null)
                    values.Generic = values.Generic ?? genericFormat.Replace("{0}", regionName);

                var standardFormat = langData.Formats.Standard;
                if (standardFormat != null)
                    values.Standard = values.Standard ?? standardFormat.Replace("{0}", regionName);

                var daylightFormat = langData.Formats.Daylight;
                if (daylightFormat != null)
                    values.Daylight = values.Daylight ?? daylightFormat.Replace("{0}", regionName);
            });


            return values;
        }

        private static string GetMetazone(string timeZoneId)
        {
            string metaZone;
            return Data.CldrMetazones.TryGetValue(timeZoneId, out metaZone) ? metaZone : null;
        }

        private static string ConvertWindowsToIana(string timeZoneId)
        {
            if (timeZoneId.Equals("UTC", StringComparison.OrdinalIgnoreCase))
                return "Etc/UTC";

            string ianaId;
            return Data.CldrWindowsMappings.TryGetValue(timeZoneId, out ianaId) ? ianaId : timeZoneId;
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
