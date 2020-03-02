using System.Threading.Tasks;

namespace LogicalCore
{
	public interface IOrdersSendable
	{
		Task<bool> SendOrder(ISession session, UniversalOrderContainer order, int statGroupID);
	}
}
