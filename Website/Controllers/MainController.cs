using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using DataLayer.Models;

namespace Website.Controllers
{
    [Authorize]
    public class MainController : Controller
    {
        ApplicationContext context;
        private readonly IStringLocalizer<MainController> _localizer;

        public MainController(ApplicationContext context, IStringLocalizer<MainController> localizer)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            _localizer = localizer;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            var www = HttpContext.Request.Headers ;
            
            //Console.WriteLine($"CurrentCulture:{CultureInfo.CurrentCulture.Name}, CurrentUICulture:{CultureInfo.CurrentUICulture.Name}");
            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        



    }
}
