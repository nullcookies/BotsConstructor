using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DataLayer.Models;

namespace DataLayer.Services
{
    public class StupidLogger
    {
        ApplicationContext _contextDb;
        ConcurrentQueue<LogMessage> logMessages;

        public StupidLogger(IConfiguration configuration)
        {
            logMessages = new ConcurrentQueue<LogMessage>();

            //Дублирование кода
            string connectionString;
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (isWindows)
                connectionString = configuration.GetConnectionString("PostgresConnectionDevelopment");
            else
                connectionString = configuration.GetConnectionString("PostgresConnectionLinux");                    

            _contextDb = new ApplicationContext(
                new DbContextOptionsBuilder<ApplicationContext>()
                .UseNpgsql(connectionString)
                .Options
            );

            PeriodicFooAsync(TimeSpan.FromSeconds(1), CancellationToken.None);
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
            Console.WriteLine("\n\n\n");
            Console.WriteLine("Сохранение " + logMessages.Count);
            Console.WriteLine("\n\n\n");

            if (logMessages.Count == 0)
                return;

            LogMessage[] _logMessages = new LogMessage[logMessages.Count];
            for (int i = 0; i < logMessages.Count; i++)
            {
                //Почему так не красиво?
                logMessages.TryDequeue(out _logMessages[i]);
            }

            _contextDb.LogMessages.AddRange(_logMessages);

            //await contextDb.SaveChangesAsync();
            _contextDb.SaveChanges();

        }


        public void Log(LogLevelMyDich logLevel,Source errorSource,  string comment = "", int accountId=default(int), Exception ex = null)
        {

            DateTime dt = DateTime.Now;
            LogMessage logRecord = new LogMessage()
            {
                DateTime = dt,
                LogLevel = logLevel,
                LogLevelString = logLevel.ToString(),
                Message = comment + " " + ex?.Message,
                Source = errorSource,
                SourceString =errorSource.ToString(),
                AccountId = accountId
            };

            logMessages.Enqueue(logRecord);

            Console.WriteLine();
            Console.WriteLine(logLevel.ToString()+"   "+ errorSource.ToString()+"   " + comment+" date="+dt);
            Console.WriteLine();

        }

    
    }

    public enum Source
    {
        WEBSITE,
        FOREST,
        MONITOR,
        OTHER
    }
}
