
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite.Internal.UrlActions;
using Monitor.Models;

namespace Monitor.Controllers
{
    public class SignIn:Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
            if (model.Password == "taras")
            {
                Authenticate();
                return RedirectToAction("Index", "Home");
            }
            return Forbid();
        }
        
        private void Authenticate()
        {

            string userRoleName = "сука, работай";

            var claims = new List<Claim>
            {
                new Claim("userId", "идите в жопу"),
                new Claim(ClaimsIdentity.DefaultNameClaimType, "мразота")
            };

            
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
    }
}