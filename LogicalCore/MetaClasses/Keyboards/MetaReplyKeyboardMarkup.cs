using System;
using System.Collections.Generic;
using System.Linq;
using LogicalCore.TreeNodes;
using Telegram.Bot.Types.ReplyMarkups;

namespace LogicalCore
{
    public class MetaReplyKeyboardMarkup : MetaKeyboardMarkup<KeyboardButton>
    {
        public override bool HaveReplyKeyboard => true;

        public override bool HaveInlineKeyboard => false;

        public MetaReplyKeyboardMarkup(List<List<(KeyboardButton button, List<Predicate<Session>> rules)>> buttons) : base (buttons) { }
        public MetaReplyKeyboardMarkup(int rowsCount = 2) : base(rowsCount) { }

		public override MetaKeyboardMarkup<KeyboardButton> Clone()
		{
			var newButtons = new List<List<(KeyboardButton button, List<Predicate<Session>> rules)>>(buttons.Count);
			foreach (var row in buttons)
			{
				var newRow = new List<(KeyboardButton button, List<Predicate<Session>> rules)>(row.Count);
				newRow.AddRange(row);
				newButtons.Add(newRow);
			}
			return new MetaReplyKeyboardMarkup(newButtons);
		}

		public override void AddNodeButton(ITreeNode node, params Predicate<Session>[] rules)
        {
            var (button, rulesList) = buttons.SelectMany((list) => list).FirstOrDefault((btnTuple) => btnTuple.button.Text == node.Name);
            if (button != null)
            {
                rulesList.AddRange(rules);
            }
            else
            {
                AddButton(new KeyboardButton(node.Name), rules);
            }
        }

		public override void AddNodeButton(int rowNumber, ITreeNode node, params Predicate<Session>[] rules) =>
            AddButton(rowNumber, new KeyboardButton(node.Name), rules);

        public override void InsertNodeButton(int rowNumber, int columnNumber, ITreeNode node, params Predicate<Session>[] rules) =>
            InsertButton(rowNumber, columnNumber, new KeyboardButton(node.Name), rules);

        public override void InsertBackButton(ITreeNode parent, int rowNumber = 0, int columnNumber = 0) =>
            InsertButton(rowNumber, columnNumber, new KeyboardButton(DefaultStrings.Back));

        public override void AddNextButton(int rowNumber = 1) =>
            AddButton(rowNumber, new KeyboardButton(DefaultStrings.Next));

        public override void InsertPreviousButton(int rowNumber = 1, int columnNumber = 0) =>
            InsertButton(rowNumber, columnNumber, new KeyboardButton(DefaultStrings.Previous));

        public override void AddSpecialButton(string name, params Predicate<Session>[] rules) =>
            AddButton(new KeyboardButton(name), rules);

        public override void AddSpecialButton(int rowNumber, string name, params Predicate<Session>[] rules) =>
            AddButton(rowNumber, new KeyboardButton(name), rules);

        public override void InsertSpecialButton(int rowNumber, int columnNumber, string name, params Predicate<Session>[] rules) =>
            InsertButton(rowNumber, columnNumber, new KeyboardButton(name), rules);

        public override IReplyMarkup Translate(Session session) => TranslateMarkup(session);

        public ReplyKeyboardMarkup TranslateMarkup(Session session)
        {
            KeyboardButton[][] translatedButtons = new KeyboardButton[buttons.Count][];

            for (int i = 0, j = 0; i < buttons.Count; i++, j = 0)
            {
                // Поиск кнопок, для которых выполняются все правила
                var neededButtons = buttons[i].Where((button) => button.rules.TrueForAll((rule) => rule(session)));
                translatedButtons[i] = new KeyboardButton[neededButtons.Count()];
                foreach (var (button, rules) in neededButtons)
                {
                    // Добавляем переведённые кнопки
                    translatedButtons[i][j++] = new KeyboardButton(session.Translate(button.Text));
                }
            }

            return new ReplyKeyboardMarkup(translatedButtons);
        }
    }
}
