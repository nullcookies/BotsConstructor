using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeleteMeWebhook.Controllers
{
    public class ForestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

    }
}