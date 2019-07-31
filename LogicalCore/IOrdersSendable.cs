using System.Threading.Tasks;

namespace LogicalCore
{
	public interface IOrdersSendable
	{
		Task<bool> SendOrder(Session session, UniversalOrderContainer order, int statGroupID);
	}
}
