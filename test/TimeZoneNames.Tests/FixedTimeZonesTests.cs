namespace TimeZoneNames.Tests;

[UsesVerify]
public class FixedTimeZonesTests
{
    [Fact]
    public Task CanGetFixedTimeZoneIds()
    {
        var ids = TZNames.GetFixedTimeZoneIds();
        return Verifier
            .Verify(ids)
#if DEBUG
            .AutoVerify()
#endif
            .UseDirectory(Path.Combine("Verify", nameof(FixedTimeZonesTests)));
    }
    
    [Fact]
    public Task CanGetFixedTimeZoneAbbreviations()
    {
        var abbreviations = TZNames.GetFixedTimeZoneAbbreviations();
        return Verifier
            .Verify(abbreviations)
#if DEBUG
            .AutoVerify()
#endif
            .UseDirectory(Path.Combine("Verify", nameof(FixedTimeZonesTests)))
            .DontSortDictionaries();
    }
    
    [Theory]
    [MemberData(nameof(TestData.GetLanguages), MemberType = typeof(TestData))]
    public Task CanGetFixedTimeZoneNames(string language)
    {
        var names = TZNames.GetFixedTimeZoneNames(language);
        return Verifier
            .Verify(names)
#if DEBUG
            .AutoVerify()
#endif
            .UseDirectory(Path.Combine("Verify", nameof(FixedTimeZonesTests), nameof(CanGetFixedTimeZoneNames)))
            .DontSortDictionaries()
            .UseParameters(language);
    }
}
