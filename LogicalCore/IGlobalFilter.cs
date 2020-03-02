using Telegram.Bot.Types;

namespace LogicalCore
{
    public interface IGlobalFilter
    {
        void Filter(ISession session, Message message);
        void Filter(ISession session, CallbackQuery callbackQuery);
    }
}