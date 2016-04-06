using System;
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
            var zones = TZNames.GetTimeZoneIdsForCountry("US", DateTimeOffset.Now);

            foreach (var zone in zones)
                _output.WriteLine(zone);

            var expected = new[]
            {
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
            var zones = TZNames.GetTimeZoneIdsForCountry("GB");

            foreach (var zone in zones)
                _output.WriteLine(zone);

            Assert.Equal(1, zones.Length);

            Assert.Equal("Europe/London", zones[0]);
        }

        [Fact]
        public void Can_Get_Zones_For_RU()
        {
            var zones = TZNames.GetTimeZoneIdsForCountry("RU", DateTimeOffset.Now);

            foreach (var zone in zones)
                _output.WriteLine(zone);

            var expected = new[]
            {
                "Europe/Kaliningrad",
                "Europe/Moscow",
                "Europe/Samara",
                "Asia/Yekaterinburg",
                "Asia/Novosibirsk",
                "Asia/Krasnoyarsk",
                "Asia/Irkutsk",
                "Asia/Yakutsk",
                "Asia/Magadan",
                "Asia/Srednekolymsk",
                "Asia/Kamchatka"
            };

            Assert.Equal(expected, zones);
        }

        [Fact]
        public void Can_Get_Zones_For_RU_Past()
        {
            var zones = TZNames.GetTimeZoneIdsForCountry("RU", new DateTime(2016, 1, 1));

            foreach (var zone in zones)
                _output.WriteLine(zone);

            var expected = new[]
            {
                "Europe/Kaliningrad",
                "Europe/Moscow",
                "Europe/Astrakhan",
                "Europe/Samara",
                "Asia/Yekaterinburg",
                "Asia/Novosibirsk",
                "Asia/Barnaul",
                "Asia/Krasnoyarsk",
                "Asia/Irkutsk",
                "Asia/Chita",
                "Asia/Yakutsk",
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
            var zones = TZNames.GetTimeZoneIdsForCountry("CA", DateTimeOffset.Now);

            foreach (var zone in zones)
                _output.WriteLine(zone);

            var expected = new[]
            {
                "America/Vancouver",
                "America/Dawson_Creek",
                "America/Edmonton",
                "America/Regina",
                "America/Winnipeg",
                "America/Atikokan",
                "America/Toronto",
                "America/Blanc-Sablon",
                "America/Halifax",
                "America/St_Johns"
            };

            Assert.Equal(expected, zones);
        }

        [Fact]
        public void Can_List_Countries_EN()
        {
            var countries = TZNames.GetCountryNames("en-US");
            foreach (var country in countries)
            {
                _output.WriteLine("{0} => {1}", country.Key, country.Value);
            }
        }

        [Fact]
        public void Can_List_Countries_FR()
        {
            var countries = TZNames.GetCountryNames("fr-CA");
            foreach (var country in countries)
            {
                _output.WriteLine("{0} => {1}", country.Key, country.Value);
            }
        }

        [Fact]
        public void Can_List_Countries_JP()
        {
            var countries = TZNames.GetCountryNames("ja-JP");
            foreach (var country in countries)
            {
                _output.WriteLine("{0} => {1}", country.Key, country.Value);
            }
        }

        [Fact]
        public void Can_Get__Fixed_TimeZones_EN()
        {
            var locale = "en-US";

            var zones = TZNames.GetFixedTimeZoneNames(locale);
            Assert.NotEmpty(zones);
            foreach (var zone in zones)
                _output.WriteLine($"{zone.Value.PadRight(50)} {zone.Key}");
        }

        [Fact]
        public void Can_Get__Fixed_TimeZones_EN_abbreviations()
        {
            var locale = "en-US";

            var zones = TZNames.GetFixedTimeZoneAbbreviations(locale);
            Assert.NotEmpty(zones);
            foreach (var zone in zones)
                _output.WriteLine($"{zone.Value.PadRight(50)} {zone.Key}");
        }

        [Fact]
        public void Can_Get__Fixed_TimeZones_FR()
        {
            var locale = "fr-FR";

            var zones = TZNames.GetFixedTimeZoneNames(locale);
            Assert.NotEmpty(zones);
            foreach (var zone in zones)
                _output.WriteLine($"{zone.Value.PadRight(50)} {zone.Key}");
        }

        [Fact]
        public void Can_Get__Fixed_TimeZones_RU()
        {
            var locale = "ru-RU";

            var zones = TZNames.GetFixedTimeZoneNames(locale);
            Assert.NotEmpty(zones);
            foreach (var zone in zones)
                _output.WriteLine($"{zone.Value.PadRight(50)} {zone.Key}");
        }

        [Fact]
        public void Can_Get_TimeZones_For_US_EN()
        {
            var locale = "en-US";

            var zones = TZNames.GetTimeZonesForCountry("US", locale, DateTimeOffset.Now);
            Assert.NotEmpty(zones);
            foreach (var zone in zones)
                _output.WriteLine($"{zone.Value.PadRight(50)} {zone.Key}");
        }

        [Fact]
        public void Can_Get_TimeZones_For_RU_EN()
        {
            var zones = TZNames.GetTimeZonesForCountry("RU", "en-US", DateTimeOffset.Now);
            Assert.NotEmpty(zones);
            foreach (var zone in zones)
                _output.WriteLine($"{zone.Value.PadRight(50)} {zone.Key}");
        }

        [Fact]
        public void Can_Get_TimeZones_For_RU_RU()
        {
            var zones = TZNames.GetTimeZonesForCountry("RU", "ru-RU", DateTimeOffset.Now);
            Assert.NotEmpty(zones);
            foreach (var zone in zones)
                _output.WriteLine($"{zone.Value.PadRight(50)} {zone.Key}");
        }

        [Fact]
        public void Can_Get_TimeZones_For_RU_RU_All()
        {
            var zones = TZNames.GetTimeZonesForCountry("RU", "ru-RU");
            Assert.NotEmpty(zones);
            foreach (var zone in zones)
                _output.WriteLine($"{zone.Value.PadRight(50)} {zone.Key}");
        }

        [Fact]
        public void Can_Get_TimeZones_For_All_Countries_EN()
        {
            var locale = "en-US";

            var countries = TZNames.GetCountryNames(locale);
            foreach (var country in countries)
            {
                _output.WriteLine("{0} : {1}", country.Key, country.Value);
                _output.WriteLine("------------------------------------------------------------------------------");
                var zones = TZNames.GetTimeZonesForCountry(country.Key, locale);
                //Assert.NotEmpty(zones);
                foreach (var zone in zones)
                    _output.WriteLine($"{zone.Value.PadRight(50)} {zone.Key}");

                _output.WriteLine("");
            }
        }

        [Fact]
        public void Can_Get_TimeZones_For_All_Countries_FR()
        {
            var locale = "fr-FR";

            var countries = TZNames.GetCountryNames(locale);
            foreach (var country in countries)
            {
                _output.WriteLine("{0} : {1}", country.Key, country.Value);
                _output.WriteLine("------------------------------------------------------------");
                var zones = TZNames.GetTimeZonesForCountry(country.Key, locale);
                //Assert.NotEmpty(zones);
                foreach (var zone in zones)
                    _output.WriteLine($"{zone.Value.PadRight(50)} {zone.Key}");

                _output.WriteLine("");
            }
        }

        [Fact]
        public void Can_Get_TimeZones_For_All_Countries_RU()
        {
            var locale = "ru-RU";

            var countries = TZNames.GetCountryNames(locale);
            foreach (var country in countries)
            {
                _output.WriteLine("{0} : {1}", country.Key, country.Value);
                _output.WriteLine("------------------------------------------------------------");
                var zones = TZNames.GetTimeZonesForCountry(country.Key, locale);
                //Assert.NotEmpty(zones);
                foreach (var zone in zones)
                    _output.WriteLine($"{zone.Value.PadRight(50)} {zone.Key}");

                _output.WriteLine("");
            }
        }

        [Fact]
        public void Can_Get_TimeZones_For_All_Countries_EN_AllZones()
        {
            var locale = "en-US";

            var countries = TZNames.GetCountryNames(locale);
            foreach (var country in countries)
            {
                _output.WriteLine("{0} : {1}", country.Key, country.Value);
                _output.WriteLine("------------------------------------------------------------");
                var zones = TZNames.GetTimeZonesForCountry(country.Key, locale);
                //Assert.NotEmpty(zones);
                foreach (var zone in zones)
                    _output.WriteLine($"{zone.Value.PadRight(50)} {zone.Key}");

                _output.WriteLine("");
            }
        }
    }
}
