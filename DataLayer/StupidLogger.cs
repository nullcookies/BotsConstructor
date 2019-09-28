using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataLayer.Models;
using Microsoft.Extensions.Configuration;

//TODO уничтожить это нахрен

namespace DataLayer
{
    public class StupidLogger
    {
        readonly DbContextFactory _dbContextWrapper;
        readonly ConcurrentQueue<LogMessage> _logMessages;
        readonly ConcurrentQueue<SpyRecord> _spyMessages;

        public StupidLogger(IConfiguration configuration)
        {
            _logMessages = new ConcurrentQueue<LogMessage>();
            _spyMessages = new ConcurrentQueue<SpyRecord>();
            _dbContextWrapper = new DbContextFactory(configuration);

#pragma warning disable CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до завершения вызова
            PeriodicFooAsync(TimeSpan.FromSeconds(1), CancellationToken.None);
#pragma warning restore CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до завершения вызова
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


            _spyMessages.Enqueue(spyRecord);
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
            ApplicationContext contextDb = _dbContextWrapper.GetNewDbContext();

            if (!_logMessages.IsEmpty)
            {
                int numberOfMessages = this._logMessages.Count;
                List<LogMessage> logMessages = new List<LogMessage>();

                for (int i = 0; i < numberOfMessages; i++)
                {                    
                    bool successfully = this._logMessages.TryDequeue(out LogMessage mes);

                    if (successfully)
                    {
                        logMessages.Add(mes);
                    }
                    
                }
                contextDb.LogMessages.AddRange(logMessages);

                int numberOfSpyMessages = this._spyMessages.Count;
                List<SpyRecord> spyMessages = new List<SpyRecord>();

                for (int i = 0; i < numberOfSpyMessages; i++)
                {
                    bool successfully = this._spyMessages.TryDequeue(out SpyRecord mes);

                    if (successfully)
                    {
                        spyMessages.Add(mes);
                    }

                }

                contextDb.SpyRecords.AddRange(spyMessages);
                contextDb.SaveChanges();
            }
        }

        public void Log(LogLevelMyDich logLevel, 
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

            _logMessages.Enqueue(logRecord);

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
        WEBSITE_BOTS_AIRSTRIP_SERVICE,
        FOREST_BANNED_USERS_SYNCHRONIZER,
        FOREST_BOT_STATISTICS_SYNCHRONIZER,
        PASSWORD_RESET
    }

}
