using System;
using System.Reflection.Metadata;
using DataLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Rewrite.Internal.UrlActions;
using Monitor.ViewModels;

namespace Monitor.Controllers
{
    public class NewsCreatorController : Controller
    {
        private readonly ApplicationContext _dbContext;

        public NewsCreatorController(ApplicationContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(NewsModel model)
        {
            if (model.HtmlText == null)
            {
                ModelState.AddModelError("","Html текст пуст");
                return View();
            }
            
            
            if (model.Title == null)
            {
                ModelState.AddModelError("","Title текст пуст");
                return View();
            }

            
            PrimitiveNews news = new PrimitiveNews()
            {
                Title = model.Title,
                HtmlText = model.HtmlText,
                IsShown = false,
                DateTime = DateTime.UtcNow
            };

            _dbContext.PrimitiveNews.Add(news);
            _dbContext.SaveChanges();

            return RedirectToAction("Index", "Home");
           
        }
      
    }
}