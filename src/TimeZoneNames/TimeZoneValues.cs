namespace TimeZoneNames;

/// <summary>
/// Represents a set of time zone name values.
/// </summary>
public class TimeZoneValues
{
    /// <summary>
    /// The time zone name that generically applies.
    /// </summary>
    public string Generic { get; set; }

    /// <summary>
    /// The time zone name that applies during standard time.
    /// </summary>
    public string Standard { get; set; }

    /// <summary>
    /// The time zone name that applies during daylight saving time.
    /// </summary>
    public string Daylight { get; set; }
}