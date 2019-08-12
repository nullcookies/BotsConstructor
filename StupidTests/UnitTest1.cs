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
               "Database=dev0008r;" +
               "Integrated Security=true;" +
               "Pooling=true;";

        [TestMethod]
        public void DbStressTestingAdequately()
        {
            DbContextWrapper dbContextWrapper = new DbContextWrapper(connectionString);

            ApplicationContext contextDb = dbContextWrapper.GetNewDbContext();
            for (int i = 0; i < 300; i++)
            {
                contextDb.LogMessages.Add(
                    new LogMessage
                    {
                        AccountId = 000,
                        DateTime = DateTime.Now,
                        LogLevel = LogLevelMyDich.INFO,
                        Message = "qq",
                        Source = DataLayer.Services.Source.OTHER,
                        SourceString = DataLayer.Services.Source.OTHER.ToString()
                    });
            }
            contextDb.SaveChanges();
            Assert.IsTrue(true);

        }


        //Медленно
        [TestMethod]
        public void DbStressTesting()
        {
            DbContextWrapper dbContextWrapper = new DbContextWrapper(connectionString);

            ApplicationContext contextDb = dbContextWrapper.GetNewDbContext();
            for (int i = 0; i < 300; i++)
            {

                contextDb.LogMessages.Add(
                    new LogMessage
                    {
                        AccountId = 111,
                        DateTime = DateTime.Now,
                        LogLevel = LogLevelMyDich.INFO,
                        Message = "qq",
                        Source = DataLayer.Services.Source.OTHER,
                        SourceString = DataLayer.Services.Source.OTHER.ToString()
                    });
                contextDb.SaveChanges();
            }
            Assert.IsTrue(true);

        }

        //Потеря данных
        [TestMethod]
        public async Task DbStressTestingAsync()
        {
            DbContextWrapper dbContextWrapper = new DbContextWrapper(connectionString);

            ApplicationContext contextDb = dbContextWrapper.GetNewDbContext();

            object lock_obj = new object();
            for (int i = 0; i < 300; i++)
            {

                await contextDb.LogMessages.AddAsync(
                        new LogMessage
                    {
                        AccountId = 222,
                        DateTime = DateTime.Now,
                        LogLevel = LogLevelMyDich.INFO,
                        Message = "qq",
                        Source = DataLayer.Services.Source.OTHER,
                        SourceString = DataLayer.Services.Source.OTHER.ToString()
                    });
                lock (lock_obj)
                {
                    contextDb.SaveChangesAsync();
                }
            }
            Assert.IsTrue(true);
        }


        //Медленно
        [TestMethod]
        public async Task DbStressTestingAsync1()
        {
            DbContextWrapper dbContextWrapper = new DbContextWrapper(connectionString);

            ApplicationContext contextDb = dbContextWrapper.GetNewDbContext();

            for (int i = 0; i < 300; i++)
            {

                await contextDb.LogMessages.AddAsync(
                    new LogMessage
                    {
                        AccountId = 333,
                        DateTime = DateTime.Now,
                        LogLevel = LogLevelMyDich.INFO,
                        Message = "qq",
                        Source = DataLayer.Services.Source.OTHER,
                        SourceString = DataLayer.Services.Source.OTHER.ToString()
                    });
                
                await contextDb.SaveChangesAsync();
                
            }
            Assert.IsTrue(true);
        }

        //медленно + потеря данных
        [TestMethod]
        public void DbStressTesting1()
        {
            DbContextWrapper dbContextWrapper = new DbContextWrapper(connectionString);

            ApplicationContext contextDb = dbContextWrapper.GetNewDbContext();

            object lock_obj = new object();
            for (int i = 0; i < 300; i++)
            {
                lock(lock_obj)
                {
                 contextDb.LogMessages.AddAsync(
                        new LogMessage
                        {
                            AccountId = 444,
                            DateTime = DateTime.Now,
                            LogLevel = LogLevelMyDich.INFO,
                            Message = "qq",
                            Source = DataLayer.Services.Source.OTHER,
                            SourceString = DataLayer.Services.Source.OTHER.ToString()
                        });
                
                contextDb.SaveChangesAsync();
                
                }
            }

            
            Assert.IsTrue(true);
        }


    }
}
