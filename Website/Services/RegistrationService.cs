using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer;
using Microsoft.AspNetCore.Http;
using Website.ViewModels;

namespace Website.Services
{
    public class RegistrationService
    {
        private readonly ApplicationContext _context;
        private readonly EmailMessageSender _emailSender;

        public RegistrationService(ApplicationContext context, EmailMessageSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }
        
        public void Register(RegisterModel model, string domain) =>
            Register(model, domain, out string link);
        
        private void Register(RegisterModel model, string domain, out string link)
        {
            link = "";
            bool thereIsNoSuchEmailYet = _context.Accounts.FirstOrDefault(a => a.Email == model.Email) == null;
            if (thereIsNoSuchEmailYet)
            {
                Account account = new Account
                {
                    //Сначала нужно подтвердить email
                    //Email = model.Email,
                    Name = model.Name, Password = model.Password, RoleTypeId = 1
                };

                //TODO: менять сообщения и названия в зависимости от языка владельца бота
                var statusGroup = new OrderStatusGroup()
                {
                    Name = "Стандартный набор статусов",
                    Owner = account,
                    OrderStatuses = new List<OrderStatus>()
                    {
                        new OrderStatus() {Name = "Просмотрено", Message = ""},
                        new OrderStatus() {Name = "В обработке", Message = "Ваш заказ находится в обработке."},
                        new OrderStatus() {Name = "В пути", Message = "Ваш заказ в пути."},
                        new OrderStatus() {Name = "Принят", Message = "Ваш заказ был принят."},
                        new OrderStatus() {Name = "Отменён", Message = "Ваш заказ был отменён."}
                    }
                };

                _context.OrderStatusGroups.Add(statusGroup);
                _context.Accounts.Add(account);
                _context.SaveChanges();

                //Отправка сообщения на указанный email, чтобы удостовериться, что он принадлежит этому пользователю
                if (!string.IsNullOrEmpty(model.Email))
                {
                    if (EmailMessageSender.EmailIsValid(model.Email))
                    {
                        Guid guid = Guid.NewGuid();
                        link =
                            $"https://{domain}/SignUp/EmailCheckSuccess?guid={guid.ToString()}&accountId={account.Id}";

                        var unconfirmedEmail = new UnconfirmedEmail()
                        {
                            AccountId = account.Id, Email = model.Email, GuidPasswordSentToEmail = guid
                        };
                        _context.UnconfirmedEmails.Add(unconfirmedEmail);

                        //Отправка сообщения
                        bool sendIsOk = _emailSender.SendEmailCheck(model.Email, model.Name, link, "botsconstructor@gmail.com", "ktoetotam_pitaetsavoiti_yaNePonyal12756");
                        if (!sendIsOk)
                        {
                            //если email не отправился, то удалить из БД запись о нём
                            _context.UnconfirmedEmails.Remove(unconfirmedEmail);
                        }
                    }
                }

                _context.SaveChanges();
            }
            else
            {
                throw new Exception("Аккаунт с таким email уже существует");
            }
        }

        public void ConfirmEmail(Guid guid, int accountId)
        {

            UnconfirmedEmail unconfirmedEmail = _context.UnconfirmedEmails
                .SingleOrDefault(_ue => _ue.AccountId == accountId);

            
            if (unconfirmedEmail != null)
            {
                Guid guidFromDb = unconfirmedEmail.GuidPasswordSentToEmail;
                if (guidFromDb == guid)
                {
                    Account acc = _context.Accounts.Find(accountId);
                    if (acc != null)
                    {
                        if (!string.IsNullOrEmpty(unconfirmedEmail.Email))
                        {
                            //Присвоить почту аккаунту
                            acc.Email = unconfirmedEmail.Email;
                            //убрать запись из таблицы неподтверждённых email
                            _context.UnconfirmedEmails.Remove(unconfirmedEmail);
                            _context.SaveChanges();

                        }
                        else
                        {
                            throw new Exception("Ошибка логики сервера. В базе данных не найден email, который нужно подтвердить.");
                        }
                    }
                    else
                    {
                        throw new Exception("Ошибка логики сервера. В базе данных не найден аккаунт, к которому нужно привязать email.");
                    }
                }
                else
                {
                    throw new Exception($"Мне не нравится guid accountId={accountId},guid={guid}");
                }
            }
            else
            {
                throw new Exception($"В базе нет запроса на подтверждение accountId={accountId},guid={guid}");
            }
            

        }
    }
}