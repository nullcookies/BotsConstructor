using Newtonsoft.Json.Linq;

namespace Website.Services
{
    public interface IBotStorageNegotiator
    {
        /// <summary>
        /// Запуск бота
        /// </summary>
        /// <param name="botId">Id бота в БД</param>
        /// <param name="accountId">Id аккаунта, для которого выполняется запуск. Например: создатель бота или администратор бота.</param>
        /// <returns> JObject со статусом запроса.</returns>
        JObject StartBot(int botId, int accountId);
        /// <summary>
        /// Остановка бота
        /// </summary>
        /// <param name="botId">Id бота в БД</param>
        /// <param name="accountId">Id аккаунта, для которого выполняетсся остановка. Например: создатель бота или администратор бота.</param>
        /// <returns> JObject со статусом запроса.</returns>
        JObject StopBot(int botId, int accountId);
    }
}