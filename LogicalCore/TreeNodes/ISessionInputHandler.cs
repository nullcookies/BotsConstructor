using Telegram.Bot.Types;

namespace LogicalCore.TreeNodes
{
    public interface ISessionInputHandler
    {
        void TakeControl(Session session, Message message);
        void TakeControl(Session session, CallbackQuery callbackQuery);
    }
}