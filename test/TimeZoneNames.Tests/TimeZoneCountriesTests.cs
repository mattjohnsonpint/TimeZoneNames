using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace TimeZoneNames.Tests
{
    public class TimeZoneCountriesTests
    {
        private readonly ITestOutputHelper _output;

        public TimeZoneCountriesTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Can_Get_Zones_For_US()
        {
            string[] zones = TZNames.GetTimeZoneIdsForCountry("US", DateTimeOffset.Now);

            foreach (string zone in zones)
                _output.WriteLine(zone);

            string[] expected = {
                "Pacific/Honolulu",
                "America/Adak",
                "America/Anchorage",
                "America/Los_Angeles",
                "America/Phoenix",
                "America/Denver",
                "America/Chicago",
                "America/New_York"
            };

            Assert.Equal(expected, zones);
        }

        [Fact]
        public void Can_Get_Zones_For_GB()
        {
            string[] zones = TZNames.GetTimeZoneIdsForCountry("GB");

            foreach (string zone in zones)
                _output.WriteLine(zone);

            Assert.Single(zones);

            Assert.Equal("Europe/London", zones[0]);
        }

        [Fact]
        public void Can_Get_Zones_For_RU()
        {
            string[] zones = TZNames.GetTimeZoneIdsForCountry("RU", DateTimeOffset.Now);

            foreach (string zone in zones)
                _output.WriteLine(zone);

            string[] expected = {
                "Europe/Kaliningrad",  // +02:00
                "Europe/Moscow",       // +03:00
                "Europe/Samara",       // +04:00
                "Asia/Yekaterinburg",  // +05:00
                "Asia/Omsk",           // +06:00
                "Asia/Novosibirsk",    // +07:00
                "Asia/Irkutsk",        // +08:00
                "Asia/Chita",          // +09:00
                "Asia/Vladivostok",    // +10:00
                "Asia/Sakhalin",       // +11:00
                "Asia/Kamchatka"       // +12:00
            };

            Assert.Equal(expected, zones);
        }

        [Fact]
        public void Can_Get_Zones_For_RU_Past()
        {
            string[] zones = TZNames.GetTimeZoneIdsForCountry("RU", new DateTime(2016, 1, 1));

            foreach (string zone in zones)
                _output.WriteLine(zone);

            string[] expected = {
                "Europe/Kaliningrad",
                "Europe/Moscow",
                "Europe/Volgograd",
                "Europe/Saratov",
                "Europe/Ulyanovsk",
                "Europe/Samara",
                "Asia/Yekaterinburg",
                "Asia/Omsk",
                "Asia/Novosibirsk",
                "Asia/Barnaul",
                "Asia/Tomsk",
                "Asia/Krasnoyarsk",
                "Asia/Irkutsk",
                "Asia/Chita",
                "Asia/Yakutsk",
                "Asia/Vladivostok",
                "Asia/Magadan",
                "Asia/Sakhalin",
                "Asia/Srednekolymsk",
                "Asia/Kamchatka"
            };

            Assert.Equal(expected, zones);
        }

        [Fact]
        public void Can_Get_Zones_For_CA()
        {
            string[] zones = TZNames.GetTimeZoneIdsForCountry("CA", DateTimeOffset.Now);

            foreach (string zone in zones)
                _output.WriteLine(zone);

            string[] expected = {
                "America/Vancouver",
                "America/Dawson_Creek",
                "America/Edmonton",
                "America/Regina",
                "America/Winnipeg",
                "America/Toronto",
                "America/Halifax",
                "America/St_Johns"
            };

            Assert.Equal(expected, zones);
        }

        [Fact]
        public void Can_List_Countries_EN()
        {
            IDictionary<string, string> countries = TZNames.GetCountryNames("en-US");
            foreach ((string countryCode, string countryDisplayName) in countries)
            {
                _output.WriteLine("{0} => {1}", countryCode, countryDisplayName);
            }
        }

        [Fact]
        public void Can_List_Countries_FR()
        {
            IDictionary<string, string> countries = TZNames.GetCountryNames("fr-CA");
            foreach ((string countryCode, string countryDisplayName) in countries)
            {
                _output.WriteLine("{0} => {1}", countryCode, countryDisplayName);
            }
        }

        [Fact]
        public void Can_List_Countries_JP()
        {
            IDictionary<string, string> countries = TZNames.GetCountryNames("ja-JP");
            foreach ((string countryCode, string countryDisplayName) in countries)
            {
                _output.WriteLine("{0} => {1}", countryCode, countryDisplayName);
            }
        }

        [Fact]
        public void Can_Get__Fixed_TimeZones_EN()
        {
            var locale = "en-US";

            IDictionary<string, string> zones = TZNames.GetFixedTimeZoneNames(locale);
            Assert.NotEmpty(zones);
            foreach ((string zoneName, string zoneDisplayName) in zones)
                _output.WriteLine($"{zoneDisplayName,-50} {zoneName}");
        }

        [Fact]
        public void Can_Get__Fixed_TimeZones_EN_abbreviations()
        {
            var locale = "en-US";

            IDictionary<string, string> zones = TZNames.GetFixedTimeZoneAbbreviations(locale);
            Assert.NotEmpty(zones);
            foreach ((string zoneName, string zoneDisplayName) in zones)
                _output.WriteLine($"{zoneDisplayName,-50} {zoneName}");
        }

        [Fact]
        public void Can_Get__Fixed_TimeZones_FR()
        {
            var locale = "fr-FR";

            IDictionary<string, string> zones = TZNames.GetFixedTimeZoneNames(locale);
            Assert.NotEmpty(zones);
            foreach ((string zoneName, string zoneDisplayName) in zones)
                _output.WriteLine($"{zoneDisplayName,-50} {zoneName}");
        }

        [Fact]
        public void Can_Get__Fixed_TimeZones_RU()
        {
            var locale = "ru-RU";

            IDictionary<string, string> zones = TZNames.GetFixedTimeZoneNames(locale);
            Assert.NotEmpty(zones);
            foreach ((string zoneName, string zoneDisplayName) in zones)
                _output.WriteLine($"{zoneDisplayName,-50} {zoneName}");
        }

        [Fact]
        public void Can_Get_TimeZones_For_US_EN()
        {
            var locale = "en-US";

            IDictionary<string, string> zones = TZNames.GetTimeZonesForCountry("US", locale, DateTimeOffset.Now);
            Assert.NotEmpty(zones);
            foreach ((string zoneName, string zoneDisplayName) in zones)
                _output.WriteLine($"{zoneDisplayName,-50} {zoneName}");
        }

        [Fact]
        public void Can_Get_TimeZones_For_RU_EN()
        {
            IDictionary<string, string> zones = TZNames.GetTimeZonesForCountry("RU", "en-US", DateTimeOffset.Now);
            Assert.NotEmpty(zones);
            foreach ((string zoneName, string zoneDisplayName) in zones)
                _output.WriteLine($"{zoneDisplayName,-50} {zoneName}");
        }

        [Fact]
        public void Can_Get_TimeZones_For_RU_RU()
        {
            IDictionary<string, string> zones = TZNames.GetTimeZonesForCountry("RU", "ru-RU", DateTimeOffset.Now);
            Assert.NotEmpty(zones);
            foreach ((string zoneName, string zoneDisplayName) in zones)
                _output.WriteLine($"{zoneDisplayName,-50} {zoneName}");
        }

        [Fact]
        public void Can_Get_TimeZones_For_RU_RU_All()
        {
            IDictionary<string, string> zones = TZNames.GetTimeZonesForCountry("RU", "ru-RU");
            Assert.NotEmpty(zones);
            foreach ((string zoneName, string zoneDisplayName) in zones)
                _output.WriteLine($"{zoneDisplayName,-50} {zoneName}");
        }

        [Fact]
        public void Can_Get_TimeZones_For_All_Countries_EN()
        {
            var locale = "en-US";

            IDictionary<string, string> countries = TZNames.GetCountryNames(locale);
            foreach ((string countryCode, string countryDisplayName) in countries)
            {
                _output.WriteLine("{0} : {1}", countryCode, countryDisplayName);
                _output.WriteLine("------------------------------------------------------------------------------");
                IDictionary<string, string> zones = TZNames.GetTimeZonesForCountry(countryCode, locale);
                //Assert.NotEmpty(zones);
                foreach ((string zoneName, string zoneDisplayName) in zones)
                    _output.WriteLine($"{zoneDisplayName,-50} {zoneName}");

                _output.WriteLine("");
            }
        }

        [Fact]
        public void Can_Get_TimeZones_For_All_Countries_FR()
        {
            var locale = "fr-FR";

            IDictionary<string, string> countries = TZNames.GetCountryNames(locale);
            foreach ((string countryCode, string countryDisplayName) in countries)
            {
                _output.WriteLine("{0} : {1}", countryCode, countryDisplayName);
                _output.WriteLine("------------------------------------------------------------");
                IDictionary<string, string> zones = TZNames.GetTimeZonesForCountry(countryCode, locale);
                //Assert.NotEmpty(zones);
                foreach ((string zoneName, string zoneDisplayName) in zones)
                    _output.WriteLine($"{zoneDisplayName,-50} {zoneName}");

                _output.WriteLine("");
            }
        }

        [Fact]
        public void Can_Get_TimeZones_For_All_Countries_RU()
        {
            var locale = "ru-RU";

            IDictionary<string, string> countries = TZNames.GetCountryNames(locale);
            foreach ((string countryCode, string countryDisplayName) in countries)
            {
                _output.WriteLine("{0} : {1}", countryCode, countryDisplayName);
                _output.WriteLine("------------------------------------------------------------");
                IDictionary<string, string> zones = TZNames.GetTimeZonesForCountry(countryCode, locale);
                //Assert.NotEmpty(zones);
                foreach ((string zoneName, string zoneDisplayName) in zones)
                    _output.WriteLine($"{zoneDisplayName,-50} {zoneName}");

                _output.WriteLine("");
            }
        }

        [Fact]
        public void Can_Get_TimeZones_For_All_Countries_EN_AllZones()
        {
            var locale = "en-US";

            IDictionary<string, string> countries = TZNames.GetCountryNames(locale);
            foreach ((string countryCode, string countryDisplayName) in countries)
            {
                _output.WriteLine("{0} : {1}", countryCode, countryDisplayName);
                _output.WriteLine("------------------------------------------------------------");
                IDictionary<string, string> zones = TZNames.GetTimeZonesForCountry(countryCode, locale);
                //Assert.NotEmpty(zones);
                foreach ((string zoneName, string zoneDisplayName) in zones)
                    _output.WriteLine($"{zoneDisplayName,-50} {zoneName}");

                _output.WriteLine("");
            }
        }
    }
}
