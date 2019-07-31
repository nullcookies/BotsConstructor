using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using website.Models;
using website.Other;

namespace website.Controllers
{
    [Authorize]
    public class SupportController : Controller
    {
        ApplicationContext context;
        
        public SupportController(ApplicationContext context, IHostingEnvironment _appEnvironment)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        
        }

        public IActionResult Index()
        {
            return View();
        }

    }
}
