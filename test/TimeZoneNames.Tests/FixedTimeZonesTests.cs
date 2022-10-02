using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace TimeZoneNames.Tests;

[UsesVerify]
public class FixedTimeZonesTests
{
    [Fact]
    public Task CanGetFixedTimeZoneIds()
    {
        var ids = TZNames.GetFixedTimeZoneIds();
        return Verifier.Verify(ids);
    }
    
    [Fact]
    public Task CanGetFixedTimeZoneAbbreviations()
    {
        var abbreviations = TZNames.GetFixedTimeZoneAbbreviations();
        return Verifier.Verify(abbreviations);
    }
    
    [Theory]
    [MemberData(nameof(TestData.GetLanguages), MemberType = typeof(TestData))]
    public Task CanGetFixedTimeZoneNames(string language)
    {
        var names = TZNames.GetFixedTimeZoneNames(language);
        return Verifier.Verify(names).UseParameters(language);
    }
}