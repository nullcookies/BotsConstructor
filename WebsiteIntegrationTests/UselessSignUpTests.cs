using System;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Website.Services;

namespace WebsiteIntegrationTests
{
    [TestClass]
    public class SignUpTests
    {
        ApplicationContext dbContext;
        [TestInitialize]
        public void Init()
        {
            dbContext = DbContextFactory.CreateTestDbContext(nameof(ConstraintsTests));
        }
        
        [TestCleanup]
        public async Task CleanUp()
        {
            var applicationContext = DbContextFactory.CreateTestDbContext(nameof(ConstraintsTests));
            applicationContext.Accounts.Clear();
            await applicationContext.SaveChangesAsync();
        }
        
        [TestMethod]
        public async Task TestMethod_SignUpWithEmail_SuccessRegistration()
        {
            //Arrange
            AccountRegistrationService registrationService = new AccountRegistrationService(dbContext);
            EmailLoginInfo emailLoginInfo = new EmailLoginInfo
            {
                Email = "kashdvbkjsahdvb",
                Password = "extraStrongPass"
            };
            string name = "Muhammad";
            
            //Act
            await registrationService.RegisterAccountAsync(name, emailLoginInfo);

            //Assert
            var emailLoginDb = await dbContext.EmailLoginInfo
                .Include(info => info.Account)
                .ThenInclude(account1 => account1.OrderStatusGroups)
                .Where(loginInfo => loginInfo.Email == emailLoginInfo.Email)
                .SingleAsync();
            var accountDb = emailLoginDb.Account;
            
            Assert.AreEqual(name, accountDb.Name);
            Assert.AreEqual(emailLoginInfo.Email, accountDb.EmailLoginInfo.Email);
            Assert.AreEqual(emailLoginInfo.Password, accountDb.EmailLoginInfo.Password);
            Assert.IsTrue(accountDb.OrderStatusGroups.Count > 0);
        }
        
        [TestMethod]
        public async Task TestMethod_SignUpWithTelegram_SuccessRegistration()
        {
            //Arrange
            AccountRegistrationService registrationService = new AccountRegistrationService(dbContext);
            TelegramLoginInfo telegramLoginInfo = new TelegramLoginInfo
            {
                TelegramId = Int32.MaxValue/2
            };
            string name = "Muhammad";
            
            //Act
            await registrationService.RegisterAccountAsync(name, telegramLoginInfo);

            //Assert
            var telegramLoginInfoDb = await dbContext.TelegramLoginInfo
                .Where(info => info.TelegramId == telegramLoginInfo.TelegramId)
                .Include(info => info.Account)
                .SingleAsync();
            var account = telegramLoginInfo.Account;
            
            Assert.AreEqual(name, account.Name);
            Assert.AreEqual(telegramLoginInfo.TelegramId, account.TelegramLoginInfo.TelegramId);
            Assert.IsTrue(account.OrderStatusGroups.Count > 0);
        }
    }
}