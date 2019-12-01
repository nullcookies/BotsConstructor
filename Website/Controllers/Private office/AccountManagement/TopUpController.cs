using System;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using DataLayer;
using Microsoft.AspNetCore.Authorization;
using MyLibrary;
using Newtonsoft.Json;


namespace Website.Controllers.Private_office.AccountManagement
{
    [Authorize]
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

        [AllowAnonymous]
        [HttpPost]
        public IActionResult LiqPayCallback(string data, string signature, decimal amount)
        {
            _simpleLogger.Log(LogLevel.IMPORTANT_INFO, Source.WEBSITE_TOP_UP,
                $"Был получен post запрос на LiqPayCallback. Запрос : data = {data}, signature={signature}, amount = {amount}");
            
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
      
        class LiqPayInfo
        {
            public string Data;
            public string Signature;
        }

        [HttpPost]
        public IActionResult SucceddPayment()
        {
            return RedirectToAction("Success", "StaticMessage",new {message="Ваш платёж был успешно принят"});
        }
        
        LiqPayInfo CalculateLiqPayInfo(decimal amount, int accountId)
        {
            string domain = "botsconstructor.com";
            string link = $"https://{domain}/";
            string serverUrl = link + "TopUp/LiqPayCallback";
            string resultUrl = link + "TopUp/SucceddPayment";
            
            _simpleLogger.Log(LogLevel.IMPORTANT_INFO, Source.WEBSITE_TOP_UP, $"Формирование data и signature. serverUrl={serverUrl} resultUrl ={resultUrl}");
            
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
                server_url = serverUrl,
                result_url = resultUrl
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