using System;
using System.Configuration;
using System.Net;
using System.Runtime.InteropServices;
using DeleteMeWebhook;
using LogicalCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Telegram.Bot;

namespace Forest
{
    public static class Program
    {

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }
        
        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost
                .CreateDefaultBuilder(args)
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Loopback, 8080);
                    options.Limits.MaxConcurrentConnections = 500;
                })
                .UseStartup<Startup>();
        }

        
        
        
        
        
        
        
        
        
//        private static readonly bool WebhookIsEnabledInDebugMode = false;
//        public static string Url;


//        private static void RunHardcodeBot()
//        {
//            bool WebhookIsOn = IsWebhookNeeded();
//
//            string token = ConfigurationManager.AppSettings["token1"];
//
//            if (WebhookIsOn)
//            {
//                bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
//                if (isWindows)
//                { 
//                    //string hardcodeLink = "https://3e93e1c7.ngrok.io/" + new TelegramBotClient(token).GetMeAsync().Result.Username;
//                    string hardcodeLink = Ngrok.GetMyAddress().Result+"/"+ new TelegramBotClient(token).GetMeAsync().Result.Username;
//                    string test23 = hardcodeLink.Substring(4, 1);
//                    if (test23 != "s")
//                    {
//                        hardcodeLink =  hardcodeLink.Insert(4, "s");
//                    }
//                    BotWrapper botWrapper = Stub.CreateBot(token, hardcodeLink);
//
//                    Stub.RunAndRegisterBot(botWrapper); 
//                }
//                else
//                {
//                    //Запрос ссылки из консоли и запуск вебхука
//                    while (true)
//                    {
//                        try
//                        {
//                            ConsoleWriter.WriteLine("Please enter the link:");
//                            string link = Console.ReadLine();
//                            BotWrapper botWrapper = Stub.CreateBot(token, link);
//                            Stub.RunAndRegisterBot(botWrapper);
//                            break;
//                        }
//                        catch (Exception eee)
//                        {
//                            ConsoleWriter.WriteLine("Something going wrong");
//                            ConsoleWriter.WriteLine(eee.Message, ConsoleColor.Red);
//                        }
//                    }
//                }
//            }
//            else
//            {
//                //Запуск long polling
//                BotWrapper botWrapper = Stub.CreateBot(token);
//                Stub.RunAndRegisterBot(botWrapper);
//            }
//        }
//
//     
//
//        private static bool IsWebhookNeeded()
//        {
//            bool webhookIsOn = false;
//
//            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
//            {
//                webhookIsOn = WebhookIsEnabledInDebugMode;
//            }
//            else
//            {
//                //спросить при запуске
//                bool needAsk = true;
//                string answer;
//                while (needAsk)
//                {
//                    ConsoleWriter.WriteLine("Do you need webhook? [y/n]");
//                    answer = Console.ReadLine();
//                    switch (answer)
//                    {
//                        case "y":
//                        case "yes":
//                            webhookIsOn = true;
//                            needAsk = false;
//                            break;
//
//                        case "n":
//                        case "no":
//                            webhookIsOn = false;
//                            needAsk = false;
//                            break;
//
//                        default:
//                            ConsoleWriter.WriteLine("I understand only the answers 'yes' ('y') or 'no' ('n')", ConsoleColor.DarkRed);
//                            ConsoleWriter.WriteLine($"You answered :\"{answer}\"", ConsoleColor.DarkYellow);
//                            break;
//                    }
//                }
//            }
//            
//            return webhookIsOn;
//        }

      
    }
}




