using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TimeZoneNames;

namespace TimeZonePickerDemo.Controllers
{
    [Route("api/[controller]")]
    public class LanguagesController : Controller
    {
        [HttpGet]
        public IDictionary<string, string> Get()
        {
            var langCodes = TZNames.GetLanguageCodes()
                .Select(x => x.Replace("_", "-"))
                .ToHashSet();

            return CultureInfo.GetCultures(CultureTypes.NeutralCultures)
                .Where(x => langCodes.Contains(x.Name))
                .OrderBy(x => x.DisplayName)
                .ToDictionary(x => x.Name, x => x.DisplayName);
        }
    }
}
