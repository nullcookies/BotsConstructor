using LogicalCore;
using Telegram.Bot.Types;

namespace Forest.Services
{
    public class BotDiagnosisService
    {
        void DiagnoseBot(IBot bot)
        {
            Message message = new Message()
            {
                Text = "рандомный текст, на котрый не нужно отвечать",
                From  = new User()
                {
                    Id = int.MinValue
                }
            };
            
            
            
        }       
    }
}