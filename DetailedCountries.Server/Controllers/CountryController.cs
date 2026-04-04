using Microsoft.AspNetCore.Mvc;

namespace DetailedCountries.Server.Controllers
{
    public class CountryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
