using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using DataLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Website.Models;

namespace Website.Controllers
{
    
    public class TestController : Controller
    {
        ApplicationContext context;

        public TestController(ApplicationContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
