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

        public static Task<DataExtractor> LoadAsync()
        {
            return LoadAsync(Downloader.GetTempDir());
        }

        public static async Task<DataExtractor> LoadAsync(string dataPath)
        {
            var data = new DataExtractor(dataPath);
            await data.LoadDataAsync();
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

        private async Task LoadDataAsync()
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
            await Task.WhenAll(actions.Select(Task.Run));
        }

        private async Task DownloadDataAsync()
        {
            if (Directory.Exists(_cldrPath))
                Directory.Delete(_cldrPath, true);

            if (Directory.Exists(_nzdPath))
                Directory.Delete(_nzdPath, true);

            await Task.WhenAll(
                Downloader.DownloadCldrAsync(_cldrPath),
                Downloader.DownloadNzdAsync(_nzdPath));
        }

        private void LoadZoneCountries()
        {
            foreach (var location in _tzdbSource.ZoneLocations.OrderBy(x => GetStandardOffset(x.ZoneId)).ThenBy(x => GetDaylightOffset(x.ZoneId)))
            {
                AddToLookup(_data.TzdbZoneCountries, location.ZoneId, location.CountryCode);
            }
                {


                }
            }
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
                    foreach (var alias in aliases)
                        _data.CldrAliases.Add(alias.ToLowerInvariant(), aliases[0]);
                }
            }
        }

        private void LoadMetaZones()
        {
            using (var stream = File.OpenRead(_cldrPath + @"common\supplemental\metaZones.xml"))
            {
                var doc = XDocument.Load(stream);
                
                var timeZoneElements = doc.XPathSelectElements("/supplementalData/metaZones/metazoneInfo/timezone");
                foreach (var element in timeZoneElements)
                {
                    var timeZone = element.Attribute("type").Value;
                    var metaZone = element.Elements("usesMetazone").Last().Attribute("mzone").Value;
                    _data.CldrMetazones.Add(timeZone, metaZone);
                }

                var mapZoneElements = doc.XPathSelectElements("/supplementalData/metaZones/mapTimezones/mapZone");
                foreach (var element in mapZoneElements)
                {
                    var timeZone = element.Attribute("type").Value;
                    var territory = element.Attribute("territory").Value;
                    if (territory.Length == 2 && !_data.CldrZoneCountries.ContainsKey(timeZone))
                        _data.CldrZoneCountries.Add(timeZone, territory);
                }
            }
        }

        private void LoadWindowsMappings()
        {
            using (var stream = File.OpenRead(_cldrPath + @"common\supplemental\windowsZones.xml"))
            {
                var doc = XDocument.Load(stream);

                var mapZoneElements = doc.XPathSelectElements("/supplementalData/windowsZones/mapTimezones/mapZone");
                foreach (var element in mapZoneElements)
                {
                    var windowsZone = element.Attribute("other").Value;
                    var timeZone = element.Attribute("type").Value.Split().First();
                    var territory = element.Attribute("territory").Value;
                    if (territory == "001") // we only care about the primary territory mapping in this library
                        _data.CldrWindowsMappings.Add(windowsZone, timeZone);
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
                    AddZoneEntries(tzElement, language, "zone", "short");
                    AddZoneEntries(tzElement, language, "zone", "long");
                    AddZoneEntries(tzElement, language, "metazone", "short");
                    AddZoneEntries(tzElement, language, "metazone", "long");
                }
            }
        }

        private void AddCountryEntries(XContainer territoriesElement, string language)
        {
            var countries = territoriesElement.Elements("territory")
                .GroupBy(x => x.Attribute("type").Value)
                .Where(x => x.Key.Length == 2)
                .ToDictionary(x => x.Key, x => x.Last().Value);

            var langData = GetLangData(language);
            foreach (var country in countries)
            {
                // we only need some of the territory names
                if (_data.CldrZoneCountries.ContainsValue(country.Key))
                {
                    langData.CountryNames.Add(country.Key, country.Value);
                }
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
        }

        private void AddZoneEntries(XContainer tzElement, string language, string elementName, string entryName)
        {
            var zones = tzElement.Elements(elementName);
            foreach (var zone in zones)
            {
                var zoneName = zone.Attribute("type").Value;

                var element = zone.Element(entryName);
                if (element == null)
                    continue;

                var values = GetTimeZoneValues(element);

                var langData = GetLangData(language);

                switch (entryName)
                {
                    case "short":
                        langData.ShortNames.Add(zoneName, values);
                        break;
                    case "long":
                        langData.LongNames.Add(zoneName, values);
                        break;
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
    }
}
