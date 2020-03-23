using Telegram.Bot.Types;

namespace LogicalCore
{
    public interface IInputHandler
    {
        void TakeControl(Message message);
        void TakeControl(CallbackQuery callbackQuery);
    }
}