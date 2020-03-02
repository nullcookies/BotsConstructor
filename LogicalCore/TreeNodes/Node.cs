using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace LogicalCore.TreeNodes
{
    public abstract class Node : ITreeNode
    {
        private static int nextID = 0;
        public int Id { get; }
        public string Name { get; }
        public ITreeNode Parent { get; private set; }
        public List<ITreeNode> Children { get; protected set; }
        protected IMetaMessage message;
		public MessageType MessageType => message.MessageType;
		public ISessionTranslatable Text => message.Text;
		public InputOnlineFile File => message.File;

		public Node(string name, IMetaMessage metaMessage)
        {
            Id = nextID++; // получать из БД
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            this.Name = name;
            Children = new List<ITreeNode>();
            message = metaMessage ?? new MetaMessage(name, ". ");
        }

        public Node(string name, string description) : this(name, description == null ? null : new MetaMessage(description)) { }

        public void SetButtonsLocation(ElementsLocation locationType) => message.MetaKeyboard.SetButtonsLocation(locationType);

        public virtual void AddChildWithButtonRules(ITreeNode child, params Predicate<Session>[] rules)
        {
            child.SetParent(this);
            message.AddNodeButton(child, rules);
        }

        void ITreeNode.AddChild(ITreeNode child) => AddChild(child);

        protected virtual void AddChild(ITreeNode child)
        {
            Children.Add(child);
		}

		public virtual void SetParent(ITreeNode parent)
        {
            if (Parent != null) throw new NotSupportedException("У узла не может быть 2 родителя.");
            Parent = parent;
            parent.AddChild(this);
        }

		public virtual void SetBackLink(ITreeNode parent)
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

        public bool CanExecute(string action, Session session) => message.MetaKeyboard.CanShowButton(action, session);

        public virtual async Task<Message> SendMessage(Session session) => await message.SendMessage(session);

        public void TakeControl(Session session, Message message)
        {
            MandatoryActions(session, message);

            if (!TryFilter(session, message)) session.GlobalFilter.Filter(session, message);
        }

        public void TakeControl(Session session, CallbackQuery callbackQuery)
        {
            MandatoryActions(session, callbackQuery);

            if (!TryFilter(session, callbackQuery)) session.GlobalFilter.Filter(session, callbackQuery);
            //Потенциально опасное место
            session.BotClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        }

        //Обязательные действия, которые должны выполняться всегда при TakeControl

        protected virtual void MandatoryActions(Session session, Message message)
        {
            string nameOfUser = message.From.FirstName + " " + message.From.LastName;

            ConsoleWriter.WriteLine($"Пользователь '{nameOfUser}' покинул узел '{Name}'.", ConsoleColor.Gray);
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
