using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Website.Models;

namespace Website.Controllers
{
    [Authorize]
    public class NewsController : Controller
    {

        public NewsController(ApplicationContext context, IStringLocalizer<MainController> localizer)
        {
            
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            
            return View();
        }





    }
}
