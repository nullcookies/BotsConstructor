namespace LogicalCore
{
    public interface IWithSessions
    {
        ISession GetSessionByTelegramId(int id);
        bool TryGetSessionByTelegramId(int id, out ISession session);
    }
}