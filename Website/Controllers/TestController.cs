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
using Telegram.Bot;
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
            //Отправить сообщение с захардкоженым текстом от бота
            //

            string token = "724246784:AAHLOtr3Vz_q0Cf5iQvuY_bf-kVm0s-JAMU";
            new TelegramBotClient(token).SendTextMessageAsync(440090552, "🚚 Ваш заказ в пути 🚚");
            return View();
        }
    }
}
