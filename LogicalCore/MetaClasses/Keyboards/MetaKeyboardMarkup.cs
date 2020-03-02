using System;
using System.Collections.Generic;
using System.Linq;
using LogicalCore.TreeNodes;
using Telegram.Bot.Types.ReplyMarkups;

namespace LogicalCore
{
    public abstract class MetaKeyboardMarkup<ButtonType> : IMetaReplyMarkup where ButtonType : IKeyboardButton
    {
        protected readonly List<List<(ButtonType button, List<Predicate<Session>> rules)>> buttons;

        public abstract bool HaveReplyKeyboard { get; }
		
        public abstract bool HaveInlineKeyboard { get; }

        public MetaKeyboardMarkup(List<List<(ButtonType button, List<Predicate<Session>> rules)>> buttons)
        {
            this.buttons = buttons ?? new List<List<(ButtonType button, List<Predicate<Session>> rules)>>();
			for(int i = 0; i < buttons.Count; i++)
			{
				var row = buttons[i];
				for(int j = 0; j < row.Count; j++)
				{
					var (button, rules) = row[j];
					if (rules == null) row[j] = (button, new List<Predicate<Session>>(0));
				}
			}
        }

        public MetaKeyboardMarkup(int rowsCount)
        {
            buttons = new List<List<(ButtonType button, List<Predicate<Session>> rules)>>();
            for(int i = 0; i < rowsCount; i++)
            {
                buttons.Add(new List<(ButtonType button, List<Predicate<Session>> rules)>());
            }
        }

        public void SetButtonsLocation(ElementsLocation locationType) => LocationManager.SetElementsLocation(locationType, buttons);

        public abstract IReplyMarkup Translate(Session session);

		public abstract MetaKeyboardMarkup<ButtonType> Clone();

		IMetaReplyMarkup IMetaReplyMarkup.Clone() => Clone();

        public abstract void AddSpecialButton(string name, params Predicate<Session>[] rules);

        public abstract void AddSpecialButton(int rowNumber, string name, params Predicate<Session>[] rules);

        public abstract void InsertSpecialButton(int rowNumber, int columnNumber, string name, params Predicate<Session>[] rules);

        public abstract void AddNodeButton(ITreeNode node, params Predicate<Session>[] rules);

        public abstract void AddNodeButton(int rowNumber, ITreeNode node, params Predicate<Session>[] rules);

        public abstract void InsertNodeButton(int rowNumber, int columnNumber, ITreeNode node, params Predicate<Session>[] rules);

        public abstract void InsertBackButton(ITreeNode parent, int rowNumber = 0, int columnNumber = 0);

        public abstract void AddNextButton(int rowNumber = 1);

        public abstract void InsertPreviousButton(int rowNumber = 1, int columnNumber = 0);

        public bool CanShowButton(string buttonName, Session session)
        {
            var (button, rules) = buttons.SelectMany((list) => list).FirstOrDefault((btnTuple) => btnTuple.button.Text == buttonName);
            if (button == null) return false;
            return rules.All((rule) => rule(session));
        }

        public void AddButton(ButtonType button, params Predicate<Session>[] rules)
        {
            var rulesList = new List<Predicate<Session>>(rules);
            if (buttons.Count == 0)
            {
                buttons.Add(new List<(ButtonType button, List<Predicate<Session>> rules)> { (button, rulesList) });
            }
            else
            {
                List<(ButtonType button, List<Predicate<Session>> rules)> minimumRow = buttons[0];
                int minCount = buttons[0].Count;
                // поиск строки с наименьшим количеством кнопок
                for(int i = 1; i < buttons.Count; i++)
                {
                    int count = buttons[i].Count;
                    if (count < minCount)
                    {
                        minimumRow = buttons[i];
                        minCount = count;
                    }
                }
                minimumRow.Add((button, rulesList));
            }
        }

        /// <summary>
        /// Добавляет кнопку в строку с указанным номером.
        /// </summary>
        /// <param name="rowNumber">Номер строки.</param>
        /// <param name="button">Кнопка, которую необходимо добавить.</param>
        /// <param name="rules">Список правил, при выполнении которых кнопка должна быть показана.</param>
        public void AddButton(int rowNumber, ButtonType button, params Predicate<Session>[] rules)
        {
            var rulesList = new List<Predicate<Session>>(rules);
            if (rowNumber < 0) throw new ArgumentOutOfRangeException(nameof(rowNumber), "Аргумент был меньше нуля.");
            // Добавление строк до необходимой, если нужно.
            while (rowNumber >= buttons.Count) buttons.Add(new List<(ButtonType button, List<Predicate<Session>> rules)>());
            buttons[rowNumber].Add((button, rulesList));
        }

        /// <summary>
        /// Вставляет кнопку в указанное место.
        /// </summary>
        /// <param name="rowNumber">Номер строки.</param>
        /// <param name="columnNumber">Номер столбца.</param>
        /// <param name="button">Кнопка, которую необходимо добавить.</param>
        /// <param name="rules">Список правил, при выполнении которых кнопка должна быть показана.</param>
        public void InsertButton(int rowNumber, int columnNumber, ButtonType button, params Predicate<Session>[] rules)
        {
            var rulesList = new List<Predicate<Session>>(rules);
            if (rowNumber < 0) throw new ArgumentOutOfRangeException(nameof(rowNumber), "Аргумент был меньше нуля.");
            if (columnNumber < 0) throw new ArgumentOutOfRangeException(nameof(columnNumber), "Аргумент был меньше нуля.");
            // Добавление строк до необходимой, если нужно.
            while (rowNumber >= buttons.Count) buttons.Add(new List<(ButtonType button, List<Predicate<Session>> rules)>());
            buttons[rowNumber].Insert(columnNumber, (button, rulesList));
        }
    }
}
