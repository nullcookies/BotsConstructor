﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using website.Models;

namespace website.Controllers
{
    [Authorize]
    public class NewsController : Controller
    {
        ApplicationContext context;
        private readonly IStringLocalizer<MainController> _localizer;

        public NewsController(ApplicationContext context, IStringLocalizer<MainController> localizer)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            
            return View();
        }





    }
}
