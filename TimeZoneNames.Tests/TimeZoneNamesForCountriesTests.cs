using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace TimeZoneNames.Tests
{
    public class TimeZoneNamesForCountriesTests
    {
        private readonly ITestOutputHelper _output;

        public TimeZoneNamesForCountriesTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Can_Get_Names_For_BR_English()
        {
            var zones = TimeZoneNames.GetTimeZonesForCountry("BR", "en-US");

            foreach (var zone in zones)
            {
                var tz = zone.Key;
                var names = zone.Value;

                _output.WriteLine(tz);
                _output.WriteLine("Generic  = " + names.Generic);
                _output.WriteLine("Standard = " + names.Standard);
                _output.WriteLine("Daylight = " + names.Daylight);
                _output.WriteLine("");
            }

            Assert.Equal(16, zones.Count);

            var expectedValues = new Dictionary<string, string[]>
            {
                {"America/Sao_Paulo", new[] {"Brasilia Time", "Brasilia Standard Time", "Brasilia Summer Time"}},
                {"America/Manaus", new[] {"Amazon Time", "Amazon Standard Time", "Amazon Summer Time"}},
                {"America/Rio_Branco", new[] {"Acre Time", "Acre Standard Time", "Acre Summer Time"}},
                {"America/Noronha", new[] {"Fernando de Noronha Time", "Fernando de Noronha Standard Time", "Fernando de Noronha Summer Time"}}
            };

            foreach (var expected in expectedValues)
            {
                var tz = expected.Key;
                Assert.True(zones.ContainsKey(tz), tz);
                Assert.Equal(expected.Value[0], zones[tz].Generic);
                Assert.Equal(expected.Value[1], zones[tz].Standard);
                Assert.Equal(expected.Value[2], zones[tz].Daylight);
            }
        }

        [Fact]
        public void Can_Get_Names_For_BR_Portuguese()
        {
            var zones = TimeZoneNames.GetTimeZonesForCountry("BR", "pt-BR");

            foreach (var zone in zones)
            {
                var tz = zone.Key;
                var names = zone.Value;

                _output.WriteLine(tz);
                _output.WriteLine("Generic  = " + names.Generic);
                _output.WriteLine("Standard = " + names.Standard);
                _output.WriteLine("Daylight = " + names.Daylight);
                _output.WriteLine("");
            }

            Assert.Equal(16, zones.Count);

            var expectedValues = new Dictionary<string, string[]>
            {
                {"America/Sao_Paulo", new[] {"Horário de Brasília", "Horário Padrão de Brasília", "Horário de Verão de Brasília"}},
                {"America/Manaus", new[] {"Horário do Amazonas", "Horário Padrão do Amazonas", "Horário de Verão do Amazonas"}},
                {"America/Rio_Branco", new[] {"Horário do Acre", "Horário Padrão do Acre", "Horário de Verão do Acre"}},
                {"America/Noronha", new[] {"Horário de Fernando de Noronha", "Horário Padrão de Fernando de Noronha", "Horário de Verão de Fernando de Noronha"}}
            };

            foreach (var expected in expectedValues)
            {
                var tz = expected.Key;
                Assert.True(zones.ContainsKey(tz), tz);
                Assert.Equal(expected.Value[0], zones[tz].Generic);
                Assert.Equal(expected.Value[1], zones[tz].Standard);
                Assert.Equal(expected.Value[2], zones[tz].Daylight);
            }
        }

        [Fact]
        public void Can_Get_Names_For_PG_English()
        {
            var zones = TimeZoneNames.GetTimeZonesForCountry("PG", "en-US");

            foreach (var zone in zones)
            {
                var tz = zone.Key;
                var names = zone.Value;

                _output.WriteLine(tz);
                _output.WriteLine("Generic  = " + names.Generic);
                _output.WriteLine("Standard = " + names.Standard);
                _output.WriteLine("Daylight = " + names.Daylight);
                _output.WriteLine("");
            }

            Assert.Equal(2, zones.Count);

            var expectedValues = new Dictionary<string, string[]>
            {
                {"Pacific/Port_Moresby", new[] {"Papua New Guinea Time", "Papua New Guinea Time", "Papua New Guinea Time"}},
                {"Pacific/Bougainville", new[] {"Papua New Guinea Time", "Papua New Guinea Time", "Papua New Guinea Time"}}
            };

            foreach (var expected in expectedValues)
            {
                var tz = expected.Key;
                Assert.True(zones.ContainsKey(tz), tz);
                Assert.Equal(expected.Value[0], zones[tz].Generic);
                Assert.Equal(expected.Value[1], zones[tz].Standard);
                Assert.Equal(expected.Value[2], zones[tz].Daylight);
            }
        }
    }
}
