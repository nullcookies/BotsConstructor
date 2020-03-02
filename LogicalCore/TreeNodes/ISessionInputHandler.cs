using Telegram.Bot.Types;

namespace LogicalCore.TreeNodes
{
    public interface ISessionInputHandler
    {
        void TakeControl(ISession session, Message message);
        void TakeControl(ISession session, CallbackQuery callbackQuery);
    }
}