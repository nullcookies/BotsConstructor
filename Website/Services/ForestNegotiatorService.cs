using System.Linq;
using DataLayer;
using Newtonsoft.Json.Linq;

namespace Website.Services
{
    public interface IForestNegotiatorService
    {
        string SendStartBotMessage(int botId);
        string SendStopBotMessage(int botId);
    }

    public class DichStubForestNegotiatorService : IForestNegotiatorService
    {
        private readonly ApplicationContext dbContext;

        public DichStubForestNegotiatorService(ApplicationContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public string SendStartBotMessage(int botId)
        {
            JObject jObject = new JObject
            {
                {"success", true}
            };
            dbContext.RouteRecords.Add(new RouteRecord{BotId = botId});
            dbContext.SaveChanges();
            return Newtonsoft.Json.JsonConvert.SerializeObject(jObject);
        }

        public string SendStopBotMessage(int botId)
        {
            JObject jObject = new JObject
            {
                {"success", true}
            };
            var routeRecord = dbContext.RouteRecords.SingleOrDefault(rr => rr.BotId == botId);
            if (routeRecord != null)
            {
                dbContext.RouteRecords.Remove(routeRecord);
                dbContext.SaveChanges();
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(jObject);
        }
    }
}