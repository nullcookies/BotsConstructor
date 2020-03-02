using System;
using System.Collections.Generic;

namespace LogicalCore
{
    public class UntranslatableTextMessagesManager : AbstractTextMessagesManager
	{
        public UntranslatableTextMessagesManager(List<string> langs = null, string defLang = null) : base(langs, defLang)
		{ }

		/// <summary>
		/// Возвращает переводчик сообщений для выбранного языка.
		/// </summary>
		/// <param name="language">Язык, для которого необходимо получить переводчик.</param>
		/// <returns>Возвращает функцию-переводчик сообщений.</returns>
		public override Func<string, string> GetTranslator(string language) => GetKeyFromTextIfExists;

		/// <summary>
		/// Возвращает ключ сообщения из текста, или же исходное сообщение, если ключа нет.
		/// </summary>
		/// <param name="text">Текст, для которого нужно выполнить "обратный перевод".</param>
		/// <returns>Возвращает строку - ключ сообщения из текста или текст.</returns>
		public override string GetKeyFromTextIfExists(string text) => text;

		/// <summary>
		/// Пытается вернуть ключ сообщения из текста.
		/// </summary>
		/// <param name="text">Текст, для которого нужно выполнить "обратный перевод".</param>
		/// <param name="key">Ключ, который соответствует сообщению (если был найден).</param>
		/// <returns>Возвращает true, если ключ был найден или false, если такого ключа нет.</returns>
		public override bool TryGetKeyFromTextIfExists(string text, out string key)
		{
			key = text;
			return true;
		}
	}
}
