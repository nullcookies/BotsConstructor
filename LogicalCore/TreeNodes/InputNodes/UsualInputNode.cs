using System;
using System.Collections.Generic;
using LogicalCore.TreeNodes;
using Telegram.Bot.Types;

namespace LogicalCore
{
	public class UsualInputNode<T> : NormalNode, IWithInput<T>
    {
        public string VarName { get; private set; }

        public TryConvert<T> Converter { get; private set; }

        public bool Required { get; private set; }

		public readonly bool usingCallbacks;

        public UsualInputNode(string name, string varName, TryConvert<T> converter,
            IMetaMessage metaMessage = null, bool required = true, bool needBack = true, bool useCallbacks = false)
            : base(name, metaMessage, needBack)
        {
            VarName = varName ?? throw new ArgumentNullException(nameof(varName));
            Children = new List<ITreeNode>(1);
            Required = required;
            //Memoization = needSave;
            Converter = converter ?? throw new ArgumentNullException(nameof(converter));
			usingCallbacks = useCallbacks;
		}

        public UsualInputNode(string name, string varName, TryConvert<T> converter,
            string description = null, bool required = true, bool needBack = true, bool useCallbacks = false)
            : this(name, varName, converter, new MetaMessage(description ?? name), required, needBack, useCallbacks) { }

        public void SetVar(ISession session, T variable) => session.Vars.SetVar(VarName, variable);

        //public T GetVar(Session session) => session.vars.GetVar<T>(VarName);

        //public bool TryGetVar(Session session, out T variable) => session.vars.TryGetVar(VarName, out variable);

        protected override void AddChild(ITreeNode child)
        {
            if (Children.Count > 0) throw new NotImplementedException("Input может иметь только одного ребёнка.");
            base.AddChild(child);
            if (!Required) message.AddNodeButton(child);
        }

        //У инпутов переход к ребёнку выполняется только после успешного ввода данных или если инпут необязательный

        protected override bool TryGoToChild(ISession session, Message message)
        {
            if(!base.TryGoToChild(session, message))
            {
                if (Converter.Invoke(message.Text, out T variable))
                {
                    //if(Memoization)
                    SetVar(session, variable);
                    GoToChildNode(session, Children[0]);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        protected override bool TryGoToChild(ISession session, CallbackQuery callbackQuerry)
        {
            if (!base.TryGoToChild(session, callbackQuerry))
            {
                if (usingCallbacks && Converter.Invoke(callbackQuerry.Data, out T variable))
                {
                    //if(Memoization)
                    SetVar(session, variable);
                    GoToChildNode(session, Children[0]);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
    }
}
