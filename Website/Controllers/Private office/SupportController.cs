﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Website.Models;
using Website.Other;

namespace Website.Controllers
{
    [Authorize]
    public class SupportController : Controller
    {
     

        public IActionResult Index()
        {
            return View();
        }

    }
}
