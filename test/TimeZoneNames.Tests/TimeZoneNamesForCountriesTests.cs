using Xunit.Abstractions;

namespace TimeZoneNames.Tests;

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
        var zones = TZNames.GetTimeZonesForCountry("BR", "en-US");

        foreach ((var zoneName, var zoneDisplayName) in zones)
        {
            _output.WriteLine($"{zoneName} => {zoneDisplayName}");
        }

        //Assert.Equal(16, zones.Count);

        //var expectedValues = new Dictionary<string, string[]>
        //{
        //    {"America/Sao_Paulo", new[] {"Brasilia Time", "Brasilia Standard Time", "Brasilia Summer Time"}},
        //    {"America/Manaus", new[] {"Amazon Time", "Amazon Standard Time", "Amazon Summer Time"}},
        //    {"America/Rio_Branco", new[] {"Acre Time", "Acre Standard Time", "Acre Summer Time"}},
        //    {"America/Noronha", new[] {"Fernando de Noronha Time", "Fernando de Noronha Standard Time", "Fernando de Noronha Summer Time"}}
        //};

        //foreach (var expected in expectedValues)
        //{
        //    var tz = expected.Key;
        //    Assert.True(zones.ContainsKey(tz), tz);
        //    Assert.Equal(expected.Value[0], zones[tz].Generic);
        //    Assert.Equal(expected.Value[1], zones[tz].Standard);
        //    Assert.Equal(expected.Value[2], zones[tz].Daylight);
        //}
    }

    [Fact]
    public void Can_Get_Names_For_BR_Portuguese()
    {
        var zones = TZNames.GetTimeZonesForCountry("BR", "pt-BR");

        foreach ((var zoneName, var zoneDisplayName) in zones)
        {
            _output.WriteLine($"{zoneName} => {zoneDisplayName}");
        }

        //Assert.Equal(16, zones.Count);

        //var expectedValues = new Dictionary<string, string[]>
        //{
        //    {"America/Sao_Paulo", new[] {"Horário de Brasília", "Horário Padrão de Brasília", "Horário de Verão de Brasília"}},
        //    {"America/Manaus", new[] {"Horário do Amazonas", "Horário Padrão do Amazonas", "Horário de Verão do Amazonas"}},
        //    {"America/Rio_Branco", new[] {"Horário do Acre", "Horário Padrão do Acre", "Horário de Verão do Acre"}},
        //    {"America/Noronha", new[] {"Horário de Fernando de Noronha", "Horário Padrão de Fernando de Noronha", "Horário de Verão de Fernando de Noronha"}}
        //};

        //foreach (var expected in expectedValues)
        //{
        //    var tz = expected.Key;
        //    Assert.True(zones.ContainsKey(tz), tz);
        //    Assert.Equal(expected.Value[0], zones[tz].Generic);
        //    Assert.Equal(expected.Value[1], zones[tz].Standard);
        //    Assert.Equal(expected.Value[2], zones[tz].Daylight);
        //}
    }

    [Fact]
    public void Can_Get_Names_For_PG_English()
    {
        var zones = TZNames.GetTimeZonesForCountry("PG", "en-US");

        foreach ((var zoneName, var zoneDisplayName) in zones)
        {
            _output.WriteLine($"{zoneName} => {zoneDisplayName}");
        }

        //Assert.Equal(2, zones.Count);

        //var expectedValues = new Dictionary<string, string[]>
        //{
        //    {"Pacific/Port_Moresby", new[] {"Papua New Guinea Time", "Papua New Guinea Time", "Papua New Guinea Time"}},
        //    {"Pacific/Bougainville", new[] {"Papua New Guinea Time", "Papua New Guinea Time", "Papua New Guinea Time"}}
        //};

        //foreach (var expected in expectedValues)
        //{
        //    var tz = expected.Key;
        //    Assert.True(zones.ContainsKey(tz), tz);
        //    Assert.Equal(expected.Value[0], zones[tz].Generic);
        //    Assert.Equal(expected.Value[1], zones[tz].Standard);
        //    Assert.Equal(expected.Value[2], zones[tz].Daylight);
        //}
    }

    [Fact]
    public void Can_Get_Names_For_All_Regions()
    {
        foreach (var region in TestData.Regions)
        {
            try
            {
                var zones = TZNames.GetTimeZonesForCountry(region, "en");
                _output.WriteLine(region + ": " + zones.Count);
                Assert.NotEqual(0, zones.Count);
            }
            catch
            {
                _output.WriteLine(region + " -- FAILED");
                throw;
            }
        }
    }
}