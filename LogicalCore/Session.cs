using System;
using System.Threading.Tasks;
using LogicalCore.TreeNodes;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace LogicalCore
{
    public class Session : ISession
    {
        public int TelegramId { get; }
        public IVariablesContainer Vars { get; }
		public string Me => Vars.GetVar<string>("Me");
        public Func<string, string> Translate { get; private set; }
		private string language;
		public string Language
		{
			get => language;
			set
			{
				language = value;
				Translate = TMM.GetTranslator(language);
			}
		}
		public IExtendedBot BotWrapper { get; }
		public BotOwner BotOwner => BotWrapper.BotOwner;
        public ITelegramBotClient BotClient => BotWrapper.BotClient;
        public IMarkupTree MarkupTree => BotWrapper.MarkupTree;
        private ITextMessagesManager TMM => BotWrapper.TMM;
        public IGlobalFilter GlobalFilter => BotWrapper.GlobalFilter;
        private ITreeNode currentNode;
        public ITreeNode CurrentNode
        {
            get => currentNode;
            set
            {
                currentNode = value;
                BlockNodePosition = 0;
            }
        }

        public User User { get; private set; }
        public CallbackQuery LastCallback { get; private set; }
        public Message LastMessage { get; private set; }

        public int BlockNodePosition { get; set; }

        public Session(ITreeNode node, int id, IExtendedBot wrapper)
        {
            CurrentNode = node;
            TelegramId = id;
            BotWrapper = wrapper;
			Language = TMM.DefaultLanguage;
            Vars = new VariablesContainer();
			BotWrapper.InitializeSessionVars?.Invoke(Vars);
            User = BotClient.GetChatMemberAsync(id, id).Result.User;
			Vars.SetVar("Me", $"[{User.FirstName} {User.LastName}](tg://user?Id={User.Id})\n");
        }

       // public Session(int Id, BotWrapper botWrapper) : this(botWrapper.MarkupTree.root, Id, botWrapper) { }

        public void TakeControl(Message message)
        {
            LastMessage = message;
			if(User != message.From)
			{
				ConsoleWriter.WriteLine($"Пользователь '{User.FirstName} {User.LastName}' изменил свои данные.");
				User = message.From;
				Vars.SetVar("Me", $"[{User.FirstName} {User.LastName}](tg://user?Id={User.Id})\n");
			}
            CurrentNode.TakeControl(this, message);
        }

        public void TakeControl(CallbackQuery callbackQuery)
        {
            LastCallback = callbackQuery;
			if (User != callbackQuery.From)
			{
				ConsoleWriter.WriteLine($"Пользователь '{User.FirstName} {User.LastName}' изменил свои данные.");
				User = callbackQuery.From;
				Vars.SetVar("Me", $"[{User.FirstName} {User.LastName}](tg://user?Id={User.Id})\n");
			}
			CurrentNode.TakeControl(this, callbackQuery);
        }

        /// <summary>
        /// Позволяет отправить разметку узла текущей сессии и перейти на него, если это возможно.
        /// </summary>
        /// <param name="node">Узел, показ которого необходимо выполнить.</param>
        public async void GoToNode(ISendingMessage node) => await node.SendMessage(this);

        /// <summary>
        /// Позволяет отправить разметку узла текущей сессии и перейти на него, если это возможно.
        /// </summary>
        /// <param name="node">Узел, показ которого необходимо выполнить.</param>
        /// <param name="msgTask">Task отправки сообщения.</param>
        public void GoToNode(ISendingMessage node, out Task<Message> msgTask) => msgTask = node.SendMessage(this);

        public string Retranslate(string text) => TMM.GetKeyFromTextIfExists(text);

        public bool TryRetranslate(string text, out string key) => TMM.TryGetKeyFromTextIfExists(text, out key);
    }
}