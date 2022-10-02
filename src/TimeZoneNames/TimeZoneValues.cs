using System.Text.Json.Serialization;

namespace TimeZoneNames;

/// <summary>
/// Represents a set of time zone name values.
/// </summary>
public class TimeZoneValues
{
    /// <summary>
    /// The time zone name that generically applies.
    /// </summary>
    [JsonInclude]
    public string Generic { get; internal set; }

    /// <summary>
    /// The time zone name that applies during standard time.
    /// </summary>
    [JsonInclude]
    public string Standard { get; internal set; }

    /// <summary>
    /// The time zone name that applies during daylight saving time.
    /// </summary>
    [JsonInclude]
    public string Daylight { get; internal set; }
}