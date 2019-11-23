using System;
using Microsoft.AspNetCore.Mvc;

namespace Website.Controllers
{
    public class MonitorNegotiatorController:Controller
    {
        [HttpPost]
        public IActionResult Ping()
        {
            Console.WriteLine("Ping");
            return Ok();
        }
    }
}