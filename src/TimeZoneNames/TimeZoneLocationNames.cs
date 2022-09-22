namespace TimeZoneNames
{
    /// <summary>
    /// Represents the names of the time zone location (country and city).
    /// </summary>
    public class TimeZoneLocationNames
    {
        /// <summary>
        /// The name(s) of the representative countries.
        /// 
        /// Note: this may be an empty array but will never be null.
        /// </summary>
        public string[] Countries { get; set; }

        /// <summary>
        /// The name of the representative city.
        ///
        /// This is guaranteed not to be `null`, though the response for some
        /// time zones ("Etc/*" for example) may not actually be a city name.
        /// </summary>
        public string City { get; set; }
    }
}
