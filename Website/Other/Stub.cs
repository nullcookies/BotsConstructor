using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using website.Models;

namespace website.Other
{
    public static class Stub
    {
        public static string DefaultMarkup { get; internal set; } = "{ \"inOutMessages\":[{\"IncomingMessage\":{\"Text\":\"Хоба, добридень\"},\"OutgoingMessage\":{\"TextIsAllowed\":true,\"VideoAllowed\":false,\"AudioAllowed\":false,\"LocationAllowed\":false}}]}";

        public static string GetLoginByHttpContext(HttpContext httpContext)
        {
            string login = httpContext
             .User
             .Claims
             .Where(claim => claim.Type == claim.Subject.NameClaimType)
             .Select(claim => claim.Value)
             .FirstOrDefault();
            return login;
        }

        public static int? GetAccountIdByHttpContext(HttpContext httpContext, ApplicationContext context)
        {


            string idStr = "";

            try
            {

                idStr = httpContext.User.FindFirst(x => x.Type == "userId").Value;

                int id = int.MaxValue;

                int.TryParse(idStr, out id);

                return id;
            }catch(Exception ex)
            {
                //TODO записать ошибку в лог
                Console.WriteLine($"Ошибка. Не удалось распарсить idStr = |{idStr}| к типу int. "+ex.Message );
                return null;
            }

        }


		public static string GetMyNgrokLink()
        {
            try
            {
                string localNgrokUrl = "http://localhost:4040/api/tunnels";
                WebRequest request = WebRequest.Create(localNgrokUrl);
                WebResponse response = request.GetResponseAsync().Result;
                string myNgrokLink = "";
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string resp = reader.ReadToEnd();
                        var j = JObject.Parse(resp);
                        string url = ((string)j.First.First.First.Next["public_url"]);

                        if (!url.Contains("https"))
                            url.Insert(4, "s");

                        myNgrokLink = url;
                    }
                }
                response.Close();
                return myNgrokLink;
            }
            catch
            {
                throw new Exception("Не удалось узнать ngrok ссылку. Для нормальной работы нужно использовать ngrok.");
            }
        }

        internal static async Task<string> SendPost(string url, string data)
        {
            WebRequest request = WebRequest.Create(url);
            request.Method = "POST";
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(data);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }


            WebResponse response = await request.GetResponseAsync();


            string answer ;
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    answer = reader.ReadToEnd();
                }
            }
            response.Close();

            return answer;
        }

        private static async Task Unregister()
        {
           
                //string monitorUrl = Stub.GetMyNgrokLink();
                string monitorUrl = "https://localhost:31416" + "/Monitor/StatisticsCollection/UnregisterSiteServer";

                string data = "siteServerUrl=5001";

                string answer = await Stub.SendPost(monitorUrl, data);

                if (answer == "ok")
                    return;
                else
                    throw new Exception(answer);
           
        }

        private static async Task Register()
        {
            
                //string monitorUrl = Stub.GetMyNgrokLink();
                string monitorUrl = "https://localhost:31416" + "/Monitor/StatisticsCollection/RegisterSiteServer";

                string data = "siteServerUrl=5001";

                string answer = await Stub.SendPost(monitorUrl, data);

                if (answer == "ok")
                    return;
                else
                    throw new Exception(answer);
            

        }


        public static async void RegisterInMonitor()
        {
            await Stub.Register();
        }

        public static async void UnregisterInMonitor()
        {
            await Stub.Unregister();
        }
    }
}
