using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace LogicalCore.TreeNodes
{
    public class ActionNode : Node, ITeleportable
	{
        private readonly Action<ISession> action;

        public ActionNode(Action<ISession> nodeAction, MetaInlineMessage metaMessage = null, string name = null)
            : base(name ?? DefaultStrings.Next, metaMessage)
        {
            Children = new List<ITreeNode>(1);
            message = metaMessage;
            action = nodeAction ?? ((session) => { });
        }

        public ActionNode(Action<ISession> nodeAction, string description, string name = null)
			: this(nodeAction, description == null ? null : new MetaInlineMessage(description), name) { }
        
        public override async Task<Message> SendMessage(ISession session)
        {
            Message msg;

            action.Invoke(session);

            if(message != null)
            {
                msg = await base.SendMessage(session).
                    ContinueWith((prevTask) =>
                {
                    session.CurrentNode = this;
                    return prevTask.Result;
                },
                TaskContinuationOptions.NotOnFaulted);
            }

            if(Children.Count > 0)
            {
                msg = await Children[0].SendMessage(session);
            }
            else
            {
                msg = await SendMarkupIfNoChildren(session);
            }

            return msg;
        }

		protected virtual Task<Message> SendMarkupIfNoChildren(ISession session) => Parent.SendMessage(session);

		protected override void AddChild(ITreeNode child)
        {
            if (Children.Count > 0) throw new ArgumentException("Узел действия уже содержит один выходной узел.");
            Children.Add(child);
        }

        public void SetPortal(ITreeNode child) => AddChild(child);

        public override void AddChildWithButtonRules(ITreeNode child, params Predicate<ISession>[] rules) =>
            throw new NotSupportedException("У узлов действий не должно быть правил для перехода.");

		public void SetChildrenList(List<ITreeNode> children)
		{
			if(children.Count > 1) throw new ArgumentException("Узел действия может содержать только один выходной узел.");
			Children = children;
		}
    }
}
