﻿using System;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using DataLayer;
using MyLibrary;
using Newtonsoft.Json;


namespace Website.Controllers.Private_office.AccountManagement
{
    public class TopUpController:Controller
    {
        private readonly SimpleLogger _simpleLogger;
        string publicKey = "sandbox_i36414964362";
        string privateKey = "sandbox_dvNF6rSNhZy6kAIbCo8RXxQS1XPlEa7rnHs8pqPp";

        public TopUpController(SimpleLogger simpleLogger)
        {
            _simpleLogger = simpleLogger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            decimal hardcodedAmount = 1;
            int accountId = (int) HttpContext.Items["accountId"];
            var liqPayInfo = CalculateLiqPayInfo(hardcodedAmount, accountId);
            ViewData["data"] = liqPayInfo.Data;
            ViewData["signature"] = liqPayInfo.Signature;
            ViewData["amount"] = hardcodedAmount;
            return View();
        }

        [HttpPost]
        public IActionResult LiqPayCallback(string data, string signature)
        {
            var request = HttpContext.Request;
            var jsonString = JsonConvert.SerializeObject(request);

            _simpleLogger.Log(LogLevel.IMPORTANT_INFO, Source.WEBSITE_TOP_UP,
                $"Был получен post запрос на LiqPayCallback. Тело запроса:{jsonString}");
            
            string checkSignature = GetBase64EncodedSHA1Hash(privateKey + data + privateKey);
            if (checkSignature != signature)
            {
                //Запрос пришёл не от LiqPay
                //Идите в жопу
                _simpleLogger.Log(LogLevel.IMPORTANT_INFO, Source.WEBSITE_TOP_UP,
                    $"Сигнатуры не совпали checkSignature={checkSignature} signature={signature}");

                return Forbid();
            }

            _simpleLogger.Log(LogLevel.IMPORTANT_INFO, Source.WEBSITE_TOP_UP,
                $"Сигнатуры совпали {checkSignature}");

            return Ok();
        }
        
        [HttpGet]
        public IActionResult SuccessPayment()
        {
            var aaa = HttpContext;
            return Content("Це тотальна перемога");
        }
        class LiqPayInfo
        {
            public string Data;
            public string Signature;
        }

        LiqPayInfo CalculateLiqPayInfo(decimal amount, int accountId)
        {
            var jsonObj = new
            {
                version = "3",
                public_key  = publicKey,
                action = "pay",
                amount = amount.ToString(CultureInfo.InvariantCulture),
                currency = "USD",
                description = $"Пополнение личного счёта на сайте botsconstructor.com, accountId={accountId}",
                info= $"accountId={accountId}",
                order_id = Guid.NewGuid().ToString(),
                server_url = Url.Action("LiqPayCallback"),
                result_url = Url.Action("SuccessPayment")
            };
            string jsonString = JsonConvert.SerializeObject(jsonObj);
            string data = Base64Encode(jsonString);
            string signString = privateKey + data + privateKey;
            string signature = GetBase64EncodedSHA1Hash(signString);
            var result = new LiqPayInfo
            {
                Data = data,
                Signature = signature
            };
            return result;
        }
        
        static string GetBase64EncodedSHA1Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                return Convert.ToBase64String(sha1.ComputeHash(Encoding.ASCII.GetBytes(input)));
            }
        }
        private static string Base64Encode(string plainText) {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}