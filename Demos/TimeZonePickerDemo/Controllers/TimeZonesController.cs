using System;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using TimeZoneNames;

namespace TimeZonePickerDemo.Controllers
{
    public class TimeZonesController : ApiController
    {
        public IHttpActionResult GetTimeZones()
        {
            var language = CultureInfo.CurrentUICulture.Name;
            var zones = TZNames.GetCountryNames(language)
                .SelectMany(x => TZNames.GetTimeZonesForCountry(x.Key, language)
                    .Select(y => new { CountryCode = x.Key, Country = x.Value, TimeZoneId = y.Key, TimeZoneName = y.Value }))
                .GroupBy(x => x.TimeZoneId)
                .ToDictionary(x => x.Key, x => $"{x.First().Country} - {x.First().TimeZoneName}");

            return Ok(zones);
        }

        public IHttpActionResult GetTimeZones(DateTimeOffset threshold)
        {
            var language = CultureInfo.CurrentUICulture.Name;
            var zones = TZNames.GetCountryNames(language)
                .SelectMany(x => TZNames.GetTimeZonesForCountry(x.Key, language, threshold)
                    .Select(y => new { CountryCode = x.Key, Country = x.Value, TimeZoneId = y.Key, TimeZoneName = y.Value }))
                .GroupBy(x => x.TimeZoneId)
                .ToDictionary(x => x.Key, x => $"{x.First().Country} - {x.First().TimeZoneName}");

            return Ok(zones);
        }

        public IHttpActionResult GetTimeZones(string country)
        {
            var zones = TZNames.GetTimeZonesForCountry(country, CultureInfo.CurrentUICulture.Name);

            return Ok(zones);
        }

        public IHttpActionResult GetTimeZones(string country, DateTimeOffset threshold)
        {
            var zones = TZNames.GetTimeZonesForCountry(country, CultureInfo.CurrentUICulture.Name, threshold);

            return Ok(zones);
        }
    }
}
