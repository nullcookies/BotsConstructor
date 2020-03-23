using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MyLibrary
{
    public static class HttpClientWrapper
    {
        public static int? GetAccountIdFromCookies(HttpContext httpContext)
        {
            var idStr = "";
            try
            {
                idStr = httpContext.User.FindFirst(x => x.Type == "userId").Value;
                int.TryParse(idStr, out var id);
                return id;
            }catch(Exception ex)
            {
                //TODO записать ошибку в лог
                Console.WriteLine($"Ошибка. Не удалось распарсить idStr = |{idStr}| к типу int. "+ex.Message );
                return null;
            }
        }
	
        //TODO выбросить WebRequest
        /// <summary>
        /// Using HttpClientWrapper.SendPost("https://localhost:8080/StopBot", "botId=" + bot.Id);
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task<string> SendPostAsync(string url, string data="")
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
                using (StreamReader reader = new StreamReader(stream ?? throw new NullReferenceException()))
                {
                    answer = reader.ReadToEnd();
                }
            }
            response.Close();
            return answer;
        }
    }
}
