using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using ProtoBuf;

namespace TimeZoneNames.DataBuilder
{
    public class DataExtractor
    {
        private readonly string _cldrPath;
        private readonly string _tzdbPath;

        private readonly TimeZoneData _data = new TimeZoneData();

        private DataExtractor(string dataPath)
        {
            _cldrPath = Path.Combine(dataPath, "cldr") + "\\";
            _tzdbPath = Path.Combine(dataPath, "tzdb") + "\\";
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
        
        private async Task LoadDataAsync()
        {
            //await DownloadDataAsync();

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

            PatchData();
        }

        private async Task DownloadDataAsync()
        {
            if (Directory.Exists(_tzdbPath))
                Directory.Delete(_tzdbPath, true);
                
            if (Directory.Exists(_cldrPath))
                Directory.Delete(_cldrPath, true);

            await Task.WhenAll(
                Downloader.DownloadCldrAsync(_cldrPath),
                Downloader.DownloadTzdbAsync(_tzdbPath));
        }

        private void LoadZoneCountries()
        {
            using (var stream = File.OpenRead(_tzdbPath + "zone.tab"))
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("#"))
                        continue;

                    var fields = line.Split('\t');
                    var country = fields[0];
                    var zone = fields[2];

                    _data.TzdbZoneCountries.Add(zone, country);
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

                var primaryZoneElements = doc.XPathSelectElements("/supplementalData/metaZones/primaryZones/primaryZone");
                foreach (var element in primaryZoneElements)
                {
                    var country = element.Attribute("iso3166").Value;
                    var zone = element.Value;
                    _data.CldrPrimaryZones.Add(country, zone);
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
            var countries = territoriesElement.Elements("territory")
                .GroupBy(x => x.Attribute("type").Value)
                .Where(x => x.Key.Length == 2)
                .ToDictionary(x => x.Key, x => x.Last().Value);

            var langData = GetLangData(language);
            foreach (var country in countries)
            {
                // we only need some of the territory names
                //if (_data.CldrZoneCountries.ContainsValue(country.Key))
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
            _data.CldrWindowsMappings.Add("Mid-Atlantic Standard Time", "Etc/GMT+2");
            _data.CldrWindowsMappings.Add("Kamchatka Standard Time", "Asia/Kamchatka");
            
            // Add mapping for mappings not yet in CLDR
            _data.CldrWindowsMappings.Add("North Korea Standard Time", "Asia/Pyongyang");
            _data.CldrWindowsMappings.Add("E. Europe Standard Time", "Europe/Chisinau");

            // Support still-valid old-school ids that cldr doesn't recognize
            // See https://github.com/eggert/tz/blob/2015g/europe#L628-L634
            // Note, these are not quite perfect mappings, but good enough for naming purposes
            _data.CldrAliases["cet"] = "Europe/Paris";
            _data.CldrAliases["eet"] = "Europe/Bucharest";
            _data.CldrAliases["met"] = "Europe/Berlin";
            _data.CldrAliases["wet"] = "Atlantic/Canary";

            // Support UTC - Not in CLDR!
            AddStandardGenericName("en", "Etc/GMT", "Coordinated Universal Time");
            AddStandardGenericName("es", "Etc/GMT", "tiempo universal coordinado");
            AddStandardGenericName("fr", "Etc/GMT", "temps universel coordonné");
            // TODO: many more needed!
        }

        private void AddStandardGenericName(string language, string zone, string name)
        {
            var values = new TimeZoneValues {Generic = name, Standard = name};
            _data.CldrLanguageData[language].LongNames.Add(zone, values);
        }
    }
}
