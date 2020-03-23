using Telegram.Bot;
using Telegram.Bot.Types;

namespace LogicalCore
{
    public interface IBot
    {
        int BotId { get; }
        string BotUsername { get; }
        ITelegramBotClient BotClient { get; }
        void Run();
        void Stop();
        void AcceptUpdate(Update update);

    }
}