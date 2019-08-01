using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace LogicalCore
{
    public abstract class Node
    {
        private static int nextID = 0;
        public readonly int id;
        public readonly string name;
        public Node Parent { get; private set; }
        public List<Node> Children { get; protected set; }
        protected IMetaMessage message;
		public MessageType MessageType => message.Type;
		public MetaText Text => message.Text;
		public InputOnlineFile File => message.File;

		public Node(string name, IMetaMessage metaMessage)
        {
            id = nextID++; // получать из БД
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            this.name = name;
            Children = new List<Node>();
            message = metaMessage ?? new MetaMessage(name, ". ");
        }

        public Node(string name, string description) : this(name, description == null ? null : new MetaMessage(description)) { }

        public void SetButtonsLocation(ElementsLocation locationType) => message.MetaKeyboard.SetButtonsLocation(locationType);

        public virtual void AddChildWithButtonRules(Node child, params Predicate<Session>[] rules)
        {
            child.SetParent(this);
            message.AddNodeButton(child, rules);
        }

        protected virtual void AddChild(Node child)
        {
            Children.Add(child);
		}

		public virtual void SetParent(Node parent)
        {
            if (Parent != null) throw new NotSupportedException("У узла не может быть 2 родителя.");
            Parent = parent;
            parent.AddChild(this);
        }

		public virtual void SetBackLink(Node parent)
		{
			if (Parent != null) ConsoleWriter.WriteLine("Родитель узла был заменён на иную обратную ссылку.", ConsoleColor.Yellow);
			Parent = parent;
		}

		public void AddSpecialButton(string name, params Predicate<Session>[] rules) =>
            message.MetaKeyboard.AddSpecialButton(name, rules);

        public void AddSpecialButton(int rowNumber, string name, params Predicate<Session>[] rules) =>
            message.MetaKeyboard.AddSpecialButton(rowNumber, name, rules);

        public void InsertSpecialButton(int rowNumber, int columnNumber, string name, params Predicate<Session>[] rules) =>
            message.MetaKeyboard.InsertSpecialButton(rowNumber, columnNumber, name, rules);

        internal bool CanExecute(string action, Session session) => message.MetaKeyboard.CanShowButton(action, session);

        internal virtual async Task<Message> SendReplyMarkup(Session session) => await message.SendMessage(session);

        internal void TakeControl(Session session, Message message)
        {
            MandatoryActions(session, message);

            if (!TryFilter(session, message)) session.GlobalFilter.Filter(session, message);
        }

        internal void TakeControl(Session session, CallbackQuery callbackQuerry)
        {
            MandatoryActions(session, callbackQuerry);

            if (!TryFilter(session, callbackQuerry)) session.GlobalFilter.Filter(session, callbackQuerry);
            //Потенциально опасное место
            session.BotClient.AnswerCallbackQueryAsync(callbackQuerry.Id);
        }

        //Обязательные действия, которые должны выполняться всегда при TakeControl

        protected virtual void MandatoryActions(Session session, Message message)
        {
            string nameOfUser = message.From.FirstName + " " + message.From.LastName;

            ConsoleWriter.WriteLine($"Пользователь '{nameOfUser}' покинул узел '{name}'.", ConsoleColor.Gray);
        }

        protected virtual void MandatoryActions(Session session, CallbackQuery callbackQuerry)
        {
            string nameOfUser = callbackQuerry.From.FirstName + " " + callbackQuerry.From.LastName;

            ConsoleWriter.WriteLine($"Пользователь '{nameOfUser}' нажал на кнопку '{callbackQuerry.Data}'.", ConsoleColor.Gray);
        }

        //TryFilter - аналог FilterChain, возвращает true, если был выполнен, иначе false

        protected virtual bool TryFilter(Session session, Message message) => false;

        protected virtual bool TryFilter(Session session, CallbackQuery callbackQuerry) => false;
    }
}
