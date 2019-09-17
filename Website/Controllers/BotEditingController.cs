using System;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Website.Models;
using System.Collections.Generic;
using Website.Other;

using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Hosting;
using Website.Other.Filters;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using DataLayer.Models;

namespace Website.Controllers
{
    [Authorize]
    public class BotEditingController : Controller
    {
        ApplicationContext context;
        IHostingEnvironment _appEnvironment;

        public BotEditingController(ApplicationContext context, IHostingEnvironment appEnvironment)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            _appEnvironment = appEnvironment;
        }


    }
}
