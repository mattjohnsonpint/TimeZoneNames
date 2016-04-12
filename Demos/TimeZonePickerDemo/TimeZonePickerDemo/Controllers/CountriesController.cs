using System.Globalization;
using System.Web.Http;
using TimeZoneNames;

namespace TimeZonePickerDemo.Controllers
{
    public class CountriesController : ApiController
    {
        public IHttpActionResult GetCountries()
        {
            var countries = TZNames.GetCountryNames(CultureInfo.CurrentUICulture.Name);

            return Ok(countries);
        }
    }
}
