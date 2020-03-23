namespace Website.Services
{
    public enum BotStartFailureReason
    {
        BotWithSuchIdDoesNotExist,
        NoAccessToThisBot,
        NotEnoughFundsInTheAccountOfTheBotOwner,
        TokenMissing,
        NoMarkupData,
        ThisBotIsAlreadyRunning,
        ServerErrorWhileStartingTheBot,
        ConnectionError
    }
}