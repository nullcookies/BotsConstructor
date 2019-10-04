using Microsoft.AspNetCore.Mvc;

namespace Website.Controllers
{
    public class MonitorController:Controller
    {
        [HttpPost]
        public IActionResult Ping()
        {
            return Ok();
        }
    }
}