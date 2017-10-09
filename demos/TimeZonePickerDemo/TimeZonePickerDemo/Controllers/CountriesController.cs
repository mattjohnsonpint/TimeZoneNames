using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using TimeZoneNames;

namespace TimeZonePickerDemo.Controllers
{
    [Route("api/[controller]")]
    public class CountriesController : Controller
    {
        [HttpGet]
        public IDictionary<string, string> Get()
        {
            return TZNames.GetCountryNames(CultureInfo.CurrentUICulture.Name);
        }
    }
}
