using Telegram.Bot.Types;

namespace LogicalCore
{
    public interface IFilter
    {
        void Filter(ISession session, Message message);
        void Filter(ISession session, CallbackQuery callbackQuery);
    }
}