using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebsiteIntegrationTests
{
    [TestClass]
    public class ConstraintsTests
    {
        [TestCleanup]
        public async Task CleanUp()
        {
            var dbContext = DbContextFactory.CreateTestDbContext(nameof(ConstraintsTests));
            dbContext.Accounts.Clear();
            await dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Нормальный аккаунт пишется в БД без проблем
        /// </summary>
        /// <returns></returns>
        [DataRow("test@mail.com", "0132qdv", "Muhammad")]
        [DataRow("3423423@mail.ru", "asasdvsd", "Igor")]
        [TestMethod]
        public async Task AddingAnAccount_Ok(string email, string pass, string name)
        {
            //Arrange
            var dbContext = DbContextFactory.CreateTestDbContext(nameof(ConstraintsTests));
            Account account = new Account
            {
                EmailLoginInfo = new EmailLoginInfo
                {
                    Email = email,
                    Password = pass
                },
                Name = name
            };
            
            //Act
            await dbContext.Accounts.AddAsync(account);
            await dbContext.SaveChangesAsync();

            //Assert
            var emailLoginDb = await dbContext.EmailLoginInfo
                .Include(info => info.Account)
                    .ThenInclude(account1 => account1.OrderStatusGroups)
                .Where(loginInfo => loginInfo.Email == account.EmailLoginInfo.Email)
                .SingleAsync();
            var accountDb = emailLoginDb.Account;
            
            Assert.AreEqual(name, accountDb.Name);
            Assert.AreEqual(account.EmailLoginInfo.Email, accountDb.EmailLoginInfo.Email);
            Assert.AreEqual(account.EmailLoginInfo.Password, accountDb.EmailLoginInfo.Password);
        }
        
        /// <summary>
        /// Длинный пароль/email/имя не запишется
        /// </summary>
        /// <returns></returns>
        [ExpectedException(typeof(DbUpdateException))]
        [DataRow("012345678901234567890123456789012345678901234567890123456789", "pass", "Muhammad")]
        [DataRow("email", "012345678901234567890123456789012345678901234567890123456789", "Muhammad")]
        [DataRow("email", "pass", "012345678901234567890123456789012345678901234567890123456789")]
        [TestMethod]
        public async Task AddingTooLongString_Fail(string email, string pass, string name)
        {
            //Arrange
            var dbContext = DbContextFactory.CreateTestDbContext(nameof(ConstraintsTests));
            Account account = new Account
            {
                EmailLoginInfo = new EmailLoginInfo
                {
                    Email = email,
                    Password = pass
                },
                Name = name
            };
            
            //Act
            await dbContext.Accounts.AddAsync(account);
            await dbContext.SaveChangesAsync();

            //Assert
            Assert.Fail();
        }
        
           
        /// <summary>
        /// null для пароль/email/имя не запишется
        /// </summary>
        /// <returns></returns>
        [ExpectedException(typeof(DbUpdateException))]
        [DataRow(null, "pass", "Muhammad")]
        [DataRow("email", null, "Muhammad")]
        [DataRow("email", "pass", null)]
        [TestMethod]
        public async Task AddingNullValue_Fail(string email, string pass, string name)
        {
            //Arrange
            var dbContext = DbContextFactory.CreateTestDbContext(nameof(ConstraintsTests));
            Account account = new Account
            {
                EmailLoginInfo = new EmailLoginInfo
                {
                    Email = email,
                    Password = pass
                },
                Name = name
            };
            
            //Act
            await dbContext.Accounts.AddAsync(account);
            await dbContext.SaveChangesAsync();

            //Assert
            Assert.Fail();
        }
        
        /// <summary>
        /// Email уникален
        /// </summary>
        /// <returns></returns>
        [ExpectedException(typeof(DbUpdateException))]
        [TestMethod]
        public async Task AddingEmailDuplicate_Fail()
        {
            //Arrange
            string email = "someString";
            var dbContext = DbContextFactory.CreateTestDbContext(nameof(ConstraintsTests));
            Account account1 = new Account
            {
                EmailLoginInfo = new EmailLoginInfo
                {
                    Email = email,
                    Password = "s"
                },
                Name = "s"
            };
            Account account2 = new Account
            {
                EmailLoginInfo = new EmailLoginInfo
                {
                    Email = email,
                    Password = "s"
                },
                Name = "s"
            };
            
            //Act
            await dbContext.Accounts.AddAsync(account1);
            await dbContext.Accounts.AddAsync(account2);
            await dbContext.SaveChangesAsync();

            //Assert
            Assert.Fail();
        }
        
        /// <summary>
        /// TelegramId уникален
        /// </summary>
        /// <returns></returns>
        [ExpectedException(typeof(DbUpdateException))]
        [TestMethod]
        public async Task AddingTelegramIdDuplicate_Fail()
        {
            //Arrange
            int telegramId = 4654;
            var dbContext = DbContextFactory.CreateTestDbContext(nameof(ConstraintsTests));
            Account account1 = new Account
            {
               TelegramLoginInfo = new TelegramLoginInfo()
               {
                   TelegramId = telegramId
               },
                Name = "s"
            };
            Account account2 = new Account
            {
                TelegramLoginInfo = new TelegramLoginInfo()
                {
                    TelegramId = telegramId
                },
                Name = "s"
            };
            
            //Act
            await dbContext.Accounts.AddAsync(account1);
            await dbContext.Accounts.AddAsync(account2);
            await dbContext.SaveChangesAsync();

            //Assert
            Assert.Fail();
        }
    }
}