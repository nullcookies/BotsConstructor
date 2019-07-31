using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using website.Models;

namespace website.Controllers
{
    
    public class Test3Controller : Controller
    {
        ApplicationContext context;

        public Test3Controller(ApplicationContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            try
            {
                return View();

            }
            catch
            {
                return NotFound();

            }

        }

       


   

    }
}
