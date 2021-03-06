﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataLayer;
using Monitor.Models;
using MyLibrary;

namespace Monitor.Services
{
    public class DiagnosticService
    {
        private bool _isWorking;
        private readonly SimpleLogger _logger;
        
        private ConcurrentDictionary<string,UrlStatistics> _targetUrlsStatistics 
            = new ConcurrentDictionary<string, UrlStatistics>();
        
        public DiagnosticService(SimpleLogger logger)
        {
            _logger = logger;
            
        }
        public bool TryAddUrl(string url, ref string errorMessage)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                errorMessage = "Строка не является ссылкой";
                return false;
            }

            if (_targetUrlsStatistics.TryAdd(url, new UrlStatistics(url)))
            {
                return true;
            }
            errorMessage = "Такая ссылка уже есть";
            return false;
        }

        public bool TryRemoveUrl(string url, ref string errorMessage)
        {
            if (_targetUrlsStatistics.TryRemove(url, out UrlStatistics value))
            {
                return true;
            }
            else
            {
                errorMessage = "Не удалось достать из словаря";
                return false;
            }
        }
        public async void StartPingAsync(int delaySec = 1)
        {
            _logger.Log(LogLevel.INFO,Source.MONITOR,"Старт сервиса диагностики");
            _isWorking = true;
            while (true)
            {
                if(!_isWorking)
                    break;
                
                foreach (var urlStatistics in _targetUrlsStatistics)
                {
                    try
                    {
                        Ping(urlStatistics.Key);
                        await WriteToDatabase(urlStatistics.Key, true);
                    }
                    catch (Exception exception)
                    {
                        urlStatistics.Value.FailedCheckDateTimes.Add(DateTime.UtcNow);
                        await WriteToDatabase(urlStatistics.Key, false, exception.Message);
                    }
                }
                await Task.Delay(1000 * delaySec);
            }
        }

        private async Task WriteToDatabase(string url, bool success, string errorMessage=null)
        {
            var dbContext = new DbContextFactory().CreateDbContext();
            var record = new PingRecord
            {
                DateTime = DateTime.UtcNow,
                IsOk = success,
                Url = url,
                Description = errorMessage
            };
            await dbContext.PingRecords.AddAsync(record);
            await dbContext.SaveChangesAsync();
        }

     
        
        public void StopPing()
        {
            _isWorking = false;
        }
        
        private void Ping(string url)
        {
            HttpClientWrapper.SendPostAsync(url).Wait();
        }


        public UrlStatistics[] GetTargetsStatistics()
        {
            UrlStatistics[] targetUrlsCopy=new UrlStatistics[_targetUrlsStatistics.Count];
            _targetUrlsStatistics.Values.CopyTo(targetUrlsCopy,0);
            return targetUrlsCopy;
        }
    }

    public class UrlStatistics
    {
        public readonly string Url;
        public readonly List<DateTime> FailedCheckDateTimes=new List<DateTime>();

        public UrlStatistics(string url)
        {
            Url = url;
        }
    }
}