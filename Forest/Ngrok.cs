using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace DeleteMeWebhook
{
    internal class Ngrok
    {
        internal static async Task<string> GetMyAddress()
        {

            try
            {
                //запрос на локальный сервер ngrok
                string localNgrokUrl = "http://localhost:4040/api/tunnels";


                WebRequest request = WebRequest.Create(localNgrokUrl);
                WebResponse response = await request.GetResponseAsync();

                string myNgrokUrl = "";

                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        //корявый парсинг json-a с внешним url
                        string resp = reader.ReadToEnd();
                        var j = JObject.Parse(resp);
                        string url = ((string)j.First.First.First.Next["public_url"]);

                        //если достал http, то вставить букву s
                        if (!url.Contains("https"))
                            url.Insert(4, "s");

                        myNgrokUrl = url;
                    }
                }
                response.Close();
                return myNgrokUrl;
            }
            catch
            {

            }


            const string commands = @"d:
                                      cd d:\WorkingDir\
                                      ngrok http 8080";
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    UseShellExecute = false

                }
            };
            process.Start();

            using (StreamWriter pWriter = process.StandardInput)
            {
                if (pWriter.BaseStream.CanWrite)
                {
                    foreach (var line in commands.Split('\n'))
                        pWriter.WriteLine(line);
                }
            }

            Thread.Sleep(3000);

            try
            {
                //запрос на локальный сервер ngrok
                string localNgrokUrl = "http://localhost:4040/api/tunnels";


                WebRequest request = WebRequest.Create(localNgrokUrl);
                WebResponse response = await request.GetResponseAsync();

                string myNgrokUrl = "";

                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        //корявый парсинг json-a с внешним url
                        string resp = reader.ReadToEnd();
                        var j = JObject.Parse(resp);
                        string url = ((string)j.First.First.First.Next["public_url"]);

                        //если достал http, то вставить букву s
                        if (!url.Contains("https"))
                            url.Insert(4, "s");

                        myNgrokUrl = url;
                    }
                }
                response.Close();
                return myNgrokUrl;
            }
            catch(Exception e)
            {
                throw new Exception("Не удалось узнать ngrok ссылку. Для нормальной работы нужно использовать ngrok. "+e.Message);
            }
        }
    }
}