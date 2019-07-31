using OpenPop.Mime;
using OpenPop.Pop3;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using website.Other;

namespace website.Services
{
    public class EmailMessageSender
    {

        public static bool EmailIsValid(string email)
        {
            if (email == null) { return false; }
            email = email.Trim();

            string pattern = "[.\\-_a-z0-9]+@([a-z0-9][\\-a-z0-9]+\\.)+[a-z]{2,6}";
            Match isMatch = Regex.Match(email.ToLower(), pattern, RegexOptions.IgnoreCase);
            return isMatch.Success;
        }


        public bool SendEmailCheck(string email, string name, string link)
        {
            email = email.Trim();

            try
            {

                if (!EmailIsValid(email))
                {
                    throw new Exception("Email введйн неверно. Таких email-ов не существует.");
                }

                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("test7684534578945@gmail.com");
                mail.To.Add(email);
                //mail.Subject = "Registration notice";
                mail.Subject = "Уведомление о регистрации";

                mail.Body =  $"Поздравляем с регистрацией на платформе Interactive bots 🤗👍🏻\nДля подтверждения своего email перейдите по ссылке {link} .";

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("test7684534578945@gmail.com", "nuzset0chn0p3r3m0ga");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
           
                return true;
            }catch (Exception ex)
            {
                //Запись в лог ошибок
                Console.WriteLine("Письмо не отправлено");
                Console.WriteLine(ex.Message);
                
                return false;
            }
        }
        public bool SendPasswordReset(string email, string name, string link)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("test7684534578945@gmail.com");
                mail.To.Add(email);
                
                mail.Subject = "Сброс пароля";

                mail.Body = $"Для сброса пароля на платформе Interactive bots перейдите по ссылке {link} . Если не вы пытаетесь сбросить пароль, то кто-то имеет доступ к вашему аккаунту. Для предотвращения урона нажмите на кнопку \"Завершить все сессии\" во вкладке\"Аккаунт\"";

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("test7684534578945@gmail.com", "nuzset0chn0p3r3m0ga");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);


                return true;
            }catch (Exception ex)
            {
                //Запись в лог ошибок
                Console.WriteLine("Письмо не отправлено");
                Console.WriteLine(ex.Message);
                return false;
            }
        }


      


    }
}

/*
 
     
       public static void SendDich2()
        {

            SmtpClient client = new SmtpClient("smtp.gmail.com");


            client.Port = 587;
            client.Credentials = new System.Net.NetworkCredential("test7684534578945@gmail.com", "nuzset0chn0p3r3m0ga");
            client.EnableSsl = true;


            MailAddress from = new MailAddress("test7684534578945@gmail.com",
               "Jane " + (char)0xD8 + " Clayton",
                System.Text.Encoding.UTF8);
                       

            //MailAddress to = new MailAddress("starovoytov.ruslan@gmail.com");
            MailAddress to = new MailAddress("starovhjhasdkvjhbsdvkjhbn@gmail.com");

            MailMessage message = new MailMessage(from, to);

            message.Body = "This is a test email message sent by an application. ";

            string someArrows = new string(new char[] { '\u2190', '\u2191', '\u2192', '\u2193' });
            message.Body += Environment.NewLine + someArrows;
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.Subject = "test message 1" + someArrows;
            message.SubjectEncoding = System.Text.Encoding.UTF8;


            client.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
            

            string userState = "test message1";
            client.SendAsync(message, userState);

            Console.WriteLine("Отработало.");
            
        }



        private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            string token = (string)e.UserState;

            if (e.Cancelled)
            {
                Console.WriteLine("Cancelled");
                Console.WriteLine("[{0}] Send canceled.", token);
            }
            if (e.Error != null)
            {
                Console.WriteLine("[{0}] {1}", token, e.Error.ToString());
            }
            else
            {
                Console.WriteLine("Message sent.");
            }
            
        }


        public static void GetMessages()
        {
            var client = new Pop3Client();

            client.Connect("pop.gmail.com", 995, true);
            client.Authenticate("test7684534578945@gmail.com", "nuzset0chn0p3r3m0ga");

            var count = client.GetMessageCount();
            Console.WriteLine($"count ={count }");

            for(int i = 0; i< count; i++)
            {
                Message message = client.GetMessage(i+1);
                Console.WriteLine(message.Headers.Subject);
                Console.WriteLine( message.RawMessage);
                Console.WriteLine(System.Text.Encoding.Default.GetString(message.RawMessage));
            }
        }


 
     
     */
