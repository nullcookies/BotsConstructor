using System;

namespace LogicalCore
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="KeyboardType"></typeparam>
	public interface IMetaMessage<KeyboardType> : IMetaMessage where KeyboardType : class, IMetaReplyMarkup
    {
		/// <summary>
		/// Метаклавиатура указанного типа.
		/// </summary>
        new KeyboardType MetaKeyboard { get; }
    }

	/// <summary>
	/// Интерфейс метасообщений.
	/// </summary>
    public interface IMetaMessage : ISendingMessage
    {
		/// <summary>
		/// Метаклавиатура.
		/// </summary>
        IMetaReplyMarkup MetaKeyboard { get; }
		/// <summary>
		/// Содержит ли сообщение reply-клавиатуру.
		/// </summary>
		bool HaveReplyKeyboard { get; }
		/// <summary>
		/// Содержит ли сообщение inline-клавиатуру.
		/// </summary>
		bool HaveInlineKeyboard { get; }

		/// <summary>
		/// Добавляет кнопку для узла.
		/// </summary>
		/// <param name="node">Узел, для которого необходимо добавить кнопку.</param>
		/// <param name="rules">Список правил, при выполнении которых кнопка должна быть показана.</param>
		void AddNodeButton(Node node, params Predicate<Session>[] rules);

		/// <summary>
		/// Добавляет кнопку для узла в указанную строку.
		/// </summary>
		/// <param name="rowNumber">Строка, в которую необходимо добавить кнопку.</param>
		/// <param name="node">Узел, для которого необходимо добавить кнопку.</param>
		/// <param name="rules">Список правил, при выполнении которых кнопка должна быть показана.</param>
		void AddNodeButton(int rowNumber, Node node, params Predicate<Session>[] rules);

		/// <summary>
		/// Вставляет кнопку "Назад" в указанное место.
		/// </summary>
		/// <param name="parent">Родитель узла, для которого нужна кнопка.</param>
		/// <param name="rowNumber">Номер строки.</param>
		/// <param name="columnNumber">Номер столбца.</param>
		void InsertBackButton(Node parent, int rowNumber = 0, int columnNumber = 0);

		/// <summary>
		/// Добавляет кнопку "Дальше" в указанное место.
		/// </summary>
		/// <param name="rowNumber">Номер строки.</param>
		void AddNextButton(int rowNumber = 1);

		/// <summary>
		/// Вставляет кнопку "Предыдущие" в указанное место.
		/// </summary>
		/// <param name="rowNumber">Номер строки.</param>
		/// <param name="columnNumber">Номер столбца.</param>
		void InsertPreviousButton(int rowNumber = 1, int columnNumber = 0);
	}
}
