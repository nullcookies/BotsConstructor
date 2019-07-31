using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace LogicalCore
{
    public interface IFlippable
    {
		bool GlobalCallbacks { get; }
		Task<Message> SendPage(Session session, Message divisionMessage, int pageNumber = 0);
    }
}
