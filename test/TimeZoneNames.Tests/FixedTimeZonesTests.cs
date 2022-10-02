using System.Collections.Generic;
using System.Linq;
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
        return Verifier.Verify(ids).AutoVerify();
    }
    
    [Fact]
    public Task CanGetFixedTimeZoneAbbreviations()
    {
        var ids = TZNames.GetFixedTimeZoneAbbreviations();
        return Verifier.Verify(ids).AutoVerify();
    }
    
    [Theory]
    [MemberData(nameof(GetLanguages))]
    public Task CanGetFixedTimeZoneNames(string language)
    {
        var ids = TZNames.GetFixedTimeZoneNames(language);
        return Verifier.Verify(ids).UseParameters(language).AutoVerify();
    }

    public static IEnumerable<object[]> GetLanguages() => 
        TZNames.GetLanguageCodes().Select(language => new object[] {language});
}