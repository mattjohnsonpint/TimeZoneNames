﻿using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using System.Xml.XPath;
using NodaTime;
using NodaTime.TimeZones;

namespace TimeZoneNames.DataBuilder;

public class DataExtractor
{
    private readonly string _cldrPath;
    private readonly string _nzdPath;

    private readonly TimeZoneData _data = new();

    private DataExtractor(string dataPath)
    {
        _cldrPath = Path.Combine(dataPath, "cldr");
        _nzdPath = Path.Combine(dataPath, "nzd");
    }

    public static DataExtractor Load(string dataPath, bool overwrite)
    {
        var data = new DataExtractor(dataPath);
        data.DownloadData(overwrite);

        data.LoadData();
        return data;
    }

    public void SaveData(string outputFilePath)
    {
        using var stream = File.Create(outputFilePath);
        using var compressedStream = new GZipStream(stream, CompressionLevel.Optimal);
        JsonSerializer.Serialize(compressedStream, _data);
    }

    private TzdbDateTimeZoneSource _tzdbSource = null!;
    private IDateTimeZoneProvider _tzdbProvider = null!;

    private void LoadData()
    {
        // init noda time
        using (var stream = File.OpenRead(Directory.GetFiles(_nzdPath)[0]))
        {
            _tzdbSource = TzdbDateTimeZoneSource.FromStream(stream);
            _tzdbProvider = new DateTimeZoneCache(_tzdbSource);
        }

        // this has to be loaded first
        LoadMetaZones();

        // these can be loaded in parallel
        var actions = new Action[]
        {
            LoadZoneCountries,
            LoadZoneAliases,
            LoadLanguages,
            LoadDisplayNames
        };
        Task.WhenAll(actions.Select(Task.Run)).Wait();

        // patch the data for any known issues
        PatchData();

        // this has to come last, as it relies on patched data
        LoadSelectionZones();
    }

    private void DownloadData(bool overwrite)
    {
        Task.WaitAll(
            DownloadCldrAsync(overwrite),
            DownloadNzdAsync(overwrite));
    }

    private async Task DownloadCldrAsync(bool overwrite)
    {
        var exists = Directory.Exists(_cldrPath);
        if (overwrite || !exists)
        {
            if (exists)
            {
                Directory.Delete(_cldrPath, true);
            }

            await Downloader.DownloadCldrAsync(_cldrPath);
        }
    }

    private async Task DownloadNzdAsync(bool overwrite)
    {
        var exists = Directory.Exists(_nzdPath);
        if (overwrite || !exists)
        {
            if (exists)
            {
                Directory.Delete(_nzdPath, true);
            }

            await Downloader.DownloadNzdAsync(_nzdPath);
        }
    }

    private void LoadZoneCountries()
    {
        foreach (var location in _tzdbSource.ZoneLocations!.OrderBy(x => GetStandardOffset(x.ZoneId)).ThenBy(x => GetDaylightOffset(x.ZoneId)))
        {
            AddToLookup(_data.TzdbZoneCountries, location.ZoneId, location.CountryCode);
        }
    }

    private static readonly Instant Now = SystemClock.Instance.GetCurrentInstant();
    private static readonly Instant Jan = Instant.FromUtc(Now.InUtc().Year, 1, 1, 0, 0);
    private static readonly Instant Jun = Instant.FromUtc(Now.InUtc().Year, 6, 1, 0, 0);
    private static readonly Instant Future10 = Instant.FromUtc(Now.InUtc().Year + 10, 1, 1, 0, 0);
    private static readonly Instant Future11 = Instant.FromUtc(Now.InUtc().Year + 11, 1, 1, 0, 0);


    private Offset GetStandardOffset(string zoneId)
    {
        return _tzdbProvider[zoneId].GetZoneInterval(Now).StandardOffset;
    }

    private Offset GetDaylightOffset(string zoneId)
    {
        var zone = _tzdbProvider[zoneId];
        return Offset.Max(zone.GetZoneInterval(Jan).WallOffset, zone.GetZoneInterval(Jun).WallOffset);
    }

    private void LoadSelectionZones()
    {
        var results = new List<TimeZoneSelectionData>();

        var precedence = File.ReadAllLines(Path.Combine("data", "zone-precedence.txt"));

        var splitPoints = GetAllZoneSplitPoints();
        IList<string>? last = null;
        var useConsole = TryHideConsoleCursor();
        for (var i = splitPoints.Count - 1; i >= 0; i--)
        {
            if (useConsole)
            {
                var pct = 100 * (1.0 * (splitPoints.Count - i)) / splitPoints.Count;
                Console.Write("{0:F1}%", pct);
                Console.CursorLeft = 0;
            }

            var point = splitPoints[i];
            var zones = GetSelectionZones(point, precedence);

            if (last == null)
            {
                last = zones;
                continue;
            }

            var items = zones.Except(last)
                .Select(x => new TimeZoneSelectionData { Id = x, ThresholdUtc = point.ToDateTimeUtc() });
            results.AddRange(items);

            last = zones;
        }

        var remaining = last?.Except(results.Select(x => x.Id));
        if (remaining != null)
        {
            var items = remaining
                .Select(x => new TimeZoneSelectionData { Id = x, ThresholdUtc = DateTime.MaxValue });
            results.AddRange(items);
        }

        _data.SelectionZones.AddRange(results
            .OrderBy(x => GetStandardOffset(x.Id))
            .ThenBy(x => GetDaylightOffset(x.Id))
            .ThenByDescending(x => x.ThresholdUtc)
            .ThenBy(x => x.Id));

        if (useConsole)
        {
            Console.WriteLine();
            Console.CursorVisible = true;
        }
    }

    private static bool TryHideConsoleCursor()
    {
        try
        {
            Console.CursorVisible = false;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private IList<string> GetSelectionZones(Instant fromInstant, string[] precedence)
    {
        var results = _tzdbProvider.Ids
            .Select(x => _tzdbSource.CanonicalIdMap[x])
            .Distinct()
            .Select(x => new
            {
                Id = x,
                Intervals = GetBoundIntervals(_tzdbProvider[x], fromInstant, Future11),
                Location = _tzdbSource.ZoneLocations!.FirstOrDefault(l => l.ZoneId == x)
            })
            .Where(x => x.Location != null)
            .GroupBy(x => new { x.Location?.CountryCode, Hash = GetIntervalsHash(x.Intervals) })
            .Select(g =>
            {
                var ids = g.Select(z => z.Id).ToArray();
                if (ids.Length == 1)
                {
                    return ids[0];
                }

                // use the zone-precedence.txt file when we need a tiebreaker
                var s = precedence.Intersect(ids).FirstOrDefault();
                if (s != null)
                {
                    return s;
                } else {
                    throw new InvalidOperationException("No tiebreaker found for " + string.Join(", ", ids));
                }
            })
            .ToList();

        // Unfortunately, some zones are too politically charged to appear in most selection lists.
        results.Remove("Asia/Urumqi");
        results.Remove("Europe/Simferopol");

        return results;
    }

    private static int GetIntervalsHash(IEnumerable<ZoneInterval> intervals)
    {
        var hash = 17;
        unchecked
        {
            foreach (var interval in intervals)
            {
                hash = hash * 23 + interval.Start.GetHashCode();
                hash = hash * 23 + interval.End.GetHashCode();
                hash = hash * 23 + interval.WallOffset.GetHashCode();
            }
        }
        return hash;
    }

    private static IEnumerable<ZoneInterval> GetBoundIntervals(DateTimeZone zone, Instant start, Instant end)
    {
        var intervals = zone.GetZoneIntervals(start, end).ToList();

        var first = intervals.First();
        if (!first.HasStart || first.Start < start)
        {
            intervals[0] = new ZoneInterval(first.Name, start, first.HasEnd ? first.End : null, first.WallOffset, first.Savings);
        }

        var last = intervals.Last();
        if (!last.HasEnd || last.End > end)
        {
            intervals[^1] = new ZoneInterval(last.Name, last.HasStart ? last.Start : null, end, last.WallOffset, last.Savings);
        }

        return intervals;
    }

    private void LoadZoneAliases()
    {
        using var stream = File.OpenRead(Path.Combine(_cldrPath, "common", "bcp47", "timezone.xml"));
        var doc = XDocument.Load(stream);
        var elements = doc.XPathSelectElements("/ldmlBCP47/keyword/key[@name='tz']/type");
        foreach (var element in elements)
        {
            var aliasAttribute = element.Attribute("alias");
            if (aliasAttribute == null)
            {
                continue;
            }

            var aliases = aliasAttribute.Value.Split(' ');
            if (aliases.Length == 1)
            {
                continue;
            }

            foreach (var alias in aliases.Skip(1))
                _data.CldrAliases.Add(alias.ToLowerInvariant(), aliases[0]);
        }
    }

    private void LoadMetaZones()
    {
        LoadMetaZonesFromFile(Path.Combine(_cldrPath, "common", "supplemental", "metaZones.xml"));
        LoadMetaZonesFromFile(Path.Combine("data", "metaZones-override.xml"));
    }

    private void LoadMetaZonesFromFile(string path)
    {
        using var stream = File.OpenRead(path);
        var doc = XDocument.Load(stream);

        var timeZoneElements = doc.XPathSelectElements("/supplementalData/metaZones/metazoneInfo/timezone");
        foreach (var element in timeZoneElements)
        {
            var timeZone = element.Attribute("type")!.Value;
            var metaZone = element.Elements("usesMetazone").Last().Attribute("mzone")!.Value;
            _data.CldrMetazones[timeZone] = metaZone;
        }

        var mapZoneElements = doc.XPathSelectElements("/supplementalData/metaZones/mapTimezones/mapZone");
        foreach (var element in mapZoneElements)
        {
            var timeZone = element.Attribute("type")!.Value;
            var territory = element.Attribute("territory")!.Value;
            AddToLookup(_data.CldrZoneCountries, timeZone, territory);
        }

        var primaryZoneElements = doc.XPathSelectElements("/supplementalData/metaZones/primaryZones/primaryZone");
        foreach (var element in primaryZoneElements)
        {
            var country = element.Attribute("iso3166")!.Value;
            var zone = element.Value;
            _data.CldrPrimaryZones[country] = zone;
        }
    }

    private void LoadLanguages()
    {
        var languages = Directory.GetFiles(Path.Combine(_cldrPath, "common", "main"))
            .Select(s => Path.GetFileName(s)[..^4]);

        Parallel.ForEach(languages, LoadLanguage);
    }

    private void LoadLanguage(string language)
    {
        using var stream = File.OpenRead(Path.Combine(_cldrPath, "common", "main", language + ".xml"));
        var doc = XDocument.Load(stream);

        var territoriesElement = doc.XPathSelectElement("/ldml/localeDisplayNames/territories");
        if (territoriesElement != null)
        {
            AddCountryEntries(territoriesElement, language);
        }

        var tzElement = doc.XPathSelectElement("/ldml/dates/timeZoneNames");
        if (tzElement != null)
        {
            AddFormatEntries(tzElement, language);
            AddZoneEntries(tzElement, language, "zone", "exemplarCity");
            AddZoneEntries(tzElement, language, "zone", "short");
            AddZoneEntries(tzElement, language, "zone", "long");
            AddZoneEntries(tzElement, language, "metazone", "short");
            AddZoneEntries(tzElement, language, "metazone", "long");
        }
    }

    private void AddCountryEntries(XContainer territoriesElement, string language)
    {
        var excluded = new[] { "AN", "BV", "CP", "EU", "HM", "QO", "ZZ" };

        var countries = territoriesElement.Elements("territory")
            .Where(x => x.Attribute("alt") == null)
            .GroupBy(x => x.Attribute("type")!.Value)
            .Where(x => x.Key.Length == 2 && !excluded.Contains(x.Key))
            .ToDictionary(x => x.Key, x => x.First().Value);

        var langData = GetLangData(language);
        foreach (var country in countries)
        {
            langData.CountryNames.Add(country.Key, country.Value);
        }
    }

    private void AddFormatEntries(XContainer tzElement, string language)
    {
        var formats = tzElement.Elements("regionFormat")
            .ToDictionary(
                x => x.Attribute("type") == null ? "generic" : x.Attribute("type")!.Value,
                x => x.Value);

        if (formats.Count == 0)
        {
            return;
        }

        var values = new TimeZoneValues();
        if (formats.TryGetValue("generic", out var genericName))
        {
            values.Generic = genericName;
        }

        if (formats.TryGetValue("standard", out var standardName))
        {
            values.Standard = standardName;
        }

        if (formats.TryGetValue("daylight", out var daylightName))
        {
            values.Daylight = daylightName;
        }

        var langData = GetLangData(language);
        langData.Formats = values;

        var fallbackFormat = tzElement.Element("fallbackFormat");
        if (fallbackFormat != null)
        {
            langData.FallbackFormat = fallbackFormat.Value;
        }
    }

    private void AddZoneEntries(XContainer tzElement, string language, string elementName, string entryName)
    {
        var langData = GetLangData(language);

        var zones = tzElement.Elements(elementName);
        foreach (var zone in zones)
        {
            var zoneName = zone.Attribute("type")!.Value;

            var element = zone.Element(entryName);
            if (element == null)
            {
                continue;
            }

            switch (entryName)
            {
                case "exemplarCity":
                {
                    langData.CityNames.Add(zoneName, element.Value);
                    break;
                }
                case "short":
                {
                    var values = GetTimeZoneValues(element);
                    langData.ShortNames.Add(zoneName, values);
                    break;
                }
                case "long":
                {
                    var values = GetTimeZoneValues(element);
                    langData.LongNames.Add(zoneName, values);
                    break;
                }
            }
        }
    }

    private readonly object _locker = new();

    private CldrLanguageData GetLangData(string language)
    {
        language = language.ToLowerInvariant();

        lock (_locker)
        {
            if (!_data.CldrLanguageData.TryGetValue(language, out var data))
            {
                data = new CldrLanguageData();
                _data.CldrLanguageData.Add(language, data);
            }
            return data;
        }
    }

    private static TimeZoneValues GetTimeZoneValues(XContainer element)
    {
        var values = new TimeZoneValues();

        var genericElement = element.Element("generic");
        if (genericElement != null && genericElement.Value != "∅∅∅")
        {
            values.Generic = genericElement.Value;
        }

        var standardElement = element.Element("standard");
        if (standardElement != null && standardElement.Value != "∅∅∅")
        {
            values.Standard = standardElement.Value;
        }

        var daylightElement = element.Element("daylight");
        if (daylightElement != null && daylightElement.Value != "∅∅∅")
        {
            values.Daylight = daylightElement.Value;
        }

        return values;
    }

    private void LoadDisplayNames()
    {
        using var stream = File.OpenRead(Path.Combine("data", "windows-displaynames.json"));
        var data = JsonNode.Parse(stream)!;
        var languages = data["Languages"];
        if (languages == null)
        {
            return;
        }

        foreach (var item in languages.AsArray())
        {
            var locale = item!["Locale"]!.GetValue<string>().Replace("-", "_");
            var list = item["TimeZones"]!.AsObject()
                .Select(o => new KeyValuePair<string, string>(o.Key, (string) o.Value!))
                .ToList();
            
            // Base offset change for Jordan, reflected in Windows, not yet in tzinfo.json
            var jordan = list.First(x => x.Key == "Jordan Standard Time");
            list.Remove(jordan);
            jordan = new KeyValuePair<string, string>(jordan.Key, jordan.Value.Replace("+02:00", "+03:00"));
            var i = list.FindIndex(x => x.Key == "Arabic Standard Time");
            list.Insert(i, jordan);
            
            var timeZones = list
                .ToOrderedDictionary(o => o.Key, o => o.Value);

            _data.DisplayNames.Add(locale, timeZones);
        }
    }

    private void PatchData()
    {
        //// Fixup Abbreviations
        //foreach(var key in _data.CldrLanguageData.Keys.Where(x=> x.Length == 2))
        //{
        //    var lang = key;
        //    var subkeys = _data.CldrLanguageData.Keys.Where(x =>
        //        x.StartsWith(lang + "_", StringComparison.OrdinalIgnoreCase)).ToArray();

        //    if (subkeys.Length == 0) continue;

        //    var zonesWithShortNames = subkeys.SelectMany(x => _data.CldrLanguageData[x].ShortNames)
        //        .Select(x => x.Key).Distinct().ToArray();

        //    if (zonesWithShortNames.Length == 0) continue;

        //    var zonesWithoutShortNamesInRoot = zonesWithShortNames.Where(x =>
        //        !_data.CldrLanguageData[lang].ShortNames.ContainsKey(x));

        //    //TODO
        //    // get countries for these zones
        //    // get short names for the language within the country
        //    // remap to the root language
        //}

        // Support still-valid old-school ids that cldr doesn't recognize
        // See https://github.com/eggert/tz/blob/2015g/europe#L628-L634
        // Note, these are not quite perfect mappings, but good enough for naming purposes
        _data.CldrAliases["cet"] = "Europe/Paris";
        _data.CldrAliases["eet"] = "Europe/Bucharest";
        _data.CldrAliases["met"] = "Europe/Berlin";
        _data.CldrAliases["wet"] = "Atlantic/Canary";

        // Additional country mappings not in zone.tab
        AddToLookup(_data.TzdbZoneCountries, "Africa/Ceuta", "EA");
        AddToLookup(_data.TzdbZoneCountries, "Atlantic/Canary", "IC");
        AddToLookup(_data.TzdbZoneCountries, "Atlantic/St_Helena", "AC");
        AddToLookup(_data.TzdbZoneCountries, "Atlantic/St_Helena", "TA");
        AddToLookup(_data.TzdbZoneCountries, "Europe/Belgrade", "XK");
        AddToLookup(_data.TzdbZoneCountries, "Indian/Chagos", "DG");
    }

    private static void AddToLookup<TKey, TValue>(IDictionary<TKey, TValue[]> lookup, TKey key, TValue value)
    {
        if (lookup.TryGetValue(key, out var items))
        {
            var temp = new TValue[items.Length + 1];
            items.CopyTo(temp, 0);
            temp[items.Length] = value;
            lookup[key] = temp;
        }
        else
        {
            lookup.Add(key, new[] { value });
        }
    }

    private IList<Instant> GetAllZoneSplitPoints()
    {
        var list = _tzdbProvider.Ids.SelectMany(
                x => _tzdbProvider[x].GetZoneIntervals(Instant.MinValue, Future10).Select(y => y.HasStart ? y.Start : Instant.MinValue))
            .Distinct().OrderBy(x => x).ToList();

        list.Remove(Instant.MinValue);
        list.Remove(Instant.MaxValue);

        return list;
    }
}
