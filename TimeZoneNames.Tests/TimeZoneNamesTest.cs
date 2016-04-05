using Xunit;

namespace TimeZoneNames.Tests
{
    public class TimeZoneNamesTest
    {
        [Fact]
        public void Can_Get_Names_For_US_Pacific()
        {
            var names = TZNames.GetNamesForTimeZone("America/Los_Angeles", "en-US");

            Assert.Equal("Pacific Time", names.Generic);
            Assert.Equal("Pacific Standard Time", names.Standard);
            Assert.Equal("Pacific Daylight Time", names.Daylight);
        }

        [Fact]
        public void Can_Get_Abbreviations_For_US_Pacific()
        {
            var abbreviations = TZNames.GetAbbreviationsForTimeZone("America/Los_Angeles", "en-US");

            Assert.Equal("PT", abbreviations.Generic);
            Assert.Equal("PST", abbreviations.Standard);
            Assert.Equal("PDT", abbreviations.Daylight);
        }

        [Fact]
        public void Can_Get_French_Names_For_US_Pacific()
        {
            var names = TZNames.GetNamesForTimeZone("America/Los_Angeles", "fr-CA");

            Assert.Equal("heure du Pacifique", names.Generic);
            Assert.Equal("heure normale du Pacifique", names.Standard);
            Assert.Equal("heure avancée du Pacifique", names.Daylight);
        }

        [Fact]
        public void Can_Get_French_Abbreviations_For_US_Pacific()
        {
            var abbreviations = TZNames.GetAbbreviationsForTimeZone("America/Los_Angeles", "fr-CA");

            Assert.Equal("HP", abbreviations.Generic);
            Assert.Equal("HNP", abbreviations.Standard);
            Assert.Equal("HAP", abbreviations.Daylight);
        }

        [Fact]
        public void Can_Get_Names_For_UK()
        {
            var names = TZNames.GetNamesForTimeZone("Europe/London", "en-US");

            Assert.Equal("U.K. Time", names.Generic);
            Assert.Equal("Greenwich Mean Time", names.Standard);
            Assert.Equal("British Summer Time", names.Daylight);
        }

        [Fact]
        public void Can_Get_Abbreviations_For_UK()
        {
            var abbreviations = TZNames.GetAbbreviationsForTimeZone("Europe/London", "en-US");

            Assert.Null(abbreviations.Generic);
            Assert.Equal("GMT", abbreviations.Standard);
            Assert.Equal("BST", abbreviations.Daylight);
        }

        [Fact]
        public void Can_Get_Names_For_IE()
        {
            var names = TZNames.GetNamesForTimeZone("Europe/Dublin", "en-US");

            Assert.Equal("Ireland Time", names.Generic);
            Assert.Equal("Greenwich Mean Time", names.Standard);
            Assert.Equal("Irish Standard Time", names.Daylight);
        }

        [Fact]
        public void Can_Get_Abbreviations_For_IE()
        {
            var abbreviations = TZNames.GetAbbreviationsForTimeZone("Europe/Dublin", "en-US");

            Assert.Null(abbreviations.Generic);
            Assert.Equal("GMT", abbreviations.Standard);
            Assert.Equal("IST", abbreviations.Daylight);
        }

        [Fact]
        public void Can_Get_Names_For_IN1()
        {
            var names = TZNames.GetNamesForTimeZone("Asia/Calcutta", "en-US");

            Assert.Equal("India Standard Time", names.Generic);
            Assert.Equal("India Standard Time", names.Standard);
            Assert.Equal(null, names.Daylight);
        }

        [Fact]
        public void Can_Get_Abbreviations_For_IN1()
        {
            var abbreviations = TZNames.GetAbbreviationsForTimeZone("Asia/Calcutta", "en-US");

            Assert.Equal("IST", abbreviations.Generic);
            Assert.Equal("IST", abbreviations.Standard);
            Assert.Equal(null, abbreviations.Daylight);
        }

        [Fact]
        public void Can_Get_Names_For_IN2()
        {
            var names = TZNames.GetNamesForTimeZone("Asia/Kolkata", "en-US");

            Assert.Equal("India Standard Time", names.Generic);
            Assert.Equal("India Standard Time", names.Standard);
            Assert.Equal(null, names.Daylight);
        }

        [Fact]
        public void Can_Get_Abbreviations_For_IN2()
        {
            var abbreviations = TZNames.GetAbbreviationsForTimeZone("Asia/Kolkata", "en-US");

            Assert.Equal("IST", abbreviations.Generic);
            Assert.Equal("IST", abbreviations.Standard);
            Assert.Equal(null, abbreviations.Daylight);
        }

        [Fact]
        public void Can_Get_Names_For_Windows_Timezone()
        {
            var names = TZNames.GetNamesForTimeZone("Eastern Standard Time", "en-US");

            Assert.Equal("Eastern Time", names.Generic);
            Assert.Equal("Eastern Standard Time", names.Standard);
            Assert.Equal("Eastern Daylight Time", names.Daylight);
        }
    }
}
