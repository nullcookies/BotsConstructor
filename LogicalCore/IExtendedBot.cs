using System;
using System.Collections.Generic;

namespace LogicalCore
{
    public interface IExtendedBot : IBot, IWithStatistics, IWithSessions
    {
        IMarkupTree MarkupTree { get; set; }
        BotOwner BotOwner { get; }
        ITextMessagesManager TMM { get; }
        IGlobalFilter GlobalFilter { get; }
        IVariablesContainer GlobalVars { get; }
        List<string> Languages { get; }
        Action<IVariablesContainer> InitializeSessionVars { get; set; }
    }
}