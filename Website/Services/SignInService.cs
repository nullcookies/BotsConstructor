using System.Linq;
using DataLayer;
using Website.ViewModels;

namespace Website.Services
{
    public class SignInService
    {
        private readonly ApplicationContext _context;

        public SignInService(ApplicationContext context)
        {
            _context = context;
        }

        public bool IsVerificationPassed(LoginModel model, out Account account)
        {
              account = _context.Accounts.FirstOrDefault(a => a.Email == model.Email && a.Password == model.Password);
              return account != null;
        }
    }
}