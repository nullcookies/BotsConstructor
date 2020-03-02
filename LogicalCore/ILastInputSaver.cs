using Telegram.Bot.Types;

namespace LogicalCore
{
    public interface ILastInputSaver
    {
        CallbackQuery LastCallback { get; }
        Message LastMessage { get; }
    }
}