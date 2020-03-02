using System;
using LogicalCore.TreeNodes;
using Telegram.Bot.Types.ReplyMarkups;

namespace LogicalCore
{
    public interface IMetaReplyMarkup
    {
        /// <summary>
        /// Есть ли нижняя клавиатура.
        /// </summary>
        bool HaveReplyKeyboard { get; }

        /// <summary>
        /// Есть ли клавиатура в сообщении.
        /// </summary>
        bool HaveInlineKeyboard { get; }

        /// <summary>
        /// Выполняет перевод кнопок для указанной сессии.
        /// </summary>
        /// <param name="session">Сессия, для которой необходимо выполнить перевод.</param>
        /// <returns>Возвращает копию клавиатуры, только с переведённым текстом.</returns>
        IReplyMarkup Translate(ISession session);

		/// <summary>
		/// Клонирует данный объект.
		/// </summary>
		/// <returns>Возвращает клон объекта.</returns>
		IMetaReplyMarkup Clone();

		/// <summary>
		/// Добавляет специальную кнопку действия.
		/// </summary>
		/// <param name="name">Название действия и текст кнопки.</param>
		/// <param name="rules">Список правил, при выполнении которых кнопка должна быть показана.</param>
		void AddSpecialButton(string name, params Predicate<ISession>[] rules);

        /// <summary>
        /// Добавляет специальную кнопку действия в указанную строку.
        /// </summary>
        /// <param name="rowNumber">Строка, в которую необходимо добавить кнопку.</param>
        /// <param name="name">Название действия и текст кнопки.</param>
        /// <param name="rules">Список правил, при выполнении которых кнопка должна быть показана.</param>
        void AddSpecialButton(int rowNumber, string name, params Predicate<ISession>[] rules);

        /// <summary>
        /// Вставляет специальную кнопку действия в указанное место.
        /// </summary>
        /// <param name="rowNumber">Номер строки.</param>
        /// <param name="columnNumber">Номер столбца.</param>
        /// <param name="name">Название действия и текст кнопки.</param>
        /// <param name="rules">Список правил, при выполнении которых кнопка должна быть показана.</param>
        void InsertSpecialButton(int rowNumber, int columnNumber, string name, params Predicate<ISession>[] rules);

        /// <summary>
        /// Добавляет кнопку для узла.
        /// </summary>
        /// <param name="node">Узел, для которого необходимо добавить кнопку.</param>
        /// <param name="rules">Список правил, при выполнении которых кнопка должна быть показана.</param>
        void AddNodeButton(ITreeNode node, params Predicate<ISession>[] rules);

        /// <summary>
        /// Добавляет кнопку для узла в указанную строку.
        /// </summary>
        /// <param name="rowNumber">Строка, в которую необходимо добавить кнопку.</param>
        /// <param name="node">Узел, для которого необходимо добавить кнопку.</param>
        /// <param name="rules">Список правил, при выполнении которых кнопка должна быть показана.</param>
        void AddNodeButton(int rowNumber, ITreeNode node, params Predicate<ISession>[] rules);

        /// <summary>
        /// Вставляет кнопку для узла в указанное место.
        /// </summary>
        /// <param name="rowNumber">Номер строки.</param>
        /// <param name="columnNumber">Номер столбца.</param>
        /// <param name="node">Узел, для которого необходимо добавить кнопку.</param>
        /// <param name="rules">Список правил, при выполнении которых кнопка должна быть показана.</param>
        void InsertNodeButton(int rowNumber, int columnNumber, ITreeNode node, params Predicate<ISession>[] rules);

        /// <summary>
        /// Вставляет кнопку "Назад" в указанное место.
        /// </summary>
        /// <param name="parent">Родитель узла, для которого нужна кнопка.</param>
        /// <param name="rowNumber">Номер строки.</param>
        /// <param name="columnNumber">Номер столбца.</param>
        void InsertBackButton(ITreeNode parent, int rowNumber = 0, int columnNumber = 0);

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

        /// <summary>
        /// Выполняются ли все правила для данной кнопки в указанной сессии.
        /// </summary>
        /// <param name="buttonName">Название кнопки или её текст.</param>
        /// <param name="session">Сессия, для которой необходимо выполнить проверку.</param>
        /// <returns>Возвращает, можно ли показывать эту кнопку. Если кнопки нет, то false.</returns>
        bool CanShowButton(string buttonName, ISession session);

        /// <summary>
        /// Устанавливает новое положение кнопок.
        /// </summary>
        /// <param name="locationType">Необходимое положение кнопок.</param>
        void SetButtonsLocation(ElementsLocation locationType);
    }
}
