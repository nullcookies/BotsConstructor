using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace LogicalCore
{
    public interface INodeVisitor
    {
        void GoToNode(ISendingMessage node);
        void GoToNode(ISendingMessage node, out Task<Message> msgTask);
    }
}