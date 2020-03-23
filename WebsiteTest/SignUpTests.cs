using System;
using System.Linq;
using DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Website.Services;

namespace WebsiteTest
{
    [TestClass]
    public class SignUpTests
    {
        [TestMethod]
        public void TestMethod_SignUpWithEmail_SuccessRegistration()
        {
            //Arrange
            var dbContext = InMemoryDatabaseFactory.Create();
            AccountRegistrationService registrationService = new AccountRegistrationService(dbContext);
            EmailLoginInfo emailLoginInfo = new EmailLoginInfo
            {
                Email = "suka@mail.com",
                Password = "extraStrongPass"
            };
            string name = "Muhammad";
            
            //Act
            registrationService.RegisterAccountAsync(name, emailLoginInfo).Wait();

            //Assert
            var account = dbContext.Accounts
                .Include(account1 => account1.EmailLoginInfo)
                .Include(account1 => account1.OrderStatusGroups)
                .First();
            
            Assert.AreEqual(name, account.Name);
            Assert.AreEqual(emailLoginInfo.Email, account.EmailLoginInfo.Email);
            Assert.AreEqual(emailLoginInfo.Password, account.EmailLoginInfo.Password);
            Assert.IsTrue(account.OrderStatusGroups.Count > 0);
        }
        
        [TestMethod]
        public void TestMethod_SignUpWithTelegram_SuccessRegistration()
        {
            //Arrange
            var dbContext = InMemoryDatabaseFactory.Create();
            AccountRegistrationService registrationService = new AccountRegistrationService(dbContext);
            TelegramLoginInfo telegramLoginInfo = new TelegramLoginInfo
            {
                TelegramId = Int32.MaxValue/2
            };
            string name = "Muhammad";
            
            //Act
            registrationService.RegisterAccountAsync(name, telegramLoginInfo).Wait();

            //Assert
            var account = dbContext.Accounts
                .Include(account1 => account1.TelegramLoginInfo)
                .Include(account1 => account1.OrderStatusGroups)
                .First();
            
            Assert.AreEqual(name, account.Name);
            Assert.AreEqual(telegramLoginInfo.TelegramId, account.TelegramLoginInfo.TelegramId);
            Assert.IsTrue(account.OrderStatusGroups.Count > 0);
        }
        
        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void TestMethod_SignUpWithNullEmailInfo_RegistrationFailure()
        {
            //Arrange
            var dbContext = InMemoryDatabaseFactory.Create();
            AccountRegistrationService registrationService = new AccountRegistrationService(dbContext);
            string name = "Muhammad";
            
            //Act
            registrationService.RegisterAccountAsync(name, (EmailLoginInfo) null).Wait();
        }
        
        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void TestMethod_SignUpWithNullTelegramInfo_RegistrationFailure()
        {
            //Arrange
            var dbContext = InMemoryDatabaseFactory.Create();
            AccountRegistrationService registrationService = new AccountRegistrationService(dbContext);
            string name = "Muhammad";
            
            //Act
            registrationService.RegisterAccountAsync(name, (TelegramLoginInfo) null).Wait();
        }
        
        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void TestMethod_SignUpWithNullName_RegistrationFailure()
        {
            //Arrange
            var dbContext = InMemoryDatabaseFactory.Create();
            AccountRegistrationService registrationService = new AccountRegistrationService(dbContext);
            TelegramLoginInfo telegramLoginInfo = new TelegramLoginInfo
            {
                TelegramId = Int32.MaxValue/2
            };
            
            //Act
            registrationService.RegisterAccountAsync(null, telegramLoginInfo).Wait();
        }
    }
}