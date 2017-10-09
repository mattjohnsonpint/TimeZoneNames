using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TimeZoneNames;

namespace TimeZonePickerDemo.Controllers
{
    [Route("api/[controller]")]
    public class TimeZonesController : Controller
    {
        [HttpGet]
        public IDictionary<string, string> Get(string country, DateTimeOffset? threshold)
        {
            var languageCode = CultureInfo.CurrentUICulture.Name;

            if (country != null)
            {
                return GetTimeZonesForCountry(country, threshold, languageCode);
            }

            return TZNames.GetCountryNames(languageCode)
                .SelectMany(x => GetTimeZonesForCountry(x.Key, threshold, languageCode)
                    .Select(y => new { CountryCode = x.Key, Country = x.Value, TimeZoneId = y.Key, TimeZoneName = y.Value }))
                .GroupBy(x => x.TimeZoneId)
                .ToDictionary(x => x.Key, x => $"{x.First().Country} - {x.First().TimeZoneName}");
        }

        private static IDictionary<string, string> GetTimeZonesForCountry(string country, DateTimeOffset? threshold, string languageCode)
        {
            return threshold == null
                ? TZNames.GetTimeZonesForCountry(country, languageCode)
                : TZNames.GetTimeZonesForCountry(country, languageCode, threshold.Value);
        }
    }
}
