using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Website.Models;

namespace Website.Controllers
{
    [Authorize]
    public class NewsController : Controller
    {
        private ApplicationContext _contextDb;
        public NewsController(ApplicationContext context,
            IStringLocalizer<MainController> localizer,
            ApplicationContext contextDb)
        {
            _contextDb = contextDb;
        }

        [AllowAnonymous]
        public IActionResult All(int pageNumber)
        {
            
            var newsDb = _contextDb.PrimitiveNews.TakeLast(7);
            var newsModel = new List<NewsPreviewModel>();
            foreach (var news in newsDb)
            {
                NewsPreviewModel model = new NewsPreviewModel()
                {
                    Date = news.DateTime.ToShortDateString(),
                    Title = news.Title,
                    BeginningOfText = GetFirstParagraph(news.HtmlText)
                };
                newsModel.Add(model);
            }
            ViewData["news"] = newsModel;
            
            return View();
        }

        private static string GetFirstParagraph(string htmlText)
        {
            return htmlText.Substring(0, Math.Min(htmlText.Length, 100));
        }

        [AllowAnonymous]
        public IActionResult News(int num)
        {
            Random random = new Random();
            int number = random.Next();
            if (number % 2 == 0)
            {
                return View();
            }
            return View("NewsSimple");
        }


    }
}
