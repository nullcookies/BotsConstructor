using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataLayer;
using Microsoft.Extensions.Configuration;

//TODO уничтожить это нахрен

namespace MyLibrary
{
    public class SimpleLogger
    {
        readonly DbContextFactory dbContextFactory;
        readonly ConcurrentQueue<LogMessage> logMessages;
        readonly ConcurrentQueue<SpyRecord> spyMessages;

        public SimpleLogger()
        {
            logMessages = new ConcurrentQueue<LogMessage>();
            spyMessages = new ConcurrentQueue<SpyRecord>();
            dbContextFactory = new DbContextFactory();

            PeriodicFooAsync(TimeSpan.FromSeconds(1), CancellationToken.None);
        }

        public void LogSpyRecord(string pathCurrent, string pathFrom, int accountId)
        {

            DateTime dt = DateTime.UtcNow;

            SpyRecord spyRecord = new SpyRecord()
            {
                Time= dt,
                PathCurrent=pathCurrent,
                PathFrom=pathFrom,
                AccountId= accountId
            };


            spyMessages.Enqueue(spyRecord);
        }

        private async void PeriodicFooAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            while (true)
            {
                SaveLogsToDb();
                await Task.Delay(interval, cancellationToken);
            }
        }

        private void SaveLogsToDb()
        {
            ApplicationContext contextDb = dbContextFactory.GetNewDbContext();

            if (!logMessages.IsEmpty)
            {
                int numberOfMessages = this.logMessages.Count;
                List<LogMessage> logMessages = new List<LogMessage>();

                for (int i = 0; i < numberOfMessages; i++)
                {                    
                    bool successfully = this.logMessages.TryDequeue(out LogMessage mes);

                    if (successfully)
                    {
                        logMessages.Add(mes);
                    }
                    
                }
                contextDb.LogMessages.AddRange(logMessages);

                int numberOfSpyMessages = this.spyMessages.Count;
                List<SpyRecord> spyMessages = new List<SpyRecord>();

                for (int i = 0; i < numberOfSpyMessages; i++)
                {
                    bool successfully = this.spyMessages.TryDequeue(out SpyRecord mes);

                    if (successfully)
                    {
                        spyMessages.Add(mes);
                    }

                }

                contextDb.SpyRecords.AddRange(spyMessages);
                contextDb.SaveChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="errorSource"></param>
        /// <param name="comment"></param>
        /// <param name="accountId"></param>
        /// <param name="ex"></param>
        public void Log(LogLevel logLevel, 
            Source errorSource, 
            string comment = "", 
            int accountId = default(int), 
            Exception ex = null)
        {

            DateTime dt = DateTime.UtcNow;

            LogMessage logRecord = new LogMessage()
            {
                DateTime = dt,
                LogLevel = logLevel,
                LogLevelString = logLevel.ToString(),
                Message = comment + " " + ex?.Message,
                Source = errorSource,
                SourceString = errorSource.ToString(),
                AccountId = accountId
            };

            logMessages.Enqueue(logRecord);

            Console.WriteLine();
            Console.WriteLine(logLevel.ToString() + "   " + errorSource.ToString() + "   " + comment + " date=" + dt);
            Console.WriteLine();

        }
    }

    public static class LoggerSingelton
    {
        private static SimpleLogger _instance;
        public static SimpleLogger GetLogger()
        {
            if (_instance == null)
                _instance = new SimpleLogger();;
            

            return _instance;
        }
    }

  

}
