using System.Globalization;
using System.Runtime.InteropServices;
using TimeZoneConverter;
using Xunit.Abstractions;

namespace TimeZoneNames.Tests;

[UsesVerify]
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

        foreach (var (key, value) in displayNames)
        {
            _output.WriteLine($"{key} = {value}");
        }

        Assert.NotEmpty(displayNames);
    }

    [SkippableFact]
    public void Can_Get_DisplayNames_For_OS_Culture()
    {
        // This test requires Windows
        Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
        
        // The data has an update for Jordan Standard Time that might not yet be installed
        Skip.If(!TimeZoneInfo.FindSystemTimeZoneById("Jordan Standard Time").DisplayName.Contains("+03:00"));
        
        var languageCode = CultureInfo.InstalledUICulture.IetfLanguageTag;

        var displayNames = TZNames.GetDisplayNames(languageCode);

        string[] obsoleteZones =
        {
            "Mid-Atlantic Standard Time",
            "Kamchatka Standard Time"
        };

        var expected = TimeZoneInfo.GetSystemTimeZones()
            .Where(tzi => !obsoleteZones.Contains(tzi.Id))
            .Select(tzi => new KeyValuePair<string, string>(tzi.Id, tzi.DisplayName))
            .ToList();

        foreach (var item in expected)
        {
            if (displayNames[item.Key] != item.Value)
            {
                _output.WriteLine($"{item.Key} = \"{displayNames[item.Key]}\" expected \"{item.Value}\"");
            }
        }

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
        var errors = new List<string>();
        foreach (var id in TZConvert.KnownWindowsTimeZoneIds)
        {
            var displayName = TZNames.GetDisplayNameForTimeZone(id, "en-AU");
            if (string.IsNullOrEmpty(displayName))
            {
                errors.Add(id);
            }
        }

        Assert.Empty(errors);
    }

    [Fact]
    public void Can_Get_DisplayName_For_Every_Mappable_IANA_Zone()
    {
        string[] unmappableZones = { "Antarctica/Troll" };

        var errors = new List<string>();
        foreach (var id in TZConvert.KnownIanaTimeZoneNames.Except(unmappableZones))
        {
            var displayName = TZNames.GetDisplayNameForTimeZone(id, "en");
            if (string.IsNullOrEmpty(displayName))
            {
                errors.Add(id);
            }
        }

        Assert.Empty(errors);
    }

    [Fact]
    public void Invalid_Zones_Return_Null()
    {
        var displayName = TZNames.GetDisplayNameForTimeZone("invalid zone", "en");
        Assert.Null(displayName);
    }

    [Fact]
    public void Can_Get_DisplayName_For_Yukon_Standard_Time()
    {
        var displayName = TZNames.GetDisplayNameForTimeZone("Yukon Standard Time", "en");
        Assert.Equal("(UTC-07:00) Yukon", displayName);
    }

    [Theory]
    [MemberData(nameof(TestData.GetDisplayNameLanguages), MemberType = typeof(TestData))]
    public Task CanGetDisplayNames_WindowsZones(string language)
    {
        var displayNames = TZNames.GetDisplayNames(language);
        return Verifier
            .Verify(displayNames)
#if DEBUG
            .AutoVerify()
#endif
            .UseDirectory(Path.Combine("Verify", nameof(DisplayNamesTests), nameof(CanGetDisplayNames_WindowsZones)))
            .UseParameters(language)
            .DontSortDictionaries();
    }
    
    [Theory]
    [MemberData(nameof(TestData.GetDisplayNameLanguages), MemberType = typeof(TestData))]
    public Task CanGetDisplayNames_IanaZones(string language)
    {
        var displayNames = TZNames.GetDisplayNames(language, useIanaZoneIds: true);
        return Verifier
            .Verify(displayNames)
#if DEBUG
            .AutoVerify()
#endif
            .UseDirectory(Path.Combine("Verify", nameof(DisplayNamesTests), nameof(CanGetDisplayNames_IanaZones)))
            .UseParameters(language)
            .DontSortDictionaries();
    }
}