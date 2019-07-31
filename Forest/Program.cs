using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Configuration;
using System.Net;
using System.Runtime.InteropServices;
using Telegram.Bot;
using LogicalCore;

namespace DeleteMeWebhook
{
    public class Program
    {
        public static bool WebhookIsEnabledInDebugMode = false;
        public static string Url;

        public static void Main(string[] args)
        {
           
            //Url = Ngrok.GetMyAddress().Result;

            //База
            //ConnectToDatabase();

            //Отладка при отсутствии разметки
            RunHardcodeBot();

            //Сообщить мастеру про свою работоспособность

            //Включить веб-сервер
            //Принимает команду запуска/остановки ботов
            CreateWebHostBuilder(args).Build().Run();
        }

        private static void RunHardcodeBot()
        {
            bool WebhookIsOn = IsWebhookNeeded();

            string token = ConfigurationManager.AppSettings["token"];

            if (WebhookIsOn)
            {
                bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                if (isWindows)
                { 
                    string hardcodeLink = "https://3e93e1c7.ngrok.io/" + new TelegramBotClient(token).GetMeAsync().Result.Username;

                    BotWrapper botWrapper = Stub.CreateBot(token, hardcodeLink);

                    Stub.RunAndRegisterBot(botWrapper); 
                }
                else
                {
                    //Запрос ссылки из консоли и запуск вебхука
                    while (true)
                    {
                        try
                        {
                            ConsoleWriter.WriteLine("Please enter the link:");
                            string link = Console.ReadLine();
                            BotWrapper botWrapper = Stub.CreateBot(token, link);
                            Stub.RunAndRegisterBot(botWrapper);
                            break;
                        }
                        catch (Exception eee)
                        {
                            ConsoleWriter.WriteLine("Something going wrong");
                            ConsoleWriter.WriteLine(eee.Message, ConsoleColor.Red);
                        }
                    }
                }
            }
            else
            {
                //Запуск long polling
                BotWrapper botWrapper = Stub.CreateBot(token);
                Stub.RunAndRegisterBot(botWrapper);
            }
        }

     

        private static bool IsWebhookNeeded()
        {
            bool webhookIsOn = false;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                webhookIsOn = WebhookIsEnabledInDebugMode;
            }
            else
            {
                //спросить при запуске
                bool needAsk = true;
                string answer;
                while (needAsk)
                {
                    ConsoleWriter.WriteLine("Do you need webhook? [y/n]");
                    answer = Console.ReadLine();
                    switch (answer)
                    {
                        case "y":
                        case "yes":
                            webhookIsOn = true;
                            needAsk = false;
                            break;

                        case "n":
                        case "no":
                            webhookIsOn = false;
                            needAsk = false;
                            break;

                        default:
                            ConsoleWriter.WriteLine("I understand only the answers 'yes' ('y') or 'no' ('n')", ConsoleColor.DarkRed);
                            ConsoleWriter.WriteLine($"You answered :\"{answer}\"", ConsoleColor.DarkYellow);
                            break;
                    }
                }
            }
            
            return webhookIsOn;
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
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
    }
}




