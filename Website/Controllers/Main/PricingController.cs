using Microsoft.AspNetCore.Mvc;

namespace Website.Controllers.Main
{
    public class PricingController:Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}