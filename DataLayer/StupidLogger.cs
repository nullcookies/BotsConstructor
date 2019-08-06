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
        ApplicationContext contextDb;
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

            contextDb = new ApplicationContext(
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
                await SaveLogsToDb();
                await Task.Delay(interval, cancellationToken);
            }
        }

        private async Task SaveLogsToDb()
        {
            if (logMessages.Count == 0)
                return;

            LogMessage[] _logMessages = new LogMessage[logMessages.Count];
            for (int i = 0; i < logMessages.Count; i++)
            {
                //Почему так не красиво?
                logMessages.TryDequeue(out _logMessages[i]);
            }

            contextDb.LogMessages.AddRange(_logMessages);

            //await contextDb.SaveChangesAsync();
            contextDb.SaveChanges();

        }


        public void Log(LogLevelMyDich logLevel,ErrorSource errorSource,  string comment = "", Exception ex = null)
        {
            Console.WriteLine();
            Console.WriteLine(logLevel.ToString()+"   "+ errorSource.ToString()+"   " + comment);
            Console.WriteLine();

            LogMessage logRecord = new LogMessage()
            {
                DateTime = DateTime.Now,
                LogLevel = logLevel,
                LogLevelString = logLevel.ToString(),
                Message = comment + " " + ex?.Message,
                ErrorSource = errorSource,
                ErrorSourceString =errorSource.ToString()
            };

            logMessages.Enqueue(logRecord);
        }

    
    }



    public enum ErrorSource
    {
        WEBSITE,
        FOREST,
        MONITOR,
        OTHER
    }
}
