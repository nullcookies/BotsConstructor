using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace LogicalCore.TreeNodes
{
    /// <summary>
    /// Нормальный узел, на который выполняется переход.
    /// </summary>
    public abstract class NormalNode : Node
    {
        public readonly bool needBackButton;

        public NormalNode(string name, IMetaMessage metaMessage, bool needBack) : base(name, metaMessage)
        {
            needBackButton = needBack;
        }

        public NormalNode(string name, string description, bool needBack) : this(name, new MetaMessage(description), needBack) { }

        public override void SetParent(ITreeNode parent)
        {
            base.SetParent(parent);
            if (needBackButton) message.InsertBackButton(parent);
        }

		public override void SetBackLink(ITreeNode parent)
		{
			bool needAddBack = needBackButton && Parent == null && parent != null;
			base.SetBackLink(parent);
			if (needAddBack) message.InsertBackButton(parent);
		}

		public override async Task<Message> SendMessage(ISession session)
        {
            Task<Message> task = base.SendMessage(session);
            return await task.ContinueWith((prevTask) =>
            {
                session.CurrentNode = this;
                return prevTask.Result;
            },
            TaskContinuationOptions.NotOnFaulted);
        }

        protected bool TryGoBack(ISession session, Message message)
        {
            if (KeyboardActionsManager.CheckNeeding(needBackButton, this.message.HaveReplyKeyboard, session, message, DefaultStrings.Back))
            {
                session.GoToNode(Parent);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool TryGoBack(ISession session, CallbackQuery callbackQuerry)
        {
            if (KeyboardActionsManager.CheckNeeding(needBackButton, message.HaveInlineKeyboard, session, callbackQuerry, DefaultStrings.Back, () =>
                ButtonIdManager.GetIDFromCallbackData(callbackQuerry.Data) == Parent.Id))
            {
                session.GoToNode(Parent);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual void GoToChildNode(ISession session, ITreeNode child) => session.GoToNode(child);

        protected virtual bool TryGoToChild(ISession session, Message message)
        {
            foreach (var child in Children)
            {
                if (KeyboardActionsManager.CheckNeeding(this.message.MetaKeyboard?.CanShowButton(child.Name, session) ?? false,
                    this.message.HaveReplyKeyboard, session, message, child.Name))
                {
                    GoToChildNode(session, child);
                    return true;
                }
            }

            return false;
        }

        protected virtual bool TryGoToChild(ISession session, CallbackQuery callbackQuerry)
        {
            int childID = ButtonIdManager.GetIDFromCallbackData(callbackQuerry.Data);
            if (Children.Find((node) => node.Id == childID) is Node child &&
                KeyboardActionsManager.CheckNeeding(message.MetaKeyboard?.CanShowButton(child.Name, session) ?? false,
                    message.HaveInlineKeyboard, session, callbackQuerry, child.Name))
            {
                GoToChildNode(session, child);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override bool TryFilter(ISession session, Message message) =>
            base.TryFilter(session, message) || TryGoBack(session, message) || TryGoToChild(session, message);

        protected override bool TryFilter(ISession session, CallbackQuery callbackQuerry) =>
            base.TryFilter(session, callbackQuerry) || TryGoBack(session, callbackQuerry) || TryGoToChild(session, callbackQuerry);
    }
}
