using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace LogicalCore
{
    /// <summary>
    /// Узел, отправляющий страницы детей блоками сообщений.
    /// </summary>
    public class BlockNode : CollectionNode<Node>
    {
        public BlockNode(string name, IMetaMessage metaMessage = null, byte pageSize = 2,
            bool needBack = true, bool needPrevious = false) : base(name, pageSize, metaMessage, null, needBack)
        {
            Children = collection;
            message.AddNextButton();
            NeedPreviousButton = needPrevious;
            if (NeedPreviousButton) message.InsertPreviousButton();
        }

        public BlockNode(string name, string description, byte pageSize = 2,
            bool needBack = true, bool needPrevious = false) :
            this(name, new MetaMessage(description), pageSize, needBack, needPrevious) { }

        protected override void AddChild(Node child)
        {
            if (!(child is LightNode))
			{
				LightNode middleNode = new LightNode(child.name, new MetaInlineMessage(child.Text, child.MessageType, child.File));
				base.AddChild(middleNode);
				middleNode.SetBackLink(this);
				middleNode.AddChildWithButtonRules(child);
				ConsoleWriter.WriteLine($"Создан лёгкий узел-посредник для узла {child.name}", ConsoleColor.DarkYellow);
			}
			else
			{
				base.AddChild(child);
			}
        }

        internal override async Task<Message> SendReplyMarkup(Session session)
        {
            Task<Message> task = base.SendReplyMarkup(session);
            return await task.ContinueWith((prevTask) =>
            {
                SendBlock(session);
                return prevTask.Result;
            },
            TaskContinuationOptions.NotOnFaulted);
        }
        
        private async void SendBlock(Session session, bool goForward = true)
        {
            GetStartFinish(session, goForward, out int start, out int finish);

            session.BlockNodePosition = finish;

            for (int i = start; i < finish; i++)
            {
                await Children[i].SendReplyMarkup(session);
            }
        }

        protected override void SendNext(Session session, Message divisionMessage = null) => SendBlock(session, true);

        protected override void SendPrevious(Session session, Message divisionMessage = null) => SendBlock(session, false);
    }
}
