using System;
using System.Configuration;
using System.Net.Mail;
using System.Text.RegularExpressions;
using DataLayer;
using MyLibrary;

//20 09 2019 14:09 дублирование кода

namespace Website.Services
{
    public class EmailMessageSender
    {
        
        private static readonly string Email  = ConfigurationManager.AppSettings["CombatEmail"];
        private static readonly string EmailPassword  = ConfigurationManager.AppSettings["CombatEmailPassword"];
        private readonly SimpleLogger _logger;

        public EmailMessageSender(SimpleLogger logger)
        {
            _logger = logger;
        }

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
                SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress(Email, "Bots Constructor");
                mail.To.Add(email);
                mail.Subject = "Уведомление о регистрации";
                mail.Body =  $"Поздравляем с регистрацией на платформе Bots Constructor! 🤖🛠\nДля подтверждения своего email перейдите по ссылке {link}";

                smtpServer.Port = 587;
                smtpServer.Credentials = new System.Net.NetworkCredential(Email, EmailPassword);
                smtpServer.EnableSsl = true;
                smtpServer.Send(mail);
           
                return true;

            }catch (Exception ex)
            {
                _logger.Log(LogLevel.EMAIL_SEND_FAILURE,Source.WEBSITE, 
                    $"Не удалось отправить email с данными для окончания регистрации. email={email}, name={name}, link={link}",ex:ex );
                
                return false;
            }
        }
        public bool SendPasswordReset(string email, string name, string link)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress(Email, "Bots Constructor");
                mail.To.Add(email);
                mail.Subject = "Сброс пароля";
                mail.Body = $"Для сброса пароля на платформе Bots Constructor перейдите по ссылке {link} .\n Если это не Вы пытаетесь сбросить пароль, то кто-то имеет доступ к Вашему аккаунту. Для предотвращения урона нажмите на кнопку \"Завершить все сессии\" во вкладке\"Аккаунт\".";

                smtpServer.Port = 587;
                smtpServer.Credentials = new System.Net.NetworkCredential(Email, EmailPassword);
                smtpServer.EnableSsl = true;
                smtpServer.Send(mail);

                return true;

            }catch (Exception ex)
            {
                _logger.Log(LogLevel.EMAIL_SEND_FAILURE, Source.WEBSITE,
                    "Не удалось отправить email для сброса пароля", ex: ex);
                return false;
            }
        }
     


    }
}

