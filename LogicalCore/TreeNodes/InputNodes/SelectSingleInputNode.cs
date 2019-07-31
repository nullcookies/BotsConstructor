using System;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace LogicalCore
{
    public class SelectSingleInputNode<T> : FlipperNode<T>, IWithSelectionInput<T>
    {
        public string VarName { get; private set; }
        public TryConvert<T> Converter { get; private set; }
        public bool Required { get; private set; }
        public List<T> Options => collection;

        public SelectSingleInputNode(string name, string varName, List<T> options,
            IMetaMessage<MetaInlineKeyboardMarkup> metaMessage = null, byte pageSize = 6,
            Func<Session, T, string> btnNameFunc = null, Func<T, string> btnCallbackFunc = null, bool required = true,
            FlipperArrowsType flipperArrows = FlipperArrowsType.Double, bool needBack = true, bool useGlobalCallbacks = false)
            : base(name, options, btnNameFunc, btnCallbackFunc, metaMessage ?? new MetaDoubleKeyboardedMessage(name),
                  pageSize, needBack, flipperArrows, useGlobalCallbacks)
        {
            VarName = varName ?? throw new ArgumentNullException(nameof(varName));
            Children = new List<Node>(1);
            Required = required;
            if (collection.Count == 0) throw new ArgumentException(nameof(options));

            Dictionary<string, T> callbackToValue = new Dictionary<string, T>(collection.Count);
            foreach (T element in collection)
            {
                callbackToValue.Add(callbackFunc(element), element);
            }

            Converter = (string text, out T variable) => callbackToValue.TryGetValue(text, out variable);
        }

        public SelectSingleInputNode(string name, string varName, List<T> options, string description = null, 
            byte pageSize = 6, Func<Session, T, string> btnNameFunc = null, Func<T, string> btnCallbackFunc = null,
            bool required = true, FlipperArrowsType flipperArrows = FlipperArrowsType.Double,
            bool needBack = true, bool useGlobalCallbacks = false)
            : this(name, varName, options, new MetaDoubleKeyboardedMessage(description ?? name), pageSize,
                  btnNameFunc, btnCallbackFunc, required, flipperArrows, needBack, useGlobalCallbacks)
        { }

        public void SetVar(Session session, T variable) => session.vars.SetVar(VarName, variable);

        protected override void AddChild(Node child)
        {
            if (Children.Count > 0) throw new NotImplementedException("Input может иметь только одного ребёнка.");
            base.AddChild(child);
            if (!Required) message.AddNodeButton(child);
        }

        //У инпутов переход к ребёнку выполняется только после успешного ввода данных или если инпут необязательный

        protected override bool TryGoToChild(Session session, Message message)
        {
            if (!base.TryGoToChild(session, message))
            {
                if (Converter.Invoke(message.Text, out T variable))
                {
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

        protected override bool TryGoToChild(Session session, CallbackQuery callbackQuerry)
        {
            if (!base.TryGoToChild(session, callbackQuerry))
            {
                if (Converter.Invoke(callbackQuerry.Data, out T variable))
                {
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
