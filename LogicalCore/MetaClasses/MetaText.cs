using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicalCore
{
    /// <summary>
    /// Класс для хранения непереведённых данных и их удобного перевода.
    /// </summary>
    public class MetaText : ISessionTranslatable, IEnumerable<string>, IEquatable<MetaText>, IClearable
	{
        private readonly Queue<string> parts;

        public MetaText()
        {
            parts = new Queue<string>();
        }

        public MetaText(IEnumerable<string> collection)
        {
            parts = new Queue<string>(collection);
        }

        public MetaText(params string[] parts) : this()
        {
            Append(parts);
        }

        public MetaText(params object[] parts) : this()
        {
            Append(parts);
        }

        public void AppendLine()
        {
            parts.Enqueue("\n");
        }

        public void Append(string part)
        {
            parts.Enqueue(part);
        }

        public static MetaText operator +(MetaText metaText, string part)
        {
            metaText.Append(part);
            return metaText;
        }

        public void Append<T>(T part)
        {
            parts.Enqueue(part.ToString());
        }

        public static MetaText operator +(MetaText metaText, object part)
        {
            metaText.Append(part);
            return metaText;
        }

        public void Append(params string[] parts)
        {
            foreach (var part in parts)
            {
                this.parts.Enqueue(part);
            }
        }

        public void Append<T>(params T[] parts)
        {
            foreach (var part in parts)
            {
                this.parts.Enqueue(part.ToString());
            }
        }

        public void Append(IEnumerable<string> parts)
        {
            foreach (var part in parts)
            {
                this.parts.Enqueue(part);
            }
        }

        public static MetaText operator +(MetaText metaText, IEnumerable<string> part)
        {
            metaText.Append(part);
            return metaText;
        }

        public void Append(MetaText part)
        {
            Append((IEnumerable<string>)part);
        }

        public static MetaText operator +(MetaText metaText, MetaText part)
        {
            metaText.Append(part);
            return metaText;
        }

        /// <summary>
        /// Возвращает непереведённую строку из всех частей.
        /// </summary>
        /// <returns>Возвращает непереведённую строку из всех частей.</returns>
        public override string ToString() => string.Concat(parts);
        /// <summary>
        /// Переводит текст на основе полученной сессии.
        /// </summary>
        /// <param name="session">Сессия, язык которой необходимо использовать.</param>
        /// <returns>Возвращает переведённый текст.</returns>
        public string ToString(Session session)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (string part in parts)
			{
				stringBuilder.Append(session.Translate(part));
			}

            return stringBuilder.ToString();
        }

		public void Clear() => parts.Clear();

        public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)parts).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<string>)parts).GetEnumerator();

        public static implicit operator MetaText(string text) => new MetaText(text);
        public static implicit operator MetaText(string[] text) => new MetaText(text);
        public static implicit operator MetaText(object[] parts) => new MetaText(parts);

        public override bool Equals(object obj)
        {
            if (!(obj is MetaText other)) return false;
            return parts.SequenceEqual(other.parts);
        }

        public bool Equals(MetaText other)
        {
            return other != null &&
                   EqualityComparer<Queue<string>>.Default.Equals(parts, other.parts);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
