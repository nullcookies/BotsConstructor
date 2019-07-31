using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace LogicalCore
{
	public class Session
    {
        public int telegramId;
        public readonly VariablesContainer vars;
		public string Me => vars.GetVar<string>("Me");
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
		public BotWrapper BotWrapper { get; }
		public BotOwner BotOwner => BotWrapper.BotOwner;
        public TelegramBotClient BotClient => BotWrapper.BotClient;
        public MegaTree MegaTree => BotWrapper.MegaTree;
        private BaseTextMessagesManager TMM => BotWrapper.tmm;
        public GlobalFilter GlobalFilter => BotWrapper.globalFilter;
        private Node currentNode;
        public Node CurrentNode
        {
            get
            {
                return currentNode;
            }
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

        public Session(Node node, int id, BotWrapper wrapper)
        {
            CurrentNode = node;
            telegramId = id;
            BotWrapper = wrapper;
			Language = TMM.defaultLanguage;
            vars = new VariablesContainer();
			BotWrapper.InitializeSessionVars?.Invoke(vars);
            User = BotClient.GetChatMemberAsync(id, id).Result.User;
			vars.SetVar("Me", $"[{User.FirstName} {User.LastName}](tg://user?id={User.Id})\n");
        }

        public Session(int id, BotWrapper botWrapper) : this(botWrapper.MegaTree.root, id, botWrapper) { }

        public void TakeControl(Message message)
        {
            LastMessage = message;
			if(User != message.From)
			{
				ConsoleWriter.WriteLine($"Пользователь '{User.FirstName} {User.LastName}' изменил свои данные.");
				User = message.From;
				vars.SetVar("Me", $"[{User.FirstName} {User.LastName}](tg://user?id={User.Id})\n");
			}
            CurrentNode.TakeControl(this, message);
        }

        public void TakeControl(CallbackQuery callbackQuerry)
        {
            LastCallback = callbackQuerry;
			if (User != callbackQuerry.From)
			{
				ConsoleWriter.WriteLine($"Пользователь '{User.FirstName} {User.LastName}' изменил свои данные.");
				User = callbackQuerry.From;
				vars.SetVar("Me", $"[{User.FirstName} {User.LastName}](tg://user?id={User.Id})\n");
			}
			CurrentNode.TakeControl(this, callbackQuerry);
        }

        /// <summary>
        /// Позволяет отправить разметку узла текущей сессии и перейти на него, если это возможно.
        /// </summary>
        /// <param name="node">Узел, показ которого необходимо выполнить.</param>
        public async void GoToNode(Node node) => await node.SendReplyMarkup(this);

        /// <summary>
        /// Позволяет отправить разметку узла текущей сессии и перейти на него, если это возможно.
        /// </summary>
        /// <param name="node">Узел, показ которого необходимо выполнить.</param>
        /// <param name="msgTask">Task отправки сообщения.</param>
        public void GoToNode(Node node, out Task<Message> msgTask) => msgTask = node.SendReplyMarkup(this);

        public string Retranslate(string text) => TMM.GetKeyFromTextIfExists(text);

        public bool TryRetranslate(string text, out string key) => TMM.TryGetKeyFromTextIfExists(text, out key);
    }
}