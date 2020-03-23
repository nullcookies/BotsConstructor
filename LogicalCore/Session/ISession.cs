using Telegram.Bot;
using Telegram.Bot.Types;

namespace LogicalCore
{
    public interface ISession : ITranslator, IInputHandler, ILastInputSaver, INodeVisitor
    {
        int TelegramId { get; }
        User User { get; }
        IVariablesContainer Vars { get; }
        string Me { get; }
        IExtendedBot BotWrapper { get; }
        BotOwner BotOwner { get; }
        ITelegramBotClient BotClient { get; }
        IFilter GlobalFilter { get; }
    }
}