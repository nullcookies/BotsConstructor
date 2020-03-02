using System;
using System.Collections.Generic;

namespace LogicalCore
{
    public interface ITextMessagesManager
    {
        string DefaultLanguage { get; }
        List<string> Languages { get; }
        Func<string, string> GetTranslator(string language);
        string GetKeyFromTextIfExists(string text);
        bool TryGetKeyFromTextIfExists(string text, out string key);

    }
}