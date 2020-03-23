using System;

namespace LogicalCore
{
    public interface ITranslator
    {
        string Language { get; set; }
        Func<string, string> Translate { get; }
        string Retranslate(string text);
        bool TryRetranslate(string text, out string key);
    }
}