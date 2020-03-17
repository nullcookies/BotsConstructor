using System.Threading.Tasks;
using LogicalCore.TreeNodes;
using Telegram.Bot.Types;

namespace LogicalCore
{
    public interface INodeVisitor
    {
        void GoToNode(ISendingMessage node);
        void GoToNode(ISendingMessage node, out Task<Message> msgTask);
        IMarkupTree MarkupTree { get; }
        ITreeNode CurrentNode { get; set; }
        int BlockNodePosition { get; set; }
    }
}