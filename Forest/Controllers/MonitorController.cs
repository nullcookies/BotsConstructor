﻿using Microsoft.AspNetCore.Mvc;

namespace Forest.Controllers
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