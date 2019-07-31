using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace LogicalCore
{
    public class MetaReplyKeyboardMarkup : MetaReplyMarkup<KeyboardButton>
    {
        public override bool HaveReplyKeyboard => true;

        public override bool HaveInlineKeyboard => false;

        public MetaReplyKeyboardMarkup(List<List<(KeyboardButton button, List<Predicate<Session>> rules)>> buttons) : base (buttons) { }
        public MetaReplyKeyboardMarkup(int rowsCount = 2) : base(rowsCount) { }

        public override void AddNodeButton(Node node, params Predicate<Session>[] rules)
        {
            var (button, rulesList) = buttons.SelectMany((list) => list).FirstOrDefault((btnTuple) => btnTuple.button.Text == node.name);
            if (button != null)
            {
                rulesList.AddRange(rules);
            }
            else
            {
                AddButton(new KeyboardButton(node.name), rules);
            }
        }

        public override void AddNodeButton(int rowNumber, Node node, params Predicate<Session>[] rules) =>
            AddButton(rowNumber, new KeyboardButton(node.name), rules);

        public override void InsertNodeButton(int rowNumber, int columnNumber, Node node, params Predicate<Session>[] rules) =>
            InsertButton(rowNumber, columnNumber, new KeyboardButton(node.name), rules);

        public override void InsertBackButton(Node parent, int rowNumber = 0, int columnNumber = 0) =>
            InsertButton(rowNumber, columnNumber, new KeyboardButton(DefaultStrings.BACK));

        public override void AddNextButton(int rowNumber = 1) =>
            AddButton(rowNumber, new KeyboardButton(DefaultStrings.NEXT));

        public override void InsertPreviousButton(int rowNumber = 1, int columnNumber = 0) =>
            InsertButton(rowNumber, columnNumber, new KeyboardButton(DefaultStrings.PREVIOUS));

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
