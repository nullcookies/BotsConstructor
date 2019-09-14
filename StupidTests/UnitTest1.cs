using DataLayer;
using DataLayer.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StupidTests
{
    [TestClass]
    public class UnitTest1
    {
        const string connectionString = "User ID = postgres;" +
               "Password=v3rRh4rdp455lidzomObCl4vui49ri4;" +
               "Server=194.9.71.76;" +
               "Port=5432;" +
               "Database=dev00087386873469827346r;" +
               "Integrated Security=true;" +
               "Pooling=true;";

        const int countOfRecord = 2000;
        [TestMethod]
        public void InitDb()
        {
            DbContextFactory dbContextWrapper = new DbContextFactory(connectionString);

            ApplicationContext contextDb = dbContextWrapper.GetNewDbContext();
            contextDb.LogMessages.RemoveRange(contextDb.LogMessages);
            contextDb.SaveChanges();
        }

        [TestMethod]
        public void DbStressTesting()
        {
            DbContextFactory dbContextWrapper = new DbContextFactory(connectionString);

            ApplicationContext contextDb = dbContextWrapper.GetNewDbContext();
            for (int i = 0; i < countOfRecord; i++)
            {
                contextDb.LogMessages.Add(
                    new LogMessage
                    {
                        AccountId = 000,
                        DateTime = DateTime.UtcNow,
                        LogLevel = LogLevelMyDich.INFO,
                        Message = "qq",
                        Source = DataLayer.Services.Source.OTHER,
                        SourceString = DataLayer.Services.Source.OTHER.ToString()
                    });
                contextDb.SaveChanges();
            }
            Assert.IsTrue(true);

        }

        [TestMethod]
        public async Task DbStressTestingTasks()
        {
            DbContextFactory dbContextWrapper = new DbContextFactory(connectionString);

            List<Task> tasks = new List<Task>();

            for (int i = 0; i < countOfRecord; i++)
            {
                tasks.Add(
                    Task.Run( 
                        ()=>WritetoDb(dbContextWrapper)
                        ));
            }

            await Task.WhenAll(tasks);

            Assert.IsTrue(true);

        }

        public async Task WritetoDb(DbContextFactory dbContextWrapper)
        {
            ApplicationContext contextDb = dbContextWrapper.GetNewDbContext();
            await contextDb.LogMessages.AddAsync(
                new LogMessage
                {
                    AccountId = 111,
                    DateTime = DateTime.UtcNow,
                    LogLevel = LogLevelMyDich.INFO,
                    Message = "qq",
                    Source = DataLayer.Services.Source.OTHER,
                    SourceString = DataLayer.Services.Source.OTHER.ToString()
                });
            await contextDb.SaveChangesAsync();
        }

    }
}
