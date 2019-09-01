using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace LogicalCore
{
    public class ActionNode : Node, ITeleportable
	{
        private readonly Action<Session> action;

        public ActionNode(Action<Session> nodeAction, MetaInlineMessage metaMessage = null, string name = null)
            : base(name ?? DefaultStrings.NEXT, metaMessage)
        {
            Children = new List<Node>(1);
            message = metaMessage;
            action = nodeAction ?? ((session) => { });
        }

        public ActionNode(Action<Session> nodeAction, string description, string name = null)
			: this(nodeAction, description == null ? null : new MetaInlineMessage(description), name) { }
        
        internal override async Task<Message> SendReplyMarkup(Session session)
        {
            Message msg;

            action.Invoke(session);

            if(message != null)
            {
                msg = await base.SendReplyMarkup(session).
                    ContinueWith((prevTask) =>
                {
                    session.CurrentNode = this;
                    return prevTask.Result;
                },
                TaskContinuationOptions.None);
            }

            if(Children.Count > 0)
            {
                msg = await Children[0].SendReplyMarkup(session);
            }
            else
            {
                msg = await SendMarkupIfNoChildren(session);
            }

            return msg;
        }

		protected virtual Task<Message> SendMarkupIfNoChildren(Session session) => Parent.SendReplyMarkup(session);

		protected override void AddChild(Node child)
        {
            if (Children.Count > 0) throw new ArgumentException("Узел действия уже содержит один выходной узел.");
            Children.Add(child);
        }

        public void SetPortal(Node child) => AddChild(child);

        public override void AddChildWithButtonRules(Node child, params Predicate<Session>[] rules) =>
            throw new NotSupportedException("У узлов действий не должно быть правил для перехода.");

		public void SetChildrenList(List<Node> children)
		{
			if(children.Count > 1) throw new ArgumentException("Узел действия может содержать только один выходной узел.");
			Children = children;
		}
    }
}
