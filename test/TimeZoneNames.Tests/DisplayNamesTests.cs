using System;
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
            var displayNames = TZNames.GetDisplayNames("en");

            foreach (var item in displayNames)
            {
                _output.WriteLine($"{item.Key} = {item.Value}");
            }

            Assert.NotEmpty(displayNames);
        }

        [Fact]
        public void Can_Get_DisplayNames_For_OS_Culture()
        {
            var languageCode = CultureInfo.InstalledUICulture.IetfLanguageTag;

            var displayNames = TZNames.GetDisplayNames(languageCode);

            var expected = TimeZoneInfo.GetSystemTimeZones().ToDictionary(x => x.Id, x => x.DisplayName);

            Assert.Equal(expected, displayNames);
        }

        [Fact]
        public void Can_Get_DisplayNames_For_French_Canada_With_Windows_Ids()
        {
            var displayNames = TZNames.GetDisplayNames("fr-CA");

            var pacific = displayNames["Pacific Standard Time"];
            var mountain = displayNames["Mountain Standard Time"];
            var central = displayNames["Central Standard Time"];
            var eastern = displayNames["Eastern Standard Time"];

            Assert.Equal("(UTC-08:00) Pacifique (É.-U. et Canada)", pacific);
            Assert.Equal("(UTC-07:00) Montagnes Rocheuses (É.-U. et Canada)", mountain);
            Assert.Equal("(UTC-06:00) Centre (É.-U. et Canada)", central);
            Assert.Equal("(UTC-05:00) Est (É.-U. et Canada)", eastern);
        }

        [Fact]
        public void Can_Get_DisplayNames_For_French_Canada_With_IANA_Ids()
        {
            var displayNames = TZNames.GetDisplayNames("fr-CA", true);

            var pacific = displayNames["America/Vancouver"];
            var mountain = displayNames["America/Edmonton"];
            var central = displayNames["America/Winnipeg"];
            var eastern = displayNames["America/Toronto"];

            Assert.Equal("(UTC-08:00) Pacifique (É.-U. et Canada)", pacific);
            Assert.Equal("(UTC-07:00) Montagnes Rocheuses (É.-U. et Canada)", mountain);
            Assert.Equal("(UTC-06:00) Centre (É.-U. et Canada)", central);
            Assert.Equal("(UTC-05:00) Est (É.-U. et Canada)", eastern);
        }

        [Fact]
        public void Can_Get_DisplayName_For_Every_Windows_Zone()
        {
            foreach (var id in TZConvert.KnownWindowsTimeZoneIds)
            {
                var displayName = TZNames.GetDisplayNameForTimeZone(id, "en");
                Assert.NotNull(displayName);
            }
        }

        [Fact]
        public void Can_Get_DisplayName_For_Every_IANA_Zone()
        {
            foreach (var id in TZConvert.KnownIanaTimeZoneNames)
            {
                var displayName = TZNames.GetDisplayNameForTimeZone(id, "en");
                Assert.NotNull(displayName);
            }
        }
    }
}
