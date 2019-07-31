using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace LogicalCore
{
    /// <summary>
    /// Контейнер <see cref="MetaValued{T}"/>, позволяющий хранить количество элементов и удаляющий их, если их количество меньше 0.
    /// </summary>
    /// <typeparam name="T">Тип в <see cref="MetaValued{T}"/>.</typeparam>
    public class MetaValuedContainer<T> : IDictionary<MetaValued<T>, int>, ISessionTranslatable, ISendingMessage, IClearable
	{
        public readonly string name;
        private readonly Dictionary<MetaValued<T>, int> mainDict;
        private readonly Dictionary<int, MetaValued<T>> translatingDict;
        private readonly string middleSeparator;
        private readonly string pairsSeparator;
        private readonly Func<Dictionary<MetaValued<T>, int>, MetaText> aggregator;
        public ICollection<MetaValued<T>> Keys => mainDict.Keys;
        public ICollection<int> Values => mainDict.Values;
        public int Count => mainDict.Count;
        public bool IsReadOnly => ((IDictionary<MetaValued<T>, int>)mainDict).IsReadOnly;

        /// <summary>
        /// Создаёт контейнер <see cref="MetaValued{T}"/> с указанными параметрами.
        /// </summary>
        /// <param name="pairsSep">Разделитель между <see cref="MetaValued{T}"/> и <see cref="int"/>.</param>
        /// <param name="midSep">Разделитель между отдельными элементами.</param>
        /// <param name="finalFunc">Функция, которая создаёт последнюю строку в сообщении по данным словаря.</param>
        public MetaValuedContainer(string containerName, string pairsSep = "\n", string midSep = ": ", Func<Dictionary<MetaValued<T>, int>, MetaText> finalFunc = null)
        {
            name = containerName;
            mainDict = new Dictionary<MetaValued<T>, int>();
            translatingDict = new Dictionary<int, MetaValued<T>>();
            pairsSeparator = pairsSep;
            middleSeparator = midSep;
            aggregator = finalFunc;
        }

        public MetaText ToMetaText()
        {
            MetaText metaText = new MetaText();

            foreach (var pair in mainDict)
            {
                metaText.Append((MetaText)pair.Key);
                metaText.Append(middleSeparator);
                metaText.Append(pair.Value);
                metaText.Append(pairsSeparator);
            }

            if (aggregator != null)
            {
                metaText.Append(aggregator.Invoke(mainDict));
            }

            return metaText;
        }

        public static implicit operator MetaText(MetaValuedContainer<T> container) => container.ToMetaText();

        public string ToString(Session session) => ToMetaText().ToString(session);

        private void PrepareMessage(Session session, out string text, out InlineKeyboardMarkup inlineKeyboard)
        {
            text = aggregator?.Invoke(mainDict).ToString(session) ?? session.Translate(name);

            InlineKeyboardButton[,] buttons = new InlineKeyboardButton[mainDict.Count, 4];

            int i = 0;
            foreach (var pair in mainDict)
            {
                string translatedText = pair.Key.ToString(session), elementHash = pair.Key.ToString().GetHashCode().ToString();
                buttons[i, 0] = InlineKeyboardButton.WithCallbackData(translatedText, DefaultStrings.DONOTHING);
                buttons[i, 1] = InlineKeyboardButton.WithCallbackData(session.Translate(DefaultStrings.MINUS), $"{DefaultStrings.MINUS}_{name}_{elementHash}");
                buttons[i, 2] = InlineKeyboardButton.WithCallbackData(pair.Value.ToString(), DefaultStrings.DONOTHING);
                buttons[i, 3] = InlineKeyboardButton.WithCallbackData(session.Translate(DefaultStrings.PLUS), $"{DefaultStrings.PLUS}_{name}_{elementHash}");
                i++;
            }

            inlineKeyboard = new InlineKeyboardMarkup(buttons.ToMultiEnumerable());
        }

        /// <summary>
        /// Отправляет сообщение с возможностью редактирования количества элементов в контейнере.
        /// </summary>
        /// <param name="session">Сессия, для которой нужно сделать перевод и отправку сообщения.</param>
        /// <returns>Возвращает <see cref="Task{Message}"/> с отправкой сообщения.</returns>
        public async Task<Message> SendMessage(Session session)
        {
            PrepareMessage(session, out string text, out InlineKeyboardMarkup inlineKeyboard);

            return await session.BotClient.SendTextMessageAsync(session.telegramId,
                text,
                ParseMode.Markdown,
                true,
                replyMarkup: inlineKeyboard);
        }

        /// <summary>
        /// Редактирует предыдущее сообщение для редактирования количества элементов в контейнере.
        /// </summary>
        /// <param name="session">Сессия, для которой нужно сделать перевод и отправку сообщения.</param>
        /// <param name="chatId">ID чата.</param>
        /// <param name="messageId">ID сообщения</param>
        /// <returns>Возвращает <see cref="Task"/> с редактированием сообщения.</returns>
        public async Task EditMessage(Session session, ChatId chatId, int messageId)
        {
            PrepareMessage(session, out string text, out InlineKeyboardMarkup inlineKeyboard);

            await session.BotClient.EditMessageTextAsync(chatId, messageId,
                text,
                ParseMode.Markdown,
                true,
                inlineKeyboard);
        }

        public int this[MetaValued<T> key]
        {
            get => mainDict[key];

            set
            {
                if (value <= 0)
                {
                    translatingDict.Remove(key.ToString().GetHashCode());
                    mainDict.Remove(key);
                }
                else
                {
                    mainDict[key] = value;
                    translatingDict[key.ToString().GetHashCode()] = key;
                }
            }
        }

        public int this[int hashCode]
        {
            get => mainDict[translatingDict[hashCode]];

            set
            {
                if (value <= 0)
                {
                    mainDict.Remove(translatingDict[hashCode]);
                    translatingDict.Remove(hashCode);
                }
                else
                {
                    mainDict[translatingDict[hashCode]] = value;
                }
            }
        }

        public void Add(MetaValued<T> key, int value)
        {
            if (mainDict.ContainsKey(key))
            {
                mainDict[key] += value;
            }
            else
            {
                mainDict.Add(key, value);
                translatingDict.Add(key.ToString().GetHashCode(), key);
            }

            if (mainDict[key] <= 0)
            {
                mainDict.Remove(key);
                translatingDict.Remove(key.ToString().GetHashCode());
            }
        }

        public void Add(KeyValuePair<MetaValued<T>, int> item) =>
            Add(item.Key, item.Value);

        public void Clear()
        {
            mainDict.Clear();
            translatingDict.Clear();
        }

        public bool Contains(KeyValuePair<MetaValued<T>, int> item) => mainDict.Contains(item);

        public bool ContainsKey(MetaValued<T> key) => mainDict.ContainsKey(key);

        public bool ContainsKey(string key) => translatingDict.ContainsKey(key.GetHashCode());

        public bool ContainsKey(int hashCode) => translatingDict.ContainsKey(hashCode);

        public void CopyTo(KeyValuePair<MetaValued<T>, int>[] array, int arrayIndex) =>
            ((IDictionary<MetaValued<T>, int>)mainDict).CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<MetaValued<T>, int>> GetEnumerator() => mainDict.GetEnumerator();

        public bool Remove(MetaValued<T> key) =>
            mainDict.Remove(key) & translatingDict.Remove(key.ToString().GetHashCode());

        public bool Remove(KeyValuePair<MetaValued<T>, int> item) => Remove(item.Key);

        public bool TryGetValue(MetaValued<T> key, out int value) => mainDict.TryGetValue(key, out value);

        public bool TryGetValue(string key, out int value) =>
            translatingDict.TryGetValue(key.GetHashCode(), out var metaValued) & mainDict.TryGetValue(metaValued, out value);

        public bool TryGetValue(int hashCode, out int value) =>
            translatingDict.TryGetValue(hashCode, out var metaValued) & mainDict.TryGetValue(metaValued, out value);

        IEnumerator IEnumerable.GetEnumerator() => mainDict.GetEnumerator();
    }
}
