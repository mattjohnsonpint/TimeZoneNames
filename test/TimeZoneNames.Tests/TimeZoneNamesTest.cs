using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using Xunit;
using Xunit.Abstractions;

namespace TimeZoneNames.Tests
{
    public class TimeZoneNamesTest
    {
        private readonly ITestOutputHelper _output;

        public TimeZoneNamesTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Can_Get_Names_For_US_Pacific()
        {
            TimeZoneValues names = TZNames.GetNamesForTimeZone("America/Los_Angeles", "en-US");

            Assert.Equal("Pacific Time", names.Generic);
            Assert.Equal("Pacific Standard Time", names.Standard);
            Assert.Equal("Pacific Daylight Time", names.Daylight);
        }

        [Fact]
        public void Can_Get_Abbreviations_For_US_Pacific()
        {
            TimeZoneValues abbreviations = TZNames.GetAbbreviationsForTimeZone("America/Los_Angeles", "en-US");

            Assert.Equal("PT", abbreviations.Generic);
            Assert.Equal("PST", abbreviations.Standard);
            Assert.Equal("PDT", abbreviations.Daylight);
        }

        [Fact]
        public void Can_Get_French_Names_For_US_Pacific()
        {
            TimeZoneValues names = TZNames.GetNamesForTimeZone("America/Los_Angeles", "fr-CA");

            Assert.Equal("heure du Pacifique", names.Generic);
            Assert.Equal("heure normale du Pacifique", names.Standard);
            Assert.Equal("heure avancée du Pacifique", names.Daylight);
        }

        [Fact]
        public void Can_Get_French_Abbreviations_For_US_Pacific()
        {
            TimeZoneValues abbreviations = TZNames.GetAbbreviationsForTimeZone("America/Los_Angeles", "fr-CA");

            Assert.Equal("HP", abbreviations.Generic);
            Assert.Equal("HNP", abbreviations.Standard);
            Assert.Equal("HAP", abbreviations.Daylight);
        }

        [Fact]
        public void Can_Get_Names_For_US_Arizona()
        {
            TimeZoneValues names = TZNames.GetNamesForTimeZone("America/Phoenix", "en-US");

            Assert.Equal("Mountain Time", names.Generic);
            Assert.Equal("Mountain Standard Time", names.Standard);
            Assert.Equal("Mountain Daylight Time", names.Daylight);
        }


        [Fact]
        public void Can_Get_Names_For_UK()
        {
            TimeZoneValues names = TZNames.GetNamesForTimeZone("Europe/London", "en-US");

            Assert.Equal("United Kingdom Time", names.Generic);
            Assert.Equal("Greenwich Mean Time", names.Standard);
            Assert.Equal("British Summer Time", names.Daylight);
        }

        [Fact]
        public void Can_Get_Abbreviations_For_UK()
        {
            TimeZoneValues abbreviations = TZNames.GetAbbreviationsForTimeZone("Europe/London", "en-US");

            Assert.Null(abbreviations.Generic);
            Assert.Equal("GMT", abbreviations.Standard);
            Assert.Equal("BST", abbreviations.Daylight);
        }

        [Fact]
        public void Can_Get_Abbreviations_For_Central_Europe()
        {
            TimeZoneValues abbreviations = TZNames.GetAbbreviationsForTimeZone("Central European Standard Time", "en-US");

            Assert.Equal("CET", abbreviations.Generic);
            Assert.Equal("CET", abbreviations.Standard);
            Assert.Equal("CEST", abbreviations.Daylight);
        }

        [Fact]
        public void Can_Get_Names_For_IE()
        {
            TimeZoneValues names = TZNames.GetNamesForTimeZone("Europe/Dublin", "en-US");

            Assert.Equal("Ireland Time", names.Generic);
            Assert.Equal("Greenwich Mean Time", names.Standard);
            Assert.Equal("Irish Standard Time", names.Daylight);
        }

        [Fact]
        public void Can_Get_Abbreviations_For_IE()
        {
            TimeZoneValues abbreviations = TZNames.GetAbbreviationsForTimeZone("Europe/Dublin", "en-US");

            Assert.Null(abbreviations.Generic);
            Assert.Equal("GMT", abbreviations.Standard);
            Assert.Equal("IST", abbreviations.Daylight);
        }

        [Fact]
        public void Can_Get_Names_For_IN1()
        {
            TimeZoneValues names = TZNames.GetNamesForTimeZone("Asia/Calcutta", "en-US");

            Assert.Equal("India Standard Time", names.Generic);
            Assert.Equal("India Standard Time", names.Standard);
            Assert.Equal("India Standard Time", names.Daylight);
        }

        [Fact]
        public void Can_Get_Abbreviations_For_IN1()
        {
            TimeZoneValues abbreviations = TZNames.GetAbbreviationsForTimeZone("Asia/Calcutta", "en-US");

            Assert.Equal("IST", abbreviations.Generic);
            Assert.Equal("IST", abbreviations.Standard);
            Assert.Equal("IST", abbreviations.Daylight);
        }

        [Fact]
        public void Can_Get_Names_For_IN2()
        {
            TimeZoneValues names = TZNames.GetNamesForTimeZone("Asia/Kolkata", "en-US");

            Assert.Equal("India Standard Time", names.Generic);
            Assert.Equal("India Standard Time", names.Standard);
            Assert.Equal("India Standard Time", names.Daylight);
        }

        [Fact]
        public void Can_Get_Abbreviations_For_IN2()
        {
            TimeZoneValues abbreviations = TZNames.GetAbbreviationsForTimeZone("Asia/Kolkata", "en-US");

            Assert.Equal("IST", abbreviations.Generic);
            Assert.Equal("IST", abbreviations.Standard);
            Assert.Equal("IST", abbreviations.Daylight);
        }

        [Fact]
        public void Can_Get_Names_For_CN()
        {
            TimeZoneValues names = TZNames.GetNamesForTimeZone("Asia/Shanghai", "en-US");

            Assert.Equal("China Time", names.Generic);
            Assert.Equal("China Standard Time", names.Standard);
            Assert.Equal("China Daylight Time", names.Daylight);
        }

        //[Fact]
        //public void Can_Get_Names_For_CN2()
        //{
        //    var names = TZNames.GetNamesForTimeZone("Asia/Kashgar", "en-US");

        //    Assert.Equal("Urumqi Time", names.Generic);
        //    Assert.Equal("Urumqi Standard Time", names.Standard);
        //    Assert.Equal("Urumqi Daylight Time", names.Daylight);
        //}

        [Fact]
        public void Can_Get_Abbreviations_For_Sao_Tome()
        {
            TimeZoneValues abbreviations = TZNames.GetAbbreviationsForTimeZone("Sao Tome Standard Time", "en-GB");

            Assert.Equal("GMT", abbreviations.Generic);
            Assert.Equal("GMT", abbreviations.Standard);
            Assert.Equal("GMT", abbreviations.Daylight);
        }

        [Fact]
        public void Can_Get_Names_For_UTC()
        {
            TimeZoneValues names = TZNames.GetNamesForTimeZone("UTC", "en-US");

            Assert.Equal("Coordinated Universal Time", names.Generic);
            Assert.Equal("Coordinated Universal Time", names.Standard);
            Assert.Equal("Coordinated Universal Time", names.Daylight);
        }

        [Fact]
        public void Can_Get_Names_For_UTC_All_Langs()
        {
            ICollection<string> langs = TZNames.GetLanguageCodes();

            var ok = true;
            foreach (string lang in langs)
            {
                try
                {
                    string name = TZNames.GetNamesForTimeZone("UTC", lang).Generic;
                    _output.WriteLine($"{lang,-10} => {name}");
                }
                catch
                {
                    ok = false;
                    _output.WriteLine($"{lang,-10} => FAIL!!!!!!!!!!!!!!!");
                }
            }
            Assert.True(ok);
        }

        [Fact]
        public void Can_Get_Names_For_Windows_Timezone()
        {
            TimeZoneValues names = TZNames.GetNamesForTimeZone("Eastern Standard Time", "en-US");

            Assert.Equal("Eastern Time", names.Generic);
            Assert.Equal("Eastern Standard Time", names.Standard);
            Assert.Equal("Eastern Daylight Time", names.Daylight);
        }

        [Fact]
        public void Can_Get_Abbreviations_For_Windows_Timezone()
        {
            TimeZoneValues abbreviations = TZNames.GetAbbreviationsForTimeZone("AUS Eastern Standard Time", "en-US");

            Assert.Equal("AET", abbreviations.Generic);
            Assert.Equal("AEST", abbreviations.Standard);
            Assert.Equal("AEDT", abbreviations.Daylight);
        }

        //[Fact]
        //public void Can_Get_Names_For_CA_Pacific_From_MX()
        //{
        //    var names = TZNames.GetNamesForTimeZone("America/Vancouver", "en-MX");

        //    Assert.Equal("Pacific Time (Canada)", names.Generic);
        //    Assert.Equal("Pacific Standard Time (Canada)", names.Standard);
        //    Assert.Equal("Pacific Daylight Time (Canada)", names.Daylight);
        //}

        [Fact]
        public void Can_Get_English_Names_For_All_Windows_Timezones()
        {
            var errors = new List<string>();
            foreach (TimeZoneInfo timeZoneInfo in TimeZoneInfo.GetSystemTimeZones().OrderBy(x => x.Id))
            {
                try
                {
                    TimeZoneValues names = TZNames.GetNamesForTimeZone(timeZoneInfo.Id, "en-US");
                    _output.WriteLine("{0} = {1}", timeZoneInfo.Id, names.Generic);
                    if (string.IsNullOrWhiteSpace(names.Generic))
                        errors.Add(timeZoneInfo.Id);
                }
                catch
                {
                    errors.Add(timeZoneInfo.Id);
                }
            }

            if (errors.Count > 0)
            {
                _output.WriteLine("Could not get names for the following Windows time zone ids:\n");
            }

            foreach (string error in errors)
            {
                _output.WriteLine(error);
            }

            Assert.Empty(errors);
        }

        [Fact]
        public void Can_Get_English_Names_For_All_IANA_Timezones()
        {
            var errors = new List<string>();
            foreach (string tzid in DateTimeZoneProviders.Tzdb.Ids.OrderBy(x => x))
            {
                try
                {
                    TimeZoneValues names = TZNames.GetNamesForTimeZone(tzid, "en-US");
                    _output.WriteLine("{0} = {1}", tzid, names.Generic);
                    if (string.IsNullOrWhiteSpace(names.Generic))
                        errors.Add(tzid);
                }
                catch
                {
                    errors.Add(tzid);
                }
            }

            if (errors.Count > 0)
            {
                _output.WriteLine("Could not get names for the following IANA time zone ids:\n");
            }

            foreach (string error in errors)
            {
                _output.WriteLine(error);
            }

            Assert.Empty(errors);
        }

        [Fact]
        public void Can_Get_English_Names_For_Alias_Cuba()
        {
            TimeZoneValues namesForZone = TZNames.GetNamesForTimeZone("America/Havana", "en-US");
            TimeZoneValues namesForAlias = TZNames.GetNamesForTimeZone("Cuba", "en-US");

            Assert.Equal(namesForZone.Generic, namesForAlias.Generic);
            Assert.Equal(namesForZone.Standard, namesForAlias.Standard);
            Assert.Equal(namesForZone.Daylight, namesForAlias.Daylight);
        }
    }
}
