using DataLayer.Models;
using DataLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Website.Services.Bookkeeper
{
    public class StupidBotForSalesBookkeeper
    {
        ApplicationContext _contextDb;
        StupidLogger _logger;

        public StupidBotForSalesBookkeeper(ApplicationContext context, StupidLogger _logger )
        {
            _contextDb = context;
            this._logger = _logger;
        }

        public StupidPriceInfo GetPriceInfo(int botId)
        {

            DateTime week_ago = DateTime.Now.AddDays(-7);

            int number_of_orders_over_the_past_week = _contextDb
                .Orders
                .Where(_or => _or.BotId == botId && _or.DateTime >= week_ago).Count();

            DateTime today_00_00 = DateTime.Now
                .AddHours(-DateTime.Now.Hour)
                .AddMinutes(-DateTime.Now.Minute)
                .AddSeconds(-DateTime.Now.Second);

            int answersCountToday = _contextDb
                .Orders
                .Where(_or => _or.BotId == botId && _or.DateTime > today_00_00).Count();

            BotForSalesPrice price = _contextDb.BotForSalesPrices.LastOrDefault();
            StupidPriceInfo priceInfo = null;

            if (price != null)
            {
                priceInfo = new StupidPriceInfo(
                    answersCountToday,
                    number_of_orders_over_the_past_week,
                    price.MaxPrice,
                    price.MinPrice,
                    price.DailyPrice,
                    price.MagicParameter);

            }
            else
            {
                _logger.Log(LogLevelMyDich.FATAL, Source.WEBSITE, $"В базе нет ни одной записи о цене. botId={botId}");

                priceInfo = new StupidPriceInfo(
                   0,
                   0,
                   0,
                   0,
                   0,
                   1);

            }

            return priceInfo;

        }

       
    }
  
    public class StupidPriceInfo
    {
        public readonly int AnswersCountToday;
        public readonly decimal SumToday;
        public readonly int Number_of_orders_over_the_past_week;
        public readonly decimal OneAnswerPrice;
        public readonly decimal DailyConst;

        public StupidPriceInfo(
            int answersCountToday, 
            int number_of_orders_over_the_past_week,
            decimal max, 
            decimal min, 
            decimal dailyConst,
            decimal magiсParameter)
        {
            AnswersCountToday = answersCountToday;
            Number_of_orders_over_the_past_week = number_of_orders_over_the_past_week;
            DailyConst = dailyConst;

            // 2 + (0;1]
            
            OneAnswerPrice = min + ((max - min) - (Number_of_orders_over_the_past_week) / (Number_of_orders_over_the_past_week + magiсParameter));

            //Показ двух знаков после запятой
            decimal roundedOneAnswerPrice = Math.Floor(OneAnswerPrice * 100) / 100;
            OneAnswerPrice = roundedOneAnswerPrice;

            SumToday = DailyConst + AnswersCountToday * OneAnswerPrice;
                     

        }        
    }    
}
