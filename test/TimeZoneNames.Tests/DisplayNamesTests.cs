using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TimeZoneConverter;
using Xunit;
using Xunit.Abstractions;

namespace TimeZoneNames.Tests
{
    public class DisplayNamesTests
    {
        private readonly ITestOutputHelper _output;

        public DisplayNamesTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Can_Get_DisplayNames_For_English()
        {
            IDictionary<string, string> displayNames = TZNames.GetDisplayNames("en");

            foreach ((string key, string value) in displayNames)
            {
                _output.WriteLine($"{key} = {value}");
            }

            Assert.NotEmpty(displayNames);
        }

        [Fact]
        public void Can_Get_DisplayNames_For_OS_Culture()
        {
            string languageCode = CultureInfo.InstalledUICulture.IetfLanguageTag;

            IDictionary<string, string> displayNames = TZNames.GetDisplayNames(languageCode);

            var expected = TimeZoneInfo.GetSystemTimeZones().ToDictionary(x => x.Id, x => x.DisplayName);

            Assert.Equal(expected, displayNames);
        }

        [Fact]
        public void Can_Get_DisplayNames_For_French_Canada_With_Windows_Ids()
        {
            IDictionary<string, string> displayNames = TZNames.GetDisplayNames("fr-CA");

            string pacific = displayNames["Pacific Standard Time"];
            string mountain = displayNames["Mountain Standard Time"];
            string central = displayNames["Central Standard Time"];
            string eastern = displayNames["Eastern Standard Time"];

            Assert.Equal("(UTC-08:00) Pacifique (É.-U. et Canada)", pacific);
            Assert.Equal("(UTC-07:00) Montagnes Rocheuses (É.-U. et Canada)", mountain);
            Assert.Equal("(UTC-06:00) Centre (É.-U. et Canada)", central);
            Assert.Equal("(UTC-05:00) Est (É.-U. et Canada)", eastern);
        }

        [Fact]
        public void Can_Get_DisplayNames_For_French_Canada_With_IANA_Ids()
        {
            IDictionary<string, string> displayNames = TZNames.GetDisplayNames("fr-CA", true);

            string pacific = displayNames["America/Vancouver"];
            string mountain = displayNames["America/Edmonton"];
            string central = displayNames["America/Winnipeg"];
            string eastern = displayNames["America/Toronto"];

            Assert.Equal("(UTC-08:00) Pacifique (É.-U. et Canada)", pacific);
            Assert.Equal("(UTC-07:00) Montagnes Rocheuses (É.-U. et Canada)", mountain);
            Assert.Equal("(UTC-06:00) Centre (É.-U. et Canada)", central);
            Assert.Equal("(UTC-05:00) Est (É.-U. et Canada)", eastern);
        }

        [Fact]
        public void Can_Get_DisplayName_For_Every_Windows_Zone()
        {
            var errors = new List<string>();
            foreach (string id in TZConvert.KnownWindowsTimeZoneIds)
            {
                string displayName = TZNames.GetDisplayNameForTimeZone(id, "en");
                if (string.IsNullOrEmpty(displayName))
                    errors.Add(id);
            }

            Assert.Empty(errors);
        }

        [Fact]
        public void Can_Get_DisplayName_For_Every_Mappable_IANA_Zone()
        {
            string[] unmappableZones = { "Antarctica/Troll" };

            var errors = new List<string>();
            foreach (string id in TZConvert.KnownIanaTimeZoneNames.Except(unmappableZones))
            {
                string displayName = TZNames.GetDisplayNameForTimeZone(id, "en");
                if (string.IsNullOrEmpty(displayName))
                    errors.Add(id);
            }

            Assert.Empty(errors);
        }

        [Fact]
        public void Invalid_Zones_Return_Null()
        {
            string displayName = TZNames.GetDisplayNameForTimeZone("invalid zone", "en");
            Assert.Null(displayName);
        }

        [Fact]
        public void Can_Get_DisplayName_For_Yukon_Standard_Time()
        {
            string displayName = TZNames.GetDisplayNameForTimeZone("Yukon Standard Time", "en");
            Assert.Equal("(UTC-07:00) Yukon", displayName);
        }
    }
}
