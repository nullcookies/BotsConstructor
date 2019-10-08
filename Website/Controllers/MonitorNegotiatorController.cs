using Microsoft.AspNetCore.Mvc;

namespace Website.Controllers
{
    public class MonitorNegotiatorController:Controller
    {
        [HttpPost]
        public IActionResult Ping()
        {
            return Ok();
        }
    }
}