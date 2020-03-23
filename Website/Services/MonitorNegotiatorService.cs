namespace Website.Services
{
    public class MonitorNegotiatorService
    {
        public string GetUrlToRunBot()
        {
            return "http://localhost:8080/Home/RunNewBot";
        }
    }
}