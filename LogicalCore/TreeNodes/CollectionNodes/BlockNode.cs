using System;
using System.Threading.Tasks;
using LogicalCore.TreeNodes;
using Telegram.Bot.Types;

namespace LogicalCore
{
    /// <summary>
    /// Узел, отправляющий страницы детей блоками сообщений.
    /// </summary>
    public class BlockNode : CollectionNode<ITreeNode>
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

        protected override void AddChild(ITreeNode child)
        {
            if (!(child is LightNode))
			{
				LightNode middleNode = new LightNode(child.Name, new MetaInlineMessage(child.Text, child.MessageType, child.File));
				base.AddChild(middleNode);
				middleNode.SetBackLink(this);
				middleNode.AddChildWithButtonRules(child);
				ConsoleWriter.WriteLine($"Создан лёгкий узел-посредник для узла {child.Name}", ConsoleColor.DarkYellow);
			}
			else
			{
				base.AddChild(child);
			}
        }

        public override async Task<Message> SendMessage(Session session)
        {
            Task<Message> task = base.SendMessage(session);
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
                await Children[i].SendMessage(session);
            }
        }

        protected override void SendNext(Session session, Message divisionMessage = null) => SendBlock(session, true);

        protected override void SendPrevious(Session session, Message divisionMessage = null) => SendBlock(session, false);
    }
}
