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

        private static string GetCldrCanonicalId(string timeZoneId)
        {
            string id;
            return Data.CldrAliases.TryGetValue(timeZoneId.ToLowerInvariant(), out id) ? id : null;
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

            var key = languageKey;
            while (key != null && (values.Generic == null || values.Standard == null || values.Daylight == null))
            {
                var langData = Data.CldrLanguageData[key];
                if (langData != null)
                {
                    var langNames = abbreviations ? langData.ShortNames : langData.LongNames;

                    if (langNames.ContainsKey(timeZoneId))
                    {
                        var names = langNames[timeZoneId];
                        values.Generic = values.Generic ?? names.Generic;
                        values.Standard = values.Standard ?? names.Standard;
                        values.Daylight = values.Daylight ?? names.Daylight;
                    }

                    if (metaZone != null && langNames.ContainsKey(metaZone))
                    {
                        var names = langNames[metaZone];
                        values.Generic = values.Generic ?? names.Generic;
                        values.Standard = values.Standard ?? names.Standard;
                        values.Daylight = values.Daylight ?? names.Daylight;
                    }
                }

                key = GetLanguageSubkey(key);
            }

            if (values.Generic != null && values.Standard != null && values.Daylight != null)
                return values;

            if (abbreviations)
            {
                if (values.Generic == null)
                {
                    values.Generic = Data.CldrLanguageData.Where(x=> x.Key.StartsWith("en"))
                        .Select(x => x.Value.ShortNames.ContainsKey(timeZoneId)
                            ? x.Value.ShortNames[timeZoneId].Generic
                            : metaZone != null && x.Value.ShortNames.ContainsKey(metaZone)
                                ? x.Value.ShortNames[metaZone].Generic
                                : null)
                        .FirstOrDefault(x => x != null);
                }

                if (values.Standard == null)
                {
                    values.Standard = Data.CldrLanguageData.Where(x => x.Key.StartsWith("en"))
                        .Select(x => x.Value.ShortNames.ContainsKey(timeZoneId)
                            ? x.Value.ShortNames[timeZoneId].Standard
                            : metaZone != null && x.Value.ShortNames.ContainsKey(metaZone)
                                ? x.Value.ShortNames[metaZone].Standard
                                : null)
                        .FirstOrDefault(x => x != null);
                }

                if (values.Daylight == null)
                {
                    values.Daylight = Data.CldrLanguageData.Where(x => x.Key.StartsWith("en"))
                        .Select(x => x.Value.ShortNames.ContainsKey(timeZoneId)
                            ? x.Value.ShortNames[timeZoneId].Daylight
                            : metaZone != null && x.Value.ShortNames.ContainsKey(metaZone)
                                ? x.Value.ShortNames[metaZone].Daylight
                                : null)
                        .FirstOrDefault(x => x != null);
                }

                if (values.Generic == null && values.Daylight == null)
                    values.Generic = values.Standard;
                
                return values;
            }

            string country;
            if (Data.CldrZoneCountries.TryGetValue(timeZoneId, out country))
            {

                string countryName = null;
                key = languageKey;
                while (key != null)
                {
                    var langData = Data.CldrLanguageData[key];
                    if (langData != null)
                    {
                        if (langData.CountryNames.ContainsKey(country))
                        {
                            countryName = langData.CountryNames[country];
                            break;
                        }
                    }

                    key = GetLanguageSubkey(key);
                }

                if (countryName == null)
                    return values;

                key = languageKey;
                while (key != null && (values.Generic == null || values.Standard == null || values.Daylight == null))
                {
                    var langData = Data.CldrLanguageData[key];
                    if (langData != null)
                    {
                        var genericFormat = langData.Formats.Generic;
                        if (genericFormat != null)
                            values.Generic = values.Generic ?? genericFormat.Replace("{0}", countryName);

                        var standardFormat = langData.Formats.Standard;
                        if (standardFormat != null)
                            values.Standard = values.Standard ?? standardFormat.Replace("{0}", countryName);

                        var daylightFormat = langData.Formats.Daylight;
                        if (daylightFormat != null)
                            values.Daylight = values.Daylight ?? daylightFormat.Replace("{0}", countryName);

                    }

                    key = GetLanguageSubkey(key);
                }
            }

            if (values.Generic == null && values.Daylight == null)
                values.Generic = values.Standard;

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
    }
}
