using LogicalCore.TreeNodes;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace LogicalCore
{
    public interface ISession : ITranslator, IInputHandler, ILastInputSaver, INodeVisitor
    {
        int TelegramId { get; }
        IVariablesContainer Vars { get; }
        string Me { get; }
        IExtendedBot BotWrapper { get; }
        BotOwner BotOwner { get; }
        ITelegramBotClient BotClient { get; }
        IMarkupTree MarkupTree { get; }
        IGlobalFilter GlobalFilter { get; }
        ITreeNode CurrentNode { get; set; }
        User User { get; }
        int BlockNodePosition { get; set; }
    }
}