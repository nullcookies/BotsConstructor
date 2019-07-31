using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataLayer.Models;


namespace Website.Controllers
{
    
    public class Test2Controller : Controller
    {
        ApplicationContext context;

        public Test2Controller(ApplicationContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Index()
        {
           
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetLatestImage()
        {
            var byteArray = context.Images.Last().Photo;
            MemoryStream ms = new MemoryStream(byteArray);
            return new FileStreamResult(ms, "image/jpeg");

        }
    }
}
