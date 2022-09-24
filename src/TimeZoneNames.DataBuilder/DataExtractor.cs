using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using NodaTime;
using NodaTime.TimeZones;

namespace TimeZoneNames.DataBuilder
{
    public class DataExtractor
    {
        private readonly string _cldrPath;
        private readonly string _nzdPath;
        private readonly string _tzresPath;

        private readonly TimeZoneData _data = new TimeZoneData();

        private DataExtractor(string dataPath)
        {
            _cldrPath = Path.Combine(dataPath, "cldr");
            _nzdPath = Path.Combine(dataPath, "nzd");
            _tzresPath = Path.Combine(dataPath, "tzres");
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
            using FileStream stream = File.Create(outputFilePath);
            using var compressedStream = new GZipStream(stream, CompressionLevel.Optimal);
            JsonSerializer.Serialize(compressedStream, _data);
        }

        private TzdbDateTimeZoneSource _tzdbSource;
        private IDateTimeZoneProvider _tzdbProvider;

        private void LoadData()
        {
            // init noda time
            using (FileStream stream = File.OpenRead(Directory.GetFiles(_nzdPath)[0]))
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
                DownloadNzdAsync(overwrite),
                DownloadTZResAsync(overwrite));
        }

        private async Task DownloadCldrAsync(bool overwrite)
        {
            bool exists = Directory.Exists(_cldrPath);
            if (overwrite || !exists)
            {
                if (exists) Directory.Delete(_cldrPath, true);
                await Downloader.DownloadCldrAsync(_cldrPath);
            }
        }

        private async Task DownloadNzdAsync(bool overwrite)
        {
            bool exists = Directory.Exists(_nzdPath);
            if (overwrite || !exists)
            {
                if (exists) Directory.Delete(_nzdPath, true);
                await Downloader.DownloadNzdAsync(_nzdPath);
            }
        }

        private async Task DownloadTZResAsync(bool overwrite)
        {
            bool exists = Directory.Exists(_tzresPath);
            if (overwrite || !exists)
            {
                if (exists) Directory.Delete(_tzresPath, true);
                await Downloader.DownloadTZResAsync(_tzresPath);
            }
        }

        private void LoadZoneCountries()
        {
            foreach (TzdbZoneLocation location in _tzdbSource.ZoneLocations!.OrderBy(x => GetStandardOffset(x.ZoneId)).ThenBy(x => GetDaylightOffset(x.ZoneId)))
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
            DateTimeZone zone = _tzdbProvider[zoneId];
            return Offset.Max(zone.GetZoneInterval(Jan).WallOffset, zone.GetZoneInterval(Jun).WallOffset);
        }

        private void LoadSelectionZones()
        {
            var results = new List<TimeZoneSelectionData>();

            string[] precedence = File.ReadAllLines(Path.Combine("data", "zone-precedence.txt"));

            IList<Instant> splitPoints = GetAllZoneSplitPoints();
            IList<string> last = null;
            bool useConsole = TryHideConsoleCursor();
            for (int i = splitPoints.Count - 1; i >= 0; i--)
            {
                if (useConsole)
                {
                    double pct = 100 * (1.0 * (splitPoints.Count - i)) / splitPoints.Count;
                    Console.Write("{0:F1}%", pct);
                    Console.CursorLeft = 0;
                }

                Instant point = splitPoints[i];
                IList<string> zones = GetSelectionZones(point, precedence);

                if (last == null)
                {
                    last = zones;
                    continue;
                }

                IEnumerable<TimeZoneSelectionData> items = zones.Except(last)
                    .Select(x => new TimeZoneSelectionData { Id = x, ThresholdUtc = point.ToDateTimeUtc() });
                results.AddRange(items);

                last = zones;
            }

            IEnumerable<string> remaining = last?.Except(results.Select(x => x.Id));
            if (remaining != null)
            {
                IEnumerable<TimeZoneSelectionData> items = remaining
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
                .GroupBy(x => new { x.Location.CountryCode, Hash = GetIntervalsHash(x.Intervals) })
                .Select(g =>
                {
                    string[] ids = g.Select(z => z.Id).ToArray();
                    if (ids.Length == 1)
                        return ids[0];

                    // use the zone-precedence.txt file when we need a tiebreaker
                    return precedence.Intersect(ids).First();
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
                foreach (ZoneInterval interval in intervals)
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

            ZoneInterval first = intervals.First();
            if (!first.HasStart || first.Start < start)
            {
                intervals[0] = new ZoneInterval(first.Name, start, first.HasEnd ? first.End : (Instant?)null, first.WallOffset, first.Savings);
            }

            ZoneInterval last = intervals.Last();
            if (!last.HasEnd || last.End > end)
            {
                intervals[^1] = new ZoneInterval(last.Name, last.HasStart ? last.Start : (Instant?)null, end, last.WallOffset, last.Savings);
            }

            return intervals;
        }

        private void LoadZoneAliases()
        {
            using FileStream stream = File.OpenRead(Path.Combine(_cldrPath, "common", "bcp47", "timezone.xml"));
            var doc = XDocument.Load(stream);
            IEnumerable<XElement> elements = doc.XPathSelectElements("/ldmlBCP47/keyword/key[@name='tz']/type");
            foreach (XElement element in elements)
            {
                XAttribute aliasAttribute = element.Attribute("alias");
                if (aliasAttribute == null)
                    continue;

                string[] aliases = aliasAttribute.Value.Split(' ');
                if (aliases.Length == 1)
                    continue;

                foreach (string alias in aliases.Skip(1))
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
            using FileStream stream = File.OpenRead(path);
            var doc = XDocument.Load(stream);

            IEnumerable<XElement> timeZoneElements = doc.XPathSelectElements("/supplementalData/metaZones/metazoneInfo/timezone");
            foreach (XElement element in timeZoneElements)
            {
                string timeZone = element.Attribute("type")!.Value;
                string metaZone = element.Elements("usesMetazone").Last().Attribute("mzone")!.Value;
                _data.CldrMetazones[timeZone] = metaZone;
            }

            IEnumerable<XElement> mapZoneElements = doc.XPathSelectElements("/supplementalData/metaZones/mapTimezones/mapZone");
            foreach (XElement element in mapZoneElements)
            {
                string timeZone = element.Attribute("type")!.Value;
                string territory = element.Attribute("territory")!.Value;
                AddToLookup(_data.CldrZoneCountries, timeZone, territory);
            }

            IEnumerable<XElement> primaryZoneElements = doc.XPathSelectElements("/supplementalData/metaZones/primaryZones/primaryZone");
            foreach (XElement element in primaryZoneElements)
            {
                string country = element.Attribute("iso3166")!.Value;
                string zone = element.Value;
                _data.CldrPrimaryZones[country] = zone;
            }
        }

        private void LoadLanguages()
        {
            IEnumerable<string> languages = Directory.GetFiles(Path.Combine(_cldrPath, "common", "main"))
                .Select(Path.GetFileName)
                .Select(x => x.Substring(0, x.Length - 4));

            Parallel.ForEach(languages, LoadLanguage);
        }

        private void LoadLanguage(string language)
        {
            using FileStream stream = File.OpenRead(Path.Combine(_cldrPath, "common", "main", language + ".xml"));
            var doc = XDocument.Load(stream);

            XElement territoriesElement = doc.XPathSelectElement("/ldml/localeDisplayNames/territories");
            if (territoriesElement != null)
            {
                AddCountryEntries(territoriesElement, language);
            }

            XElement tzElement = doc.XPathSelectElement("/ldml/dates/timeZoneNames");
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
            string[] excluded = new[] { "AN", "BV", "CP", "EU", "HM", "QO", "ZZ" };

            var countries = territoriesElement.Elements("territory")
                .Where(x => x.Attribute("alt") == null)
                .GroupBy(x => x.Attribute("type")!.Value)
                .Where(x => x.Key.Length == 2 && !excluded.Contains(x.Key))
                .ToDictionary(x => x.Key, x => x.First().Value);

            CldrLanguageData langData = GetLangData(language);
            foreach (KeyValuePair<string, string> country in countries)
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
                return;

            var values = new TimeZoneValues();
            if (formats.TryGetValue("generic", out string genericName))
                values.Generic = genericName;
            if (formats.TryGetValue("standard", out string standardName))
                values.Standard = standardName;
            if (formats.TryGetValue("daylight", out string daylightName))
                values.Daylight = daylightName;

            CldrLanguageData langData = GetLangData(language);
            langData.Formats = values;

            XElement fallbackFormat = tzElement.Element("fallbackFormat");
            if (fallbackFormat != null)
                langData.FallbackFormat = fallbackFormat.Value;
        }

        private void AddZoneEntries(XContainer tzElement, string language, string elementName, string entryName)
        {
            CldrLanguageData langData = GetLangData(language);

            IEnumerable<XElement> zones = tzElement.Elements(elementName);
            foreach (XElement zone in zones)
            {
                string zoneName = zone.Attribute("type")!.Value;

                XElement element = zone.Element(entryName);
                if (element == null)
                    continue;

                switch (entryName)
                {
                    case "exemplarCity":
                        {
                            langData.CityNames.Add(zoneName, element.Value);
                            break;
                        }
                    case "short":
                        {
                            TimeZoneValues values = GetTimeZoneValues(element);
                            langData.ShortNames.Add(zoneName, values);
                            break;
                        }
                    case "long":
                        {
                            TimeZoneValues values = GetTimeZoneValues(element);
                            langData.LongNames.Add(zoneName, values);
                            break;
                        }
                }
            }
        }

        private readonly object _locker = new object();

        private CldrLanguageData GetLangData(string language)
        {
            language = language.ToLowerInvariant();

            lock (_locker)
            {
                if (!_data.CldrLanguageData.TryGetValue(language, out CldrLanguageData data))
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

            XElement genericElement = element.Element("generic");
            if (genericElement != null && genericElement.Value != "∅∅∅")
                values.Generic = genericElement.Value;

            XElement standardElement = element.Element("standard");
            if (standardElement != null && standardElement.Value != "∅∅∅")
                values.Standard = standardElement.Value;

            XElement daylightElement = element.Element("daylight");
            if (daylightElement != null && daylightElement.Value != "∅∅∅")
                values.Daylight = daylightElement.Value;

            return values;
        }

        private void LoadDisplayNames()
        {
            using var stream = File.OpenRead(Path.Combine(_tzresPath, "tzinfo.json"));
            var data = JsonNode.Parse(stream);
            var languages = data["Languages"];
            if (languages == null) return;
            foreach (var item in languages.AsArray())
            {
                string locale = item["Locale"].GetValue<string>().Replace("-", "_");
                Dictionary<string, string> timeZones = item["TimeZones"]!.AsObject().ToDictionary(o=> o.Key, o=> (string)o.Value);

                _data.DisplayNames.Add(locale, timeZones);
            }
        }

        private void Fixup(Dictionary<string, string> dictionary, string key, string find, string replace)
        {
            if (dictionary.TryGetValue(key, out string value))
            {
                dictionary[key] = value.Replace(find, replace);
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
            if (lookup.TryGetValue(key, out TValue[] items))
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
}
