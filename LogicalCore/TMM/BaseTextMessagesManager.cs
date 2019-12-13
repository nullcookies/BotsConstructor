using System;
using System.Collections.Generic;

namespace LogicalCore
{
	public class BaseTextMessagesManager
	{
		/// <summary>
		/// Язык без перевода (язык разметки).
		/// </summary>
		protected const string nullLanguage = "null";
		/// <summary>
		/// Язык по умолчанию.
		/// </summary>
		public readonly string defaultLanguage;
		/// <summary>
		/// Список доступных языков.
		/// </summary>
		public readonly List<string> languages;

		public BaseTextMessagesManager(List<string> langs = null, string defLang = null)
		{
			languages = langs;
			if ((languages == null) || (languages.Count == 0))
			{
				languages = new List<string>(1) { nullLanguage };
			}

			defaultLanguage = string.IsNullOrWhiteSpace(defLang) ? nullLanguage : defLang;
			if (!languages.Contains(defaultLanguage))
			{
				defaultLanguage = languages[0];
			}
		}

		/// <summary>
		/// Возвращает переводчик сообщений для выбранного языка.
		/// </summary>
		/// <param name="language">Язык, для которого необходимо получить переводчик.</param>
		/// <returns>Возвращает функцию-переводчик сообщений.</returns>
		public virtual Func<string, string> GetTranslator(string language) => GetKeyFromTextIfExists;

		/// <summary>
		/// Возвращает ключ сообщения из текста, или же исходное сообщение, если ключа нет.
		/// </summary>
		/// <param name="text">Текст, для которого нужно выполнить "обратный перевод".</param>
		/// <returns>Возвращает строку - ключ сообщения из текста или текст.</returns>
		public virtual string GetKeyFromTextIfExists(string text) => text;

		/// <summary>
		/// Пытается вернуть ключ сообщения из текста.
		/// </summary>
		/// <param name="text">Текст, для которого нужно выполнить "обратный перевод".</param>
		/// <param name="key">Ключ, который соответствует сообщению (если был найден).</param>
		/// <returns>Возвращает true, если ключ был найден или false, если такого ключа нет.</returns>
		public virtual bool TryGetKeyFromTextIfExists(string text, out string key)
		{
			key = text;
			return true;
		}
	}
}
