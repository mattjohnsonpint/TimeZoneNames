using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using NodaTime;
using NodaTime.TimeZones;
using ProtoBuf;

namespace TimeZoneNames.DataBuilder
{
    public class DataExtractor
    {
        private readonly string _cldrPath;
        private readonly string _nzdPath;

        private readonly TimeZoneData _data = new TimeZoneData();

        private DataExtractor(string dataPath)
        {
            _cldrPath = Path.Combine(dataPath, "cldr") + "\\";
            _nzdPath = Path.Combine(dataPath, "nzd") + "\\";
        }

        public static DataExtractor Load(string dataPath, bool overwrite)
        {
            var data = new DataExtractor(dataPath);

            if (overwrite || !Directory.Exists(dataPath))
                data.DownloadData();

            data.LoadData();
            return data;
        }

        public void SaveData(string outputPath)
        {
            using (var stream = File.Create(Path.Combine(outputPath, "tz.dat")))
            {
                Serializer.Serialize(stream, _data);
            }
        }

        private TzdbDateTimeZoneSource _tzdbSource;
        private IDateTimeZoneProvider _tzdbProvider;

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
                LoadWindowsMappings,
                LoadLanguages
            };
            Task.WhenAll(actions.Select(Task.Run)).Wait();

            // patch the data for any known issues
            PatchData();

            // this has to come last, as it relies on patched data
            LoadSelectionZones();
        }

        private void DownloadData()
        {
            if (Directory.Exists(_cldrPath))
                Directory.Delete(_cldrPath, true);

            if (Directory.Exists(_nzdPath))
                Directory.Delete(_nzdPath, true);

            Task.WaitAll(
                Downloader.DownloadCldrAsync(_cldrPath),
                Downloader.DownloadNzdAsync(_nzdPath));
        }

        private void LoadZoneCountries()
        {
            foreach (var location in _tzdbSource.ZoneLocations.OrderBy(x => GetStandardOffset(x.ZoneId)).ThenBy(x => GetDaylightOffset(x.ZoneId)))
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

            var precedence = File.ReadAllLines(@"data\zone-precedence.txt");

            var splitPoints = GetAllZoneSplitPoints();
            IList<string> last = null;
            Console.CursorVisible = false;
            for (int i = splitPoints.Count - 1; i >= 0; i--)
            {
                var pct = 100 * (1.0 * (splitPoints.Count - i)) / splitPoints.Count;
                Console.Write("{0:F1}%", pct);
                Console.CursorLeft = 0;
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

            Console.WriteLine();
            Console.CursorVisible = true;
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
                    Location = _tzdbSource.ZoneLocations.FirstOrDefault(l => l.ZoneId == x)
                })
                .Where(x => x.Location != null)
                .GroupBy(x => new { x.Location.CountryCode, Hash = GetIntervalsHash(x.Intervals) })
                .Select(g =>
                {
                    var ids = g.Select(z => z.Id).ToArray();
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
            int hash = 17;
            unchecked
            {
                foreach (var interval in intervals)
                {
                    hash = hash * 23 + interval.GetHashCode();
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
                intervals[0] = new ZoneInterval(first.Name, start, first.HasEnd ? first.End : (Instant?) null, first.WallOffset, first.Savings);
            }

            var last = intervals.Last();
            if (!last.HasEnd || last.End > end)
            {
                intervals[intervals.Count - 1] = new ZoneInterval(last.Name, last.HasStart ? last.Start : (Instant?) null, end, last.WallOffset, last.Savings);
            }

            return intervals;
        }

        private void LoadZoneAliases()
        {
            using (var stream = File.OpenRead(_cldrPath + @"common\bcp47\timezone.xml"))
            {
                var doc = XDocument.Load(stream);
                var elements = doc.XPathSelectElements("/ldmlBCP47/keyword/key[@name='tz']/type");
                foreach (var element in elements)
                {
                    var aliasAttribute = element.Attribute("alias");
                    if (aliasAttribute == null)
                        continue;

                    var aliases = aliasAttribute.Value.Split(' ');
                    if (aliases.Length == 1)
                        continue;

                    foreach (var alias in aliases.Skip(1))
                        _data.CldrAliases.Add(alias.ToLowerInvariant(), aliases[0]);
                }
            }
        }

        private void LoadMetaZones()
        {
            LoadMetaZonesFromFile(_cldrPath + @"common\supplemental\metaZones.xml");
            LoadMetaZonesFromFile(@"data\metaZones-override.xml");
        }

        private void LoadMetaZonesFromFile(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                var doc = XDocument.Load(stream);

                var timeZoneElements = doc.XPathSelectElements("/supplementalData/metaZones/metazoneInfo/timezone");
                foreach (var element in timeZoneElements)
                {
                    var timeZone = element.Attribute("type").Value;
                    var metaZone = element.Elements("usesMetazone").Last().Attribute("mzone").Value;
                    _data.CldrMetazones.AddOrUpdate(timeZone, metaZone);
                }

                var mapZoneElements = doc.XPathSelectElements("/supplementalData/metaZones/mapTimezones/mapZone");
                foreach (var element in mapZoneElements)
                {
                    var timeZone = element.Attribute("type").Value;
                    var territory = element.Attribute("territory").Value;
                    AddToLookup(_data.CldrZoneCountries, timeZone, territory);
                }

                var primaryZoneElements = doc.XPathSelectElements("/supplementalData/metaZones/primaryZones/primaryZone");
                foreach (var element in primaryZoneElements)
                {
                    var country = element.Attribute("iso3166").Value;
                    var zone = element.Value;
                    _data.CldrPrimaryZones.AddOrUpdate(country, zone);
                }
            }
        }

        private void LoadWindowsMappings()
        {
            LoadWindowsMappingsFromFile(_cldrPath + @"common\supplemental\windowsZones.xml");
            LoadWindowsMappingsFromFile(@"data\windowsZones-override.xml");
        }

        private void LoadWindowsMappingsFromFile(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                var doc = XDocument.Load(stream);

                var mapZoneElements = doc.XPathSelectElements("/supplementalData/windowsZones/mapTimezones/mapZone");
                foreach (var element in mapZoneElements)
                {
                    var windowsZone = element.Attribute("other").Value;
                    var timeZone = element.Attribute("type").Value.Split().First();
                    var territory = element.Attribute("territory").Value;

                    Dictionary<string, string> mappings;
                    if (!_data.CldrWindowsMappings.TryGetValue(territory, out mappings))
                    {
                        mappings = new Dictionary<string, string>();
                        _data.CldrWindowsMappings.Add(territory, mappings);
                    }

                    if (timeZone == string.Empty)
                    {
                        if (mappings.ContainsKey(windowsZone))
                            mappings.Remove(windowsZone);
                    }
                    else
                    {
                        mappings.AddOrUpdate(windowsZone, timeZone);
                    }
                }
            }
        }

        private void LoadLanguages()
        {
            var languages = Directory.GetFiles(_cldrPath + @"common\main")
                .Select(Path.GetFileName)
                .Select(x => x.Substring(0, x.Length - 4));

            Parallel.ForEach(languages, LoadLanguage);
        }

        private void LoadLanguage(string language)
        {
            using (var stream = File.OpenRead(_cldrPath + @"common\main\" + language + ".xml"))
            {
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
        }

        private void AddCountryEntries(XContainer territoriesElement, string language)
        {
            var excluded = new[] { "AN", "BV", "CP", "EU", "HM", "QO", "ZZ" };

            var countries = territoriesElement.Elements("territory")
                .Where(x => x.Attribute("alt") == null)
                .GroupBy(x => x.Attribute("type").Value)
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
                    x => x.Attribute("type") == null ? "generic" : x.Attribute("type").Value,
                    x => x.Value);

            if (formats.Count == 0)
                return;

            string s;
            var values = new TimeZoneValues();
            if (formats.TryGetValue("generic", out s))
                values.Generic = s;
            if (formats.TryGetValue("standard", out s))
                values.Standard = s;
            if (formats.TryGetValue("daylight", out s))
                values.Daylight = s;

            var langData = GetLangData(language);
            langData.Formats = values;

            var fallbackFormat = tzElement.Element("fallbackFormat");
            if (fallbackFormat != null)
                langData.FallbackFormat = fallbackFormat.Value;
        }

        private void AddZoneEntries(XContainer tzElement, string language, string elementName, string entryName)
        {
            var langData = GetLangData(language);

            var zones = tzElement.Elements(elementName);
            foreach (var zone in zones)
            {
                var zoneName = zone.Attribute("type").Value;

                var element = zone.Element(entryName);
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

        private readonly object _locker = new object();

        private CldrLanguageData GetLangData(string language)
        {
            language = language.ToLowerInvariant();

            lock (_locker)
            {
                CldrLanguageData data;
                if (!_data.CldrLanguageData.TryGetValue(language, out data))
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
                values.Generic = genericElement.Value;

            var standardElement = element.Element("standard");
            if (standardElement != null && standardElement.Value != "∅∅∅")
                values.Standard = standardElement.Value;

            var daylightElement = element.Element("daylight");
            if (daylightElement != null && daylightElement.Value != "∅∅∅")
                values.Daylight = daylightElement.Value;

            return values;
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

            // Add additional mappings to support obsolete Windows time zone IDs
            _data.CldrWindowsMappings["001"].Add("Mid-Atlantic Standard Time", "Etc/GMT+2");
            _data.CldrWindowsMappings["001"].Add("Kamchatka Standard Time", "Asia/Kamchatka");

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

            // Support UTC - Not in CLDR!
            using (var file = File.OpenRead(@"data\utc.txt"))
            using (var reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        break;

                    var parts = line.Split(',');
                    if (parts.Length == 2)
                        AddStandardGenericName(parts[0], "Etc/GMT", parts[1]);
                }
            }

            // Support localizations of cities for new zones not yet in CLDR
            using (var file = File.OpenRead(@"data\cities.txt"))
            using (var reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        break;

                    var parts = line.Split(',');
                    if (parts.Length == 3)
                    {
                        var zone = _tzdbProvider.Ids.FirstOrDefault(x => x.EndsWith("/" + parts[0]));
                        AddCityName(parts[1], zone, parts[2]);
                    }
                }
            }
        }

        private void AddStandardGenericName(string language, string zone, string name)
        {
            try
            {
                var langKey = NormalizeLangKey(language);
                var values = new TimeZoneValues { Generic = name, Standard = name };
                _data.CldrLanguageData[langKey].LongNames.Add(zone, values);
            }
            catch
            {
                Console.WriteLine("No CLDR data for language " + language);
            }
        }

        private void AddCityName(string language, string zone, string name)
        {
            try
            {
                var langKey = NormalizeLangKey(language);
                var cityNames = _data.CldrLanguageData[langKey].CityNames;
                if (!cityNames.ContainsKey(zone))
                    cityNames.Add(zone, name);
            }
            catch
            {
                Console.WriteLine("No CLDR data for language " + language);
            }
        }

        private string NormalizeLangKey(string language)
        {
            var langKey = language.Replace("-", "_").ToLowerInvariant();
            if (!_data.CldrLanguageData.ContainsKey(langKey))
                langKey = langKey.Split('_')[0];

            return langKey;
        }

        private static void AddToLookup<TKey, TValue>(IDictionary<TKey, TValue[]> lookup, TKey key, TValue value)
        {
            TValue[] items;
            if (lookup.TryGetValue(key, out items))
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
