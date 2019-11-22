using Microsoft.AspNetCore.Mvc;

namespace Forest.Controllers
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