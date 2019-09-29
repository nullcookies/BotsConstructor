using Microsoft.AspNetCore.Mvc;

namespace Forest.Controllers
{
    public class MonitorController:Controller
    {
        [HttpGet]
        public IActionResult AmAlive()
        {
            return Content("I am alive");
        }
    }
}