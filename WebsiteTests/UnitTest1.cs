using System;
using System.Reflection;
using DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyLibrary;
using Website.Controllers.SignInUpOut;
using Website.Services;
using Website.ViewModels;

namespace WebsiteTests
{
    [TestClass]
    public class UnitTest1
    {
        private string conn =
            $"User ID = postgres;Password=3t0ssszheM3G4MMM0Ch~n`yparollb_wubfubrkmdbwiyro38;Server=54.209.245.213;Port=5432;Database=IntegrationTests {DateTime.UtcNow.ToShortDateString()} {DateTime.UtcNow.ToLongTimeString()};Integrated Security=true;Pooling=true;";
        
        
        [TestMethod]
        public void Registration()
        {
            ApplicationContext context = DbContextFactory.GetNewDbContext(conn);
            RegistrationService registrationService = new RegistrationService(context, new EmailMessageSenderMock(new SimpleLogger()));
            var model = new RegisterModel
            {
                Email = "starovoytov.ruslan@gmail.com",
                Name = "Ты",
                Password = "11qqQQ",
                ConfirmPassword = "11qqQQ"
            };

            var method = typeof(RegistrationService).GetMethod("Register", BindingFlags.NonPublic | BindingFlags.Instance,
                null, CallingConventions.Any,
                new []{typeof(RegisterModel), typeof(string), typeof(string).MakeByRefType()}, null);

            var arguments = new object[] {model, "domainqq", null};
            method.Invoke(registrationService, arguments);
            string link = (string)arguments[2];

            var (guid, accountId) = GetGuidFromLinkQuery(link);
            
            registrationService.ConfirmEmail(guid, accountId);
                
            var signInService = new SignInService(context);
            var loginModel = new LoginModel()
            {
                Email = model.Email,
                Password = model.Password
            };
            bool verificationPassed = signInService.IsVerificationPassed(loginModel, out Account account);
            
            Assert.IsTrue(verificationPassed);

        }

        private (Guid guid, int accId) GetGuidFromLinkQuery(string link)
        {
            int firstEqualIndex = link.IndexOf('=');
            int ampersandIndex = link.IndexOf('&');
            int lastEqualIndex = link.IndexOf('=', ampersandIndex);

            Guid guid = Guid.Parse(link.Substring(firstEqualIndex+1, 36));
            int accountId = int.Parse(link.Substring(lastEqualIndex+1));

            return (guid, accountId);
        }
    }
    
    
    
}