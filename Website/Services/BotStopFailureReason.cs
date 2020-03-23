namespace Website.Services
{
    public enum BotStopFailureReason
    {
        BotWithSuchIdDoesNotExist,
        NoAccessToThisBot,
        ThisBotIsAlreadyStopped,
        ServerErrorWhileStoppingTheBot,
        ConnectionError
    }
}