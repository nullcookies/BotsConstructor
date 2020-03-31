using System.Collections.Generic;
using System.Linq;
using DataLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyLibrary;
using Website.Services;

namespace WebsiteTest
{
    [TestClass]
    public class BotsAirstripServiceTest
    {
        readonly ApplicationContext dbContext;
        readonly BotsAirstripService botsAirstripService;
        
        public BotsAirstripServiceTest()
        {
            dbContext = InMemoryDatabaseFactory.Create();
            var logger = new StubLogger();
            var accessCheckService = new AccessCheckService(dbContext);
            var monitorNegotiatorService = new MonitorNegotiatorService();
            var forestNegotiatorService = new DichStubForestNegotiatorService(dbContext);

            botsAirstripService = new BotsAirstripService(logger, accessCheckService, 
                monitorNegotiatorService, forestNegotiatorService, dbContext);
        }
        
        [TestMethod]
        public void StartBot_NormalData_SuccessStart()
        {
            //Arrange
            Account account = new Account
            {
                Bots = new List<BotDB>
                {
                    new BotDB
                    {
                        Markup = "someMarkup",
                        Token = "someData"
                    }
                },
                Money = 1
            };
            dbContext.Accounts.Add(account);
            dbContext.SaveChanges();
            
            //Act
            BotStartMessage result = botsAirstripService.StartBot(account.Bots.First().Id, account.Id);
            
            //Assert
            Assert.IsTrue(result.Success);
            Assert.IsNull(result.FailureReason);
            Assert.IsNull(result.ForestException);
        }

        [TestMethod]
        public void StartBot_StartBotWithoutToken_TokenMissing()
        {
            //Arrange
            Account account = new Account
            {
                Bots = new List<BotDB>
                {
                    new BotDB
                    {
                        Markup = "ss",
                        Token = null
                    }
                },
                Money = 1
            };
            dbContext.Accounts.Add(account);
            dbContext.SaveChanges();
            
            //Act
            BotStartMessage result = botsAirstripService.StartBot(account.Bots.First().Id, account.Id);
            
            //Assert
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.FailureReason);
            Assert.AreEqual(BotStartFailureReason.TokenMissing, result.FailureReason);
        }
        
        [TestMethod]
        public void StartBot_StartBotWithoutMarkup_NoMarkupData()
        {
            //Arrange
            Account account = new Account
            {
                Bots = new List<BotDB>
                {
                    new BotDB
                    {
                        Markup = null,
                        Token = "someToken"
                    }
                },
                Money = 1
            };
            dbContext.Accounts.Add(account);
            dbContext.SaveChanges();
            
            //Act
            BotStartMessage result = botsAirstripService.StartBot(account.Bots.First().Id, account.Id);
            
            //Assert
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.FailureReason);
            Assert.AreEqual(BotStartFailureReason.NoMarkupData, result.FailureReason);
        }
        
        [TestMethod]
        public void StartBot_StartBotWithWrongBotId_BotWithSuchIdDoesNotExist()
        {
            //Arrange
            Account account = new Account
            {
                Bots = new List<BotDB>
                {
                    new BotDB
                    {
                        Markup = "someMarkup",
                        Token = "someToken"
                    }
                },
                Money = 1
            };
            dbContext.Accounts.Add(account);
            dbContext.SaveChanges();
            
            //Act
            BotStartMessage result = botsAirstripService.StartBot(int.MaxValue, account.Id);
            
            //Assert
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.FailureReason);
            Assert.AreEqual(BotStartFailureReason.BotWithSuchIdDoesNotExist, result.FailureReason);
        }
        
        [TestMethod]
        public void StartBot_StartBotWithWrongAccountId_NoAccessToThisBot()
        {
            //Arrange
            Account account = new Account
            {
                Bots = new List<BotDB>
                {
                    new BotDB
                    {
                        Markup = "someMarkup",
                        Token = "someToken"
                    }
                },
                Money = 1
            };
            dbContext.Accounts.Add(account);
            dbContext.SaveChanges();
            
            //Act
            BotStartMessage result = botsAirstripService.StartBot(account.Bots.First().Id, int.MaxValue);
            
            //Assert
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.FailureReason);
            Assert.AreEqual(BotStartFailureReason.NoAccessToThisBot, result.FailureReason);
        }

        [TestMethod]
        public void StartBot_AttemptToRunBotThatIsAlreadyRunning_ThisBotIsAlreadyRunning()
        {
            //Arrange
            Account account = new Account
            {
                Bots = new List<BotDB>
                {
                    new BotDB
                    {
                        Markup = "someMarkup",
                        Token = "someToken"
                    }
                },
                Money = 1
            };
            dbContext.Accounts.Add(account);
            dbContext.SaveChanges();
            RouteRecord routeRecord = new RouteRecord
            {
                BotId = account.Bots.First().Id
            };
            dbContext.RouteRecords.Add(routeRecord);
            dbContext.SaveChanges();
            
            //Act
            BotStartMessage result = botsAirstripService.StartBot(account.Bots.First().Id, account.Id);
            
            //Assert
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.FailureReason);
            Assert.AreEqual(BotStartFailureReason.ThisBotIsAlreadyRunning, result.FailureReason);
        }
        
        [TestMethod]
        public void StartBot_BotLaunchWithoutMoney_NotEnoughFundsInTheAccountOfTheBotOwner()
        {
            //Arrange
            Account account = new Account
            {
                Money = 0,
                Bots = new List<BotDB>
                {
                    new BotDB
                    {
                        Markup = "someMarkup",
                        Token = "someToken"
                    }
                }
            };
            dbContext.Accounts.Add(account);
            dbContext.SaveChanges();
       
            
            //Act
            BotStartMessage result = botsAirstripService.StartBot(account.Bots.First().Id, account.Id);
            
            //Assert
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.FailureReason);
            Assert.AreEqual(BotStartFailureReason.NotEnoughFundsInTheAccountOfTheBotOwner, result.FailureReason);
        }
        
        
        
        [TestMethod]
        public void StopBot_NormalData_SuccessStop()
        {
            //Arrange
            Account account = new Account
            {
                Bots = new List<BotDB>
                {
                    new BotDB
                    {
                        Markup = "someMarkup",
                        Token = "someData"
                    }
                }
            };
            RouteRecord routeRecord = new RouteRecord
            {
                Bot = account.Bots.First()
            };
            dbContext.Accounts.Add(account);
            dbContext.RouteRecords.Add(routeRecord);
            dbContext.SaveChanges();
            
            //Act
            var result = botsAirstripService.StopBot(account.Bots.First().Id, account.Id);
            
            //Assert
            Assert.IsTrue(result.Success);
            Assert.IsNull(result.FailureReason);
            Assert.IsNull(result.ForestException);
        }
        
        [TestMethod]
        public void StopBot_StartBotWithWrongBotId_BotWithSuchIdDoesNotExist()
        {
            //Arrange
            Account account = new Account
            {
                Bots = new List<BotDB>
                {
                    new BotDB
                    {
                        Markup = "someMarkup",
                        Token = "someData"
                    }
                }
            };
            RouteRecord routeRecord = new RouteRecord
            {
                Bot = account.Bots.First()
            };
            dbContext.Accounts.Add(account);
            dbContext.RouteRecords.Add(routeRecord);
            dbContext.SaveChanges();
            
            //Act
            var result = botsAirstripService.StopBot(int.MaxValue, account.Id);
            
            //Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(BotStopFailureReason.BotWithSuchIdDoesNotExist,result.FailureReason);
            Assert.IsNull(result.ForestException);
        }
        
        [TestMethod]
        public void StopBot_StartBotWithWrongAccountId_NoAccessToThisBot()
        {
            //Arrange
            Account account = new Account
            {
                Bots = new List<BotDB>
                {
                    new BotDB
                    {
                        Markup = "someMarkup",
                        Token = "someData"
                    }
                }
            };
            RouteRecord routeRecord = new RouteRecord
            {
                Bot = account.Bots.First()
            };
            dbContext.Accounts.Add(account);
            dbContext.RouteRecords.Add(routeRecord);
            dbContext.SaveChanges();
            
            //Act
            var result = botsAirstripService.StopBot(account.Bots.First().Id, int.MaxValue);
            
            //Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(BotStopFailureReason.NoAccessToThisBot,result.FailureReason);
            Assert.IsNull(result.ForestException);
        }
        
        [TestMethod]
        public void StopBot_StartAlreadyStoppedBot_ThisBotIsAlreadyStopped()
        {
            //Arrange
            Account account = new Account
            {
                Bots = new List<BotDB>
                {
                    new BotDB
                    {
                        Markup = "someMarkup",
                        Token = "someData"
                    }
                }
            };
            dbContext.Accounts.Add(account);
            dbContext.SaveChanges();
            
            //Act
            var result = botsAirstripService.StopBot(account.Bots.First().Id, account.Id);
            
            //Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(BotStopFailureReason.ThisBotIsAlreadyStopped,result.FailureReason);
            Assert.IsNull(result.ForestException);
        }
    }
}