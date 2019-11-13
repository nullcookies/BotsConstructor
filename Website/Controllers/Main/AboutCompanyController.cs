using Microsoft.AspNetCore.Mvc;

namespace Website.Controllers.Main
{
    public class AboutCompanyController:Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}