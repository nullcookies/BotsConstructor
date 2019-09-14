using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace LogicalCore
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

        public override void SetParent(Node parent)
        {
            base.SetParent(parent);
            if (needBackButton) message.InsertBackButton(parent);
        }

		public override void SetBackLink(Node parent)
		{
			bool needAddBack = needBackButton && Parent == null && parent != null;
			base.SetBackLink(parent);
			if (needAddBack) message.InsertBackButton(parent);
		}

		internal override async Task<Message> SendReplyMarkup(Session session)
        {
            Task<Message> task = base.SendReplyMarkup(session);
            return await task.ContinueWith((prevTask) =>
            {
                session.CurrentNode = this;
                return prevTask.Result;
            },
            TaskContinuationOptions.None);
        }

        protected bool TryGoBack(Session session, Message message)
        {
            if (KeyboardActionsManager.CheckNeeding(needBackButton, this.message.HaveReplyKeyboard, session, message, DefaultStrings.BACK))
            {
                session.GoToNode(Parent);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool TryGoBack(Session session, CallbackQuery callbackQuerry)
        {
            if (KeyboardActionsManager.CheckNeeding(needBackButton, message.HaveInlineKeyboard, session, callbackQuerry, DefaultStrings.BACK, () =>
                ButtonIdManager.GetIDFromCallbackData(callbackQuerry.Data) == Parent.id))
            {
                session.GoToNode(Parent);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual void GoToChildNode(Session session, Node child) => session.GoToNode(child);

        protected virtual bool TryGoToChild(Session session, Message message)
        {
            foreach (Node child in Children)
            {
                if (KeyboardActionsManager.CheckNeeding(this.message.MetaKeyboard?.CanShowButton(child.name, session) ?? false,
                    this.message.HaveReplyKeyboard, session, message, child.name))
                {
                    GoToChildNode(session, child);
                    return true;
                }
            }

            return false;
        }

        protected virtual bool TryGoToChild(Session session, CallbackQuery callbackQuerry)
        {
            int childID = ButtonIdManager.GetIDFromCallbackData(callbackQuerry.Data);
            if (Children.Find((node) => node.id == childID) is Node child &&
                KeyboardActionsManager.CheckNeeding(message.MetaKeyboard?.CanShowButton(child.name, session) ?? false,
                    message.HaveInlineKeyboard, session, callbackQuerry, child.name))
            {
                GoToChildNode(session, child);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override bool TryFilter(Session session, Message message) =>
            base.TryFilter(session, message) || TryGoBack(session, message) || TryGoToChild(session, message);

        protected override bool TryFilter(Session session, CallbackQuery callbackQuerry) =>
            base.TryFilter(session, callbackQuerry) || TryGoBack(session, callbackQuerry) || TryGoToChild(session, callbackQuerry);
    }
}
