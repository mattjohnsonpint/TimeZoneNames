using System.Globalization;
using System.Linq;
using System.Web.Http;
using TimeZoneNames;

namespace TimeZonePickerDemo.Controllers
{
    public class LanguagesController : ApiController
    {
        public IHttpActionResult GetLanguages()
        {
            var langCodes = TZNames.GetLanguageCodes().Select(x => x.Replace("_", "-"));
            var cultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures)
                .Where(x => langCodes.Contains(x.Name))
                .OrderBy(x => x.DisplayName);
            var results = cultures.ToDictionary(x => x.Name, x => x.DisplayName);

            return Ok(results);
        }
    }
}
