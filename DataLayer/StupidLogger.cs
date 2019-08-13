using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DataLayer.Models;
using System.Collections.Generic;

namespace DataLayer.Services
{
    public class StupidLogger
    {      

        DbContextWrapper _dbContextWrapper;
        ConcurrentQueue<LogMessage> logMessages;

        public StupidLogger(IConfiguration configuration)
        {
            logMessages = new ConcurrentQueue<LogMessage>();
            _dbContextWrapper = new DbContextWrapper(configuration);

#pragma warning disable CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до завершения вызова
            PeriodicFooAsync(TimeSpan.FromSeconds(1), CancellationToken.None);
#pragma warning restore CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до завершения вызова
        }

        private async Task PeriodicFooAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            while (true)
            {
                SaveLogsToDb();
                await Task.Delay(interval, cancellationToken);
            }
        }

        private void SaveLogsToDb()
        {
            ApplicationContext _contextDb = _dbContextWrapper.GetNewDbContext();

            if (!logMessages.IsEmpty)
            {
                int numberOfMessages = logMessages.Count;
                List<LogMessage> _logMessages = new List<LogMessage>();

                for (int i = 0; i < numberOfMessages; i++)
                {                    
                    bool successfully = logMessages.TryDequeue(out LogMessage mes);

                    if (successfully)
                    {
                        _logMessages.Add(mes);
                    }
                    
                }

                _contextDb.LogMessages.AddRange(_logMessages);
                _contextDb.SaveChanges();
            }
        }

        public void Log(LogLevelMyDich logLevel, 
            Source errorSource, 
            string comment = "", 
            int accountId = default(int), 
            Exception ex = null)
        {

            DateTime dt = DateTime.Now;

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

    public enum Source
    {
        WEBSITE,
        FOREST,
        MONITOR,
        OTHER,
        MONEY_COLLECTOR_SERVICE,
        BOTS_AIRSTRIP_SERVICE
    }

}
