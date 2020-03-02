using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace LogicalCore
{
	public interface ISendingMessage
    {
        /// <summary>
        /// Выполняет отправку переведённого сообщения указанной сессии.
        /// </summary>
        /// <param name="session">Сессия, для которой нужно сделать перевод и отправку сообщения.</param>
        /// <returns>Возвращает <see cref="Task{Message}"/> с отправкой сообщения.</returns>
        Task<Message> SendMessage(ISession session);
    }
}
