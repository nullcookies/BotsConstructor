using System;
using System.Collections.Generic;

namespace LogicalCore
{
    public abstract class AbstractTextMessagesManager : ITextMessagesManager
    {
        /// <summary>
        /// Язык без перевода (язык разметки).
        /// </summary>
        protected const string nullLanguage = "null";

        /// <summary>
        /// Язык по умолчанию.
        /// </summary>
        public string DefaultLanguage { get; }

        /// <summary>
        /// Список доступных языков.
        /// </summary>
        public List<string> Languages { get; }

        protected AbstractTextMessagesManager(List<string> langs = null, string defLang = null)
        {
            Languages = langs;
            if ((Languages == null) || (Languages.Count == 0))
            {
                Languages = new List<string>(1) { nullLanguage };
            }

            DefaultLanguage = string.IsNullOrWhiteSpace(defLang) ? nullLanguage : defLang;
            if (!Languages.Contains(DefaultLanguage))
            {
                DefaultLanguage = Languages[0];
            }
        }
        public abstract Func<string, string> GetTranslator(string language);
        public abstract string GetKeyFromTextIfExists(string text);
        public abstract bool TryGetKeyFromTextIfExists(string text, out string key);
    }
}