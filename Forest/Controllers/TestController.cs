using Microsoft.AspNetCore.Mvc;

namespace Forest.Controllers
{
    public class TestController:Controller
    {
        public IActionResult Index()
        {
            return Content("Работает");
        }
    }
}