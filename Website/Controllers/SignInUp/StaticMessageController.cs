using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Website.Controllers.SignInUp
{
    public class StaticMessageController : Controller
    {
                
        [HttpGet]
        public IActionResult Success(string message)
        {
            ViewData["message"] = message;
            return View();

        }

    }
}