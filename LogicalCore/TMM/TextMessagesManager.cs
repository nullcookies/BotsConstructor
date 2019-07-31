//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace LogicalCore
//{
//	public class TextMessagesManager : BaseTextMessagesManager
//    {
//        /// <summary>
//        /// Словарь словарей для перевода сообщений {Язык, {Переменная, Значение}}.
//        /// </summary>
//        private readonly Dictionary<string, Dictionary<string, string>> keysToValuesDicts;
//		/// <summary>
//		/// Переводчики сообщений {Язык, (Ключ) => Значение}.
//		/// </summary>
//		private readonly Dictionary<string, Func<string, string>> translators;
//        /// <summary>
//        /// Словарь для обратного перевода сообщений {Значение, Переменная}.
//        /// </summary>
//        private readonly Dictionary<string, string> valuesToKeysDict;

//        public TextMessagesManager(Dictionary<string, Dictionary<string, string>> keysToValuesTranslator = null, string defLang = nullLanguage, bool tryGetForTranslator = true) :
//			base(keysToValuesTranslator?.Keys.ToList(), defLang)
//        {
//            ConsoleWriter.WriteLine("Создание менеджера сообщений...", ConsoleColor.White);
//            keysToValuesDicts = keysToValuesTranslator ?? new Dictionary<string, Dictionary<string, string>>(0);
//			translators = new Dictionary<string, Func<string, string>>(keysToValuesDicts.Count + 1)
//			{ { nullLanguage, (key) => key } }; // Изначальный переводчик без перевода
			
//			if (!keysToValuesDicts.Keys.Contains(defaultLanguage))
//			{
//				ConsoleWriter.WriteLine($"Язык по умолчанию '{defLang}' не находится в словаре!", ConsoleColor.Red);
//			}

//			if(defaultLanguage != null)
//			{
//				Dictionary<string, string> defaultDict = keysToValuesDicts[defaultLanguage];

//				valuesToKeysDict = new Dictionary<string, string>(keysToValuesDicts.Count * defaultDict.Count);
//				foreach (var pair in keysToValuesDicts)
//				{
//					if (pair.Key != defaultLanguage)
//					{
//						if (pair.Value.Keys.Count != defaultDict.Keys.Count || !pair.Value.Keys.All(defaultDict.Keys.Contains))
//						{
//							throw new Exception("Словари для разных языков не совпадают!");
//						}
//					}

//					foreach (var keyValue in pair.Value)
//					{
//						valuesToKeysDict.TryAdd(keyValue.Value, keyValue.Key);
//					}

//					if (tryGetForTranslator)
//					{
//						translators.Add(pair.Key, (key) =>
//						pair.Value.TryGetValue(key, out string value) ? value : key);
//					}
//					else
//					{
//						translators.Add(pair.Key, (key) => pair.Value[key]);
//					}
//				}
//			}
//			else
//			{
//				valuesToKeysDict = new Dictionary<string, string>(0);
//			}

//            ConsoleWriter.WriteLine("Создание и проверка менеджера сообщений завершена.", ConsoleColor.White);
//        }

//		/// <summary>
//		/// Возвращает переводчик сообщений для выбранного языка.
//		/// </summary>
//		/// <param name="language">Язык, для которого необходимо получить переводчик.</param>
//		/// <returns>Возвращает функцию-переводчик сообщений.</returns>
//		public override Func<string, string> GetTranslator(string language) =>
//			translators.TryGetValue(language, out var translator) ?
//			translator : translators[nullLanguage];

//        /// <summary>
//        /// Возвращает ключ сообщения из текста, или же исходное сообщение, если ключа нет.
//        /// </summary>
//        /// <param name="text">Текст, для которого нужно выполнить "обратный перевод".</param>
//        /// <returns>Возвращает строку - ключ сообщения из текста или текст.</returns>
//        public override string GetKeyFromTextIfExists(string text)
//        {
//			if (string.IsNullOrWhiteSpace(text)) return text;
//            if (valuesToKeysDict.TryGetValue(text, out string key)) return key;
//            return text;
//        }

//		/// <summary>
//		/// Пытается вернуть ключ сообщения из текста.
//		/// </summary>
//		/// <param name="text">Текст, для которого нужно выполнить "обратный перевод".</param>
//		/// <param name="key">Ключ, который соответствует сообщению (если был найден).</param>
//		/// <returns>Возвращает true, если ключ был найден или false, если такого ключа нет.</returns>
//		public override bool TryGetKeyFromTextIfExists(string text, out string key) => valuesToKeysDict.TryGetValue(text ?? "", out key);
//    }
//}
