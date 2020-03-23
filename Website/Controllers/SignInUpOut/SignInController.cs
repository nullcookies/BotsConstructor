using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DataLayer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Website.Services;
using Website.ViewModels;

namespace Website.Controllers.SignInUpOut
{
    public class SignInController : Controller
    {
        private readonly ApplicationContext context;
        private readonly AccountRegistrationService registrationService;
        
        public SignInController(ApplicationContext context, AccountRegistrationService registrationService)
        {
            this.context = context;
            this.registrationService = registrationService;
        }
        
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("MyBots", "MyBots");
            }
            else
            {
                return View();
            }
        }


        [HttpGet]
        public async Task<IActionResult> LoginWithTelegram(string id,
            [FromQuery(Name = "first_name")] string firstName,
            [FromQuery(Name = "last_name")]  string lastName,
            [FromQuery(Name = "username")]   string username,
            [FromQuery(Name = "photo_url")]  string photoUrl,
            [FromQuery(Name = "auth_date")]  string authDate,
            [FromQuery(Name = "hash")]       string hash)
        {
            //@bots_constructor_bot
            string botToken = "913688656:AAH5l-ZAOmpI5MDqcq3ye1M1CF8ESF-Bms8";

            List<string> myList = new List<string>
            {
                $"id={id}",
                $"auth_date={authDate}"
            };

            //Эти поля могут быть пустыми
            if (firstName != null) myList.Add($"first_name={firstName}");
            if (lastName != null) myList.Add($"last_name={lastName}");
            if (username != null) myList.Add($"username={username}");
            if (photoUrl != null) myList.Add($"photo_url={photoUrl}");

            string[] myArr = myList.ToArray();
            Array.Sort(myArr);
            string dataCheckString = string.Join("\n", myArr);
            
            var authorizationIsValid = CheckTelegramAuthorization(hash, botToken, dataCheckString);

            if (authorizationIsValid)
            {
                int.TryParse(id, out int telegramId);

                var account = context.TelegramLoginInfo
                    .Include(info => info.Account)
                    .SingleOrDefault(_acc => _acc.TelegramId == telegramId)
                    ?.Account;
                
                
                if (account == null)
                {
                    string name = firstName + " " + lastName;
                    TelegramLoginInfo telegramLoginInfo = new TelegramLoginInfo
                    {
                        TelegramId = telegramId
                    };

                    account = await registrationService.RegisterAccountAsync(name, telegramLoginInfo);
                }


                if (account != null)
                {
                    Authenticate(account);
                    return RedirectToAction("MyBots", "MyBots");
                }
                else
                {
                    return RedirectToAction("Failure", "StaticMessage", new {message = "Что-то пошло не так(("});
                }
                
                //TODO Отправить сообщение о авторизации пользователю (приветствие)
            }

            ModelState.AddModelError("", "Ошибка авторизации");
            return RedirectToAction("Login", "SignIn");
        }

        private static bool CheckTelegramAuthorization(string hash, string botToken, string dataCheckString)
        {
            using (SHA256 mySha256 = SHA256.Create())
            {
                byte[] botTokenByteArr = Encoding.UTF8.GetBytes(botToken);
                byte[] secretKey = mySha256.ComputeHash(botTokenByteArr);
                byte[] allUSerData = Encoding.UTF8.GetBytes(dataCheckString);

                using (HMACSHA256 hmac = new HMACSHA256(secretKey))
                {
                    byte[] myValueByteArr = hmac.ComputeHash(allUSerData);
                    string calculatedHashString = BitConverter.ToString(myValueByteArr)
                        .Replace("-", string.Empty);

                    if (hash == calculatedHashString.ToLower()) return true;
                }
            }
            return false;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(EmailPasswordLoginModel model)
        {
            if (ModelState.IsValid)
            {
                var account = context.EmailLoginInfo
                    .Include(info => info.Account)
                    .FirstOrDefault(a => a.Email == model.Email && a.Password == model.Password)
                    ?.Account;

                if (account != null)
                {
                    Authenticate(account);
                    return RedirectToAction("MyBots", "MyBots");
                }

                ModelState.AddModelError("", "Некорректные логин и(или) пароль ");
            }

            return View(model);
        }
        
      
        
        [HttpGet]
        public async Task<IActionResult> ChangeAccount()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            return RedirectToAction("Login", "SignIn");
        }
        
        private void Authenticate(Account user)
        {
            var claims = new List<Claim>
            {
                new Claim("userId", user.Id.ToString()),
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Name)
            };

            
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id), new AuthenticationProperties()
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            });
        }
    }
}