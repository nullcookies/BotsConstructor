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
    public class BotForSalesEditingController : Controller
    {

        ApplicationContext context;
        IHostingEnvironment _appEnvironment;

        public BotForSalesEditingController(ApplicationContext context, IHostingEnvironment appEnvironment)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            _appEnvironment = appEnvironment;
        }


        [HttpPost]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult SetImage(IFormFile uploadedFile, int botId, long productId)
        {


            if (uploadedFile != null)
            {
                byte[] imageData = null;

                //считываем переданный файл в массив байтов
                using (var binaryReader = new BinaryReader(uploadedFile.OpenReadStream()))
                {
                    imageData = binaryReader.ReadBytes((int)uploadedFile.Length);
                }

                int count = context.Images.Count();

                //добавить или заменить
                var image = context.Images.Where(_im => _im.BotId == botId && _im.ProductId == productId).SingleOrDefault();
                if (image == null)
                {
                    context.Images.Add(new ImageMy() { BotId = botId, ProductId = productId, Name = "hardcode " + count, Photo = imageData });
                }
                else
                {
                    image.Photo = imageData;
                }

                context.SaveChanges();


            }

            JObject answer = new JObject
            {
                { "success", true }
            };

            return Json(answer);
        }

        [HttpGet]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult GetImage(int botId, long productId)
        {
            var image = context.Images.Where(_img => _img.BotId == botId && _img.ProductId == productId).SingleOrDefault();


            var byteArray = image?.Photo;
            if (byteArray == null)
            {
                return NotFound();
            }

            MemoryStream ms = new MemoryStream(byteArray);

            return Json(new { base64imgage = Convert.ToBase64String(byteArray) });



        }

        [HttpGet]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult GetImagePhoto(int botId, long productId)
        {


            var image = context.Images.Where(_img => _img.BotId == botId && _img.ProductId == productId).SingleOrDefault();


            var byteArray = image?.Photo;
            if (byteArray == null)
            {
                return NotFound();
            }

            MemoryStream ms = new MemoryStream(byteArray);

            return new FileStreamResult(ms, "image/jpeg");

        }


        [HttpGet]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult BotForSales(int botId)
        {


            ViewData["xVisible"] = 2;
            ViewData["yVisible"] = 2;

            //отправка пустой страницы
            return View();

        }


        [HttpGet]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult GetSalesBotMarkup(int botId)
        {


            //сформировать объект, который содержит всю информацию для фронта
            string jsonMarkup = context.Bots.Find(botId).Markup;
            if (string.IsNullOrEmpty(jsonMarkup))
            {
                return StatusCode(418);
            }

            return Content(jsonMarkup);
        }



        [HttpPost]
        [TypeFilter(typeof(CheckAccessToTheBot))]
        public IActionResult SaveSalesBotMarkup(int botId, string markup)
        {

            //TODO тут вставить фильтрацию адекватности содержимого

            //сохранение новой разметки в базу
            context.Bots.Find(botId).Markup = markup;
            context.SaveChanges();

            return Ok();
        }


    }
}