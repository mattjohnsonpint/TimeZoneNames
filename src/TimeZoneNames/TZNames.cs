using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using TimeZoneConverter;

namespace TimeZoneNames;

/// <summary>
/// Provides methods for getting localized names of time zones, and related functionality.
/// </summary>
public static class TZNames
{
    private static readonly TimeZoneData Data = TimeZoneData.Load();
    private static readonly ConcurrentDictionary<string, IComparer<string>> Comparers = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets an array of IANA time zone identifiers for a specific country.
    /// </summary>
    /// <param name="countryCode">The two character ISO 3166 country code.</param>
    /// <returns>An array of IANA time zone identifiers.</returns>
    public static string[] GetTimeZoneIdsForCountry(string countryCode)
    {
        return GetTimeZoneIdsForCountry(countryCode, DateTimeOffset.MinValue);
    }

    /// <summary>
    /// Gets an array of IANA time zone identifiers for a specific country.
    /// </summary>
    /// <param name="countryCode">The two character ISO 3166 country code.</param>
    /// <param name="threshold">A point in time to filter to.  The resulting list will only contain zones that differ after this point.</param>
    /// <returns>An array of IANA time zone identifiers.</returns>
    public static string[] GetTimeZoneIdsForCountry(string countryCode, DateTimeOffset threshold)
    {
        var zones = Data.TzdbZoneCountries
            .Where(x => x.Value.Contains(countryCode, StringComparer.OrdinalIgnoreCase))
            .Select(x => x.Key)
            .ToArray();

        if (threshold == DateTimeOffset.MinValue)
        {
            return zones;
        }

        var withinThreshold = Data.SelectionZones.Where(x => x.ThresholdUtc >= threshold.UtcDateTime).Select(x => x.Id);
        var selectionZones = zones.Intersect(withinThreshold).ToArray();
        return selectionZones.Length > 0 ? selectionZones : zones;
    }

    /// <summary>
    /// Gets an dictionary of IANA time zone identifiers and their corresponding localized display names, for a specific country.
    /// The results are suitable to populate a user-facing time zone selection control.
    /// </summary>
    /// <param name="countryCode">The two character ISO 3166 country code.</param>
    /// <param name="languageCode">The IETF language tag (culture code) to use when localizing the display names.</param>
    /// <returns>A dictionary of IANA time zone identifiers and localized display names.</returns>
    public static IDictionary<string, string> GetTimeZonesForCountry(string countryCode, string languageCode)
    {
        return GetTimeZonesForCountry(countryCode, languageCode, DateTimeOffset.MinValue);
    }

    /// <summary>
    /// Gets an dictionary of IANA time zone identifiers and their corresponding localized display names, for a specific country.
    /// The results are suitable to populate a user-facing time zone selection control.
    /// </summary>
    /// <param name="countryCode">The two character ISO 3166 country code.</param>
    /// <param name="languageCode">The IETF language tag (culture code) to use when localizing the display names.</param>
    /// <param name="threshold">A point in time to filter to.  The resulting list will only contain zones that differ after this point.</param>
    /// <returns>A dictionary of IANA time zone identifiers and localized display names.</returns>
    public static IDictionary<string, string> GetTimeZonesForCountry(string countryCode, string languageCode, DateTimeOffset threshold)
    {
        var langKey = GetLanguageKey(languageCode);
        if (langKey == null)
        {
            throw new ArgumentException("Invalid Language Code", nameof(languageCode));
        }

        var zones = GetTimeZoneIdsForCountry(countryCode, threshold);
        var results = zones.Select(
                x => new
                {
                    Id = x,
                    Name = GetNames(x, langKey, false).Generic
                })
            .ToDictionary(x => x.Id, x => x.Name, StringComparer.OrdinalIgnoreCase);

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

    /// <summary>
    /// Gets a list of IANA time zone identifiers that represent a fixed offset from UTC, including UTC itself.
    /// Note that time zones of the form Etc/GMT[+/-]n use an inverted sign from the usual conventions.
    /// </summary>
    /// <returns>A list of IANA time zone identifiers.</returns>
    public static IList<string> GetFixedTimeZoneIds()
    {
        var zones = new List<string>();
        for (var i = -12; i <= 14; i++)
        {
            if (i == 0)
            {
                zones.Add("Etc/UTC");
            }
            else
            {
                zones.Add("Etc/GMT" + (i < 0 ? "+" : "-") + Math.Abs(i));
            }
        }

        return zones.ToArray();
    }

    /// <summary>
    /// Gets a dictionary of IANA time zone identifiers that represent a fixed offset from UTC, including UTC itself,
    /// along with the corresponding localized display name.
    /// Note that time zones of the form Etc/GMT[+/-]n use an inverted sign from the usual conventions.
    /// </summary>
    /// <param name="languageCode">The IETF language tag (culture code) to use when localizing the display names.</param>
    /// <returns>A dictionary of IANA time zone identifiers and localized display names.</returns>
    public static IDictionary<string, string> GetFixedTimeZoneNames(string languageCode)
    {
        return GetFixedTimeZoneNames(languageCode, false);
    }

    /// <summary>
    /// This method is obsolete.
    /// Instead, use <see cref="GetFixedTimeZoneAbbreviations()"/> without passing a language code.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Do not pass a language code. Language does not apply for fixed time zone abbreviations.")]
    public static IDictionary<string, string> GetFixedTimeZoneAbbreviations(string languageCode)
    {
        return GetFixedTimeZoneAbbreviations();
    }

    /// <summary>
    /// Gets a dictionary of IANA time zone identifiers that represent a fixed offset from UTC, including UTC itself,
    /// along with the corresponding abbreviation.
    /// Note that time zones of the form Etc/GMT[+/-]n use an inverted sign from the usual conventions.
    /// </summary>
    /// <returns>A dictionary of IANA time zone identifiers and abbreviations.</returns>
    public static IDictionary<string, string> GetFixedTimeZoneAbbreviations()
    {
        return GetFixedTimeZoneNames("en", true);
    }

    private static IDictionary<string, string> GetFixedTimeZoneNames(string languageCode, bool abbreviations)
    {
        var langKey = GetLanguageKey(languageCode);
        if (langKey == null)
        {
            throw new ArgumentException("Invalid Language Code", nameof(languageCode));
        }

        var zones = GetFixedTimeZoneIds();

        var results = new OrderedDictionary<string, string>(zones.Count, StringComparer.OrdinalIgnoreCase);
        
        foreach (var zone in zones)
        {
            var name = GetNames(zone, langKey, abbreviations).Generic;
            results.Add(zone, name);
        }
        

        return results;
    }

    private static string AppendCity(this string name, string city)
    {
        return string.IsNullOrWhiteSpace(city) ? name : $"{name} ({city})";
    }

    /// <summary>
    /// Gets the localized names for a given IANA or Windows time zone identifier.
    /// </summary>
    /// <param name="timeZoneId">An IANA or Windows time zone identifier.</param>
    /// <param name="languageCode">The IETF language tag (culture code) to use when localizing the display names.</param>
    /// <returns>A <see cref="TimeZoneValues"/> object containing the localized generic, standard, and daylight names.</returns>
    public static TimeZoneValues GetNamesForTimeZone(string timeZoneId, string languageCode)
    {
        var langKey = GetLanguageKey(languageCode);
        if (langKey == null)
        {
            throw new ArgumentException("Invalid Language Code", nameof(languageCode));
        }

        return GetNames(timeZoneId, langKey, false);
    }

    /// <summary>
    /// Gets the abbreviations for a given IANA or Windows time zone identifier, localizing them when possible.
    /// </summary>
    /// <param name="timeZoneId">An IANA or Windows time zone identifier.</param>
    /// <param name="languageCode">The IETF language tag (culture code) to use when localizing the abbreviations.</param>
    /// <returns>A <see cref="TimeZoneValues"/> object containing the localized generic, standard, and daylight abbreviations.</returns>
    public static TimeZoneValues GetAbbreviationsForTimeZone(string timeZoneId, string languageCode)
    {
        var langKey = GetLanguageKey(languageCode);
        if (langKey == null)
        {
            throw new ArgumentException("Invalid Language Code", nameof(languageCode));
        }

        return GetNames(timeZoneId, langKey, true);
    }

    /// <summary>
    /// Gets a dictionary of ISO 3166 country codes and their corresponding localized names.
    /// </summary>
    /// <param name="languageCode">The IETF language tag (culture code) to use when localizing the country names.</param>
    /// <returns>A dictionary of country codes and names.</returns>
    public static IDictionary<string, string> GetCountryNames(string languageCode)
    {
        var langKey = GetLanguageKey(languageCode);
        if (langKey == null)
        {
            throw new ArgumentException("Invalid Language Code", nameof(languageCode));
        }

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
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the localized names for a given IANA or Windows time zone identifier.
    /// </summary>
    /// <param name="timeZoneId">An IANA or Windows time zone identifier.</param>
    /// <param name="languageCode">The IETF language tag (culture code) to use when localizing the display names.</param>
    /// <returns>A display name associated with this time zone.</returns>
    public static string GetDisplayNameForTimeZone(string timeZoneId, string languageCode)
    {
        var langKey = GetLanguageKey(languageCode, true);
        if (langKey == null)
        {
            throw new ArgumentException("Invalid Language Code", nameof(languageCode));
        }

        var displayNames = Data.DisplayNames[langKey];

        if (displayNames.TryGetValue(timeZoneId, out var displayName))
        {
            return displayName;
        }

        if (TZConvert.TryIanaToWindows(timeZoneId, out timeZoneId) && displayNames.TryGetValue(timeZoneId, out displayName))
        {
            return displayName;
        }

        return null;
    }

    /// <summary>
    /// Get display names suitable for use in a single drop-down list to select a time zone.
    /// </summary>
    /// <param name="languageCode">The IETF language tag (culture code) to use when localizing the display names.</param>
    /// <param name="useIanaZoneIds"><c>true</c> to use IANA time zone keys, otherwise uses Windows time zone keys.</param>
    /// <returns>A dictionary where the key is the time zone id, and the name is the localized display name.</returns>
    public static IDictionary<string, string> GetDisplayNames(string languageCode, bool useIanaZoneIds = false)
    {
        var langKey = GetLanguageKey(languageCode, true);
        if (langKey == null)
        {
            throw new ArgumentException("Invalid Language Code", nameof(languageCode));
        }

        var displayNames = Data.DisplayNames[langKey];

        if (!useIanaZoneIds)
        {
            return displayNames;
        }

        var languageCodeParts = languageCode.Split('_', '-');
        var territoryCode = languageCodeParts.Length < 2 ? "001" : languageCodeParts[1];
        return displayNames
            .Where(x => !TimeZoneData.ObsoleteWindowsZones.Contains(x.Key))
            .ToDictionary(
                x => TZConvert.WindowsToIana(x.Key, territoryCode),
                x => x.Value,
                StringComparer.OrdinalIgnoreCase);
    }



    /// <summary>
    /// Gets a list of all language codes supported by this library.
    /// </summary>
    /// <returns>A list of language codes.</returns>
    public static ICollection<string> GetLanguageCodes()
    {
        return Data.CldrLanguageData.Keys.OrderBy(x => x).ToArray();
    }

    private static IComparer<string> GetComparer(string langKey)
    {
        return Comparers.GetOrAdd(langKey, key =>
        {
            var culture = new CultureInfo(key.Replace('_', '-'));
            return StringComparer.Create(culture, true);
        });
    }

    private static string GetLanguageKey(string languageCode, bool forDisplayNames = false)
    {
        var key = languageCode.ToLowerInvariant().Replace('-', '_');
        while (true)
        {
            if (forDisplayNames)
            {
                if (Data.DisplayNames.ContainsKey(key))
                {
                    return key;
                }
            }
            else
            {
                if (Data.CldrLanguageData.ContainsKey(key))
                {
                    return key;
                }
            }

            key = GetLanguageSubkey(key);

            if (key == null)
            {
                var keys = forDisplayNames ? (IEnumerable<string>)Data.DisplayNames.Keys : Data.CldrLanguageData.Keys;
                key = keys.FirstOrDefault(x => x.Split('_')[0].Equals(languageCode, StringComparison.OrdinalIgnoreCase));

                if (key == null)
                {
                    throw new Exception("Could not find a language with code " + languageCode);
                }
            }
        }
    }

    private static string GetLanguageSubkey(string languageKey)
    {
        var keyParts = languageKey.Split('_');
        if (keyParts.Length == 1)
        {
            return null;
        }

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
        return Data.CldrAliases.TryGetValue(timeZoneId.ToLowerInvariant(), out var id) ? id : timeZoneId;
    }

    private static string GetCityName(string timeZoneId, string languageKey)
    {
        while (languageKey != null)
        {
            var data = Data.CldrLanguageData[languageKey];
            if (data.CityNames.TryGetValue(timeZoneId, out var cityName))
            {
                return cityName;
            }

            languageKey = GetLanguageSubkey(languageKey);
        }

        return timeZoneId.Split('/').Last().Replace("_", " ");
    }

    private static TimeZoneValues GetNames(string timeZoneId, string languageKey, bool abbreviations)
    {
        if (TZConvert.KnownWindowsTimeZoneIds.Contains(timeZoneId, StringComparer.OrdinalIgnoreCase))
        {
            timeZoneId = TZConvert.WindowsToIana(timeZoneId);
        }

        timeZoneId = GetCldrCanonicalId(timeZoneId);
        if (timeZoneId == null)
        {
            throw new ArgumentException("Invalid Time Zone", nameof(timeZoneId));
        }
        
        if (abbreviations && (timeZoneId == "Etc/GMT" || timeZoneId == "Etc/UTC"))
        {
            return new TimeZoneValues { Generic = "UTC", Standard = "UTC", Daylight = "UTC" };
        }

        var metaZone = GetMetazone(timeZoneId);
        var values = new TimeZoneValues();

        // First try for direct values
        var found = false;
        SearchLanguages(languageKey, values, langKey =>
        {
            var b = PopulateDirectValues(langKey, values, timeZoneId, metaZone, abbreviations);
            if (b)
            {
                found = true;
            }
        });

        var countries = new List<string>();
        if (Data.CldrZoneCountries.TryGetValue(timeZoneId, out var c))
        {
            countries.AddRange(c);
        }

        if (Data.TzdbZoneCountries.TryGetValue(timeZoneId, out c))
        {
            countries.AddRange(c);
        }

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
            if (b)
            {
                found = true;
            }

            // try english as a last resort
            if (values.Generic == null || values.Standard == null || values.Daylight == null)
            {
                b = PopulateDirectValues("en_" + country.ToLowerInvariant(), values, timeZoneId, metaZone, true);
                if (b)
                {
                    found = true;
                }
                else
                {
                    // really, try any variant of english
                    foreach (var english in Data.CldrLanguageData.Keys.Where(x => x.StartsWith("en_")))
                    {
                        b = PopulateDirectValues(english, values, timeZoneId, metaZone, true);
                        if (b)
                        {
                            found = true;
                            break;
                        }
                    }
                }
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
            values.Generic ??= (values.Daylight == null ? values.Standard : null);
            values.Daylight ??= values.Generic;

            // return whatever we have for abbreviations
            if (abbreviations)
            {
                return values;
            }

            // return names if everything is complete
            if (values.Generic != null && values.Standard != null && values.Daylight != null)
            {
                return values;
            }
        }

        string regionName = null;
        if (country != null)
        {
            SearchLanguages(languageKey, values, key =>
            {
                if (regionName != null)
                {
                    return;
                }
                
                if (Data.CldrLanguageData.TryGetValue(key, out var langData) && langData.CountryNames.ContainsKey(country))
                {
                    regionName = langData.CountryNames[country];
                }
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
                if (regionName != null)
                {
                    return;
                }

                if (Data.CldrLanguageData.TryGetValue(key, out var langData) &&
                    langData.CityNames.ContainsKey(timeZoneId))
                {
                    regionName = langData.CityNames[timeZoneId];
                }
            });
        }

        regionName ??= timeZoneId.Split('/').Last().Replace("_", " ");

        if (timeZoneId.StartsWith("Etc/GMT+") || timeZoneId.StartsWith("Etc/GMT-"))
        {
            values = GetNames(abbreviations ? "Etc/GMT" : "Etc/UTC", languageKey, abbreviations);

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
            if (!Data.CldrLanguageData.TryGetValue(key, out var langData))
            {
                return;
            }

            var genericFormat = langData.Formats?.Generic;
            if (genericFormat != null)
            {
                values.Generic ??= genericFormat.Replace("{0}", regionName);
            }

            var standardFormat = langData.Formats?.Standard;
            if (standardFormat != null)
            {
                values.Standard ??= standardFormat.Replace("{0}", regionName);
            }

            var daylightFormat = langData.Formats?.Daylight;
            if (daylightFormat != null)
            {
                values.Daylight ??= daylightFormat.Replace("{0}", regionName);
            }
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
        return Data.CldrMetazones.TryGetValue(timeZoneId, out var metaZone) ? metaZone : null;
    }

    private static bool PopulateDirectValues(string langKey, TimeZoneValues values, string timeZoneId, string metaZone, bool abbreviations)
    {
        if (!Data.CldrLanguageData.ContainsKey(langKey))
        {
            return false;
        }

        var langData = Data.CldrLanguageData[langKey];
        var langNames = abbreviations ? langData.ShortNames : langData.LongNames;

        var found = false;

        if (langNames.ContainsKey(timeZoneId))
        {
            found = true;
            var names = langNames[timeZoneId];
            values.Generic ??= names.Generic;
            values.Standard ??= names.Standard;
            values.Daylight ??= names.Daylight;
        }

        if (metaZone != null && langNames.ContainsKey(metaZone))
        {
            found = true;
            var names = langNames[metaZone];
            values.Generic ??= names.Generic;
            values.Standard ??= names.Standard;
            values.Daylight ??= names.Daylight;
        }

        return found;
    }
}