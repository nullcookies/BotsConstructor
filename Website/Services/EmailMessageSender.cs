
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Website.Other;

namespace Website.Services
{
    public class EmailMessageSender
    {
        
        private static string _email  = ConfigurationManager.AppSettings["CombatEmail"];
        private static string _emailPassword  = ConfigurationManager.AppSettings["CombatEmailPassword"];

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
                    throw new Exception("Email введён неверно.");
                }

                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                //mail.From = new MailAddress("test7684534578945@gmail.com");

                mail.From = new MailAddress(_email, "Bots constructor");
                
                mail.To.Add(email);
                mail.Subject = "Уведомление о регистрации";
                mail.Body =  $"Поздравляем с регистрацией на платформе Interactive bots 🤗👍🏻\nДля подтверждения своего email перейдите по ссылке {link} .";
                

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential(_email, _emailPassword);
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

                mail.From = new MailAddress(_email, "Bots constructor");
                mail.To.Add(email);
                
                mail.Subject = "Сброс пароля";

                mail.Body = $"Для сброса пароля на платформе Interactive bots перейдите по ссылке {link} . Если не вы пытаетесь сбросить пароль, то кто-то имеет доступ к вашему аккаунту. Для предотвращения урона нажмите на кнопку \"Завершить все сессии\" во вкладке\"Аккаунт\"";

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential(_email, _emailPassword);
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

