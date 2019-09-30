//using System;
//using System.Collections.Generic;
//using System.Linq;
//using DataLayer.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Rewrite.Internal.UrlActions;
//using Microsoft.Extensions.Localization;
//using Website.Models;
//
//namespace Website.Controllers.Main
//{
//    [Authorize]
//    public class NewsController : Controller
//    {
//        private ApplicationContext _contextDb;
//        public NewsController(ApplicationContext context,
//            IStringLocalizer<MainController> localizer,
//            ApplicationContext contextDb)
//        {
//            _contextDb = contextDb;
//        }
//
//        //Постранично показывает новости 
//        [AllowAnonymous]
//        public IActionResult All(int pageNumber)
//        {
//
//            var newsDb = _contextDb.PrimitiveNews.ToArray();
//            
//            var newsModel = new List<NewsPreviewModel>();
//            foreach (var news in newsDb)
//            {
//                NewsPreviewModel model = new NewsPreviewModel()
//                {
//                    Id = news.Id,
//                    Date = news.DateTime.ToShortDateString(),
//                    Title = news.Title,
//                    BeginningOfText = GetFirstParagraph(news.HtmlText)
//                };
//                newsModel.Add(model);
//            }
//            ViewData["news"] = newsModel;
//            
//            return View();
//        }
//
//        private static string GetFirstParagraph(string htmlText)
//        {
//            return htmlText.Substring(0, Math.Min(htmlText.Length, 100));
//        }
//
//        //Показывает новость на отдельной странице
//        [AllowAnonymous]
//        public IActionResult News(int id)
//        {
////            Random random = new Random();
////            int number = random.Next();
////            if (number % 2 == 0)
////            {
////                return View();
////            }
//
//            var newsDb = _contextDb.PrimitiveNews.SingleOrDefault(_news => _news.Id == id);
//
//            if (newsDb != null)
//            {
//                ViewData["date"] = newsDb.DateTime;
//                ViewData["title"] = newsDb.Title;
//                ViewData["htmlText"] = newsDb.HtmlText;
//            }
//            else
//            {
//                return RedirectToAction("Failure", "StaticMessage", 
//                    new {message="Не удалось загрузить новость"});
//            }
//
//            return View("NewsSimple");
//        }
//
//
//    }
//}
