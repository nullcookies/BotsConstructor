using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace LogicalCore
{
    public class MetaInlineKeyboardMarkup : MetaKeyboardMarkup<InlineKeyboardButton>
    {
        public override bool HaveReplyKeyboard => false;

        public override bool HaveInlineKeyboard => true;

        public MetaInlineKeyboardMarkup(List<List<(InlineKeyboardButton button, List<Predicate<Session>> rules)>> buttons) : base(buttons) { }
        public MetaInlineKeyboardMarkup(int rowsCount = 1) : base(rowsCount) { }

		public override MetaKeyboardMarkup<InlineKeyboardButton> Clone()
		{
			var newButtons = new List<List<(InlineKeyboardButton button, List<Predicate<Session>> rules)>>(buttons.Count);
			foreach (var row in buttons)
			{
				var newRow = new List<(InlineKeyboardButton button, List<Predicate<Session>> rules)>(row.Count);
				newRow.AddRange(row);
				newButtons.Add(newRow);
			}
			return new MetaInlineKeyboardMarkup(newButtons);
		}

		public override void AddNodeButton(Node node, params Predicate<Session>[] rules)
        {
            var (button, rulesList) = buttons.SelectMany((list) => list).FirstOrDefault((btnTuple) => btnTuple.button.Text == node.name);
            if(button != null)
            {
                rulesList.AddRange(rules);
            }
            else
            {
                AddButton(InlineKeyboardButton.WithCallbackData(node.name, ButtonIdManager.GetInlineButtonId(node)), rules);
            }
		}

		public override void AddNodeButton(int rowNumber, Node node, params Predicate<Session>[] rules) =>
            AddButton(rowNumber, InlineKeyboardButton.WithCallbackData(node.name, ButtonIdManager.GetInlineButtonId(node)), rules);

        public override void InsertNodeButton(int rowNumber, int columnNumber, Node node, params Predicate<Session>[] rules) =>
            InsertButton(rowNumber, columnNumber, InlineKeyboardButton.WithCallbackData(node.name, ButtonIdManager.GetInlineButtonId(node)), rules);

        public override void InsertBackButton(Node parent, int rowNumber = 0, int columnNumber = 0) =>
            InsertButton(rowNumber, columnNumber, InlineKeyboardButton.WithCallbackData(DefaultStrings.Back, ButtonIdManager.GetInlineButtonId(parent)));

        public override void AddNextButton(int rowNumber = 1)
            => throw new NotSupportedException("Кнопка 'Next' должна добавляться из узла.");

        public override void InsertPreviousButton(int rowNumber = 1, int columnNumber = 0)
            => throw new NotSupportedException("Кнопка 'Previous' должна добавляться из узла.");

        public override void AddSpecialButton(string name, params Predicate<Session>[] rules) =>
            AddButton(InlineKeyboardButton.WithCallbackData(name), rules);

        public override void AddSpecialButton(int rowNumber, string name, params Predicate<Session>[] rules) =>
            AddButton(rowNumber, InlineKeyboardButton.WithCallbackData(name), rules);

        public override void InsertSpecialButton(int rowNumber, int columnNumber, string name, params Predicate<Session>[] rules) =>
            InsertButton(rowNumber, columnNumber, InlineKeyboardButton.WithCallbackData(name), rules);

        public override IReplyMarkup Translate(Session session) => TranslateMarkup(session);

        public InlineKeyboardMarkup TranslateMarkup(Session session)
        {
            InlineKeyboardButton[][] translatedButtons = new InlineKeyboardButton[buttons.Count][];

            for (int i = 0, j = 0; i < buttons.Count; i++, j = 0)
            {
                // Поиск кнопок, для которых выполняются все правила
                var neededButtons = buttons[i].Where((button) => button.rules.TrueForAll((rule) => rule(session)));
                translatedButtons[i] = new InlineKeyboardButton[neededButtons.Count()];
                foreach (var (button, rules) in neededButtons)
                {
                    // Добавляем переведённые кнопки
                    if (button.CallbackGame != null)
                    {
                        translatedButtons[i][j++] = InlineKeyboardButton.WithCallBackGame(session.Translate(button.Text), button.CallbackGame);
                    }
                    else if (button.Pay)
                    {
                        translatedButtons[i][j++] = InlineKeyboardButton.WithPayment(session.Translate(button.Text));
                    }
                    else if(!string.IsNullOrWhiteSpace(button.SwitchInlineQuery))
                    {
                        translatedButtons[i][j++] = InlineKeyboardButton.WithSwitchInlineQuery(session.Translate(button.Text), button.SwitchInlineQuery);
                    }
                    else if(!string.IsNullOrWhiteSpace(button.SwitchInlineQueryCurrentChat))
                    {
                        translatedButtons[i][j++] = InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(session.Translate(button.Text), button.SwitchInlineQueryCurrentChat);
                    }
                    else if(!string.IsNullOrWhiteSpace(button.Url))
                    {
                        translatedButtons[i][j++] = InlineKeyboardButton.WithUrl(session.Translate(button.Text), button.Url);
                    }
                    else
                    {
                        translatedButtons[i][j++] = InlineKeyboardButton.WithCallbackData(session.Translate(button.Text), button.CallbackData);
                    }
                }
            }

            return new InlineKeyboardMarkup(translatedButtons);
        }
    }
}
