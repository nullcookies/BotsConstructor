﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LogicalCore.TreeNodes;
using Telegram.Bot.Types;

namespace LogicalCore
{
    public class SelectManyInputNode<T> : FlipperNode<T>, IWithSelectionInput<T>, IWithCollectionInput<T>
    {
        protected const string defaultChecked = "☑️";
        protected const string defaultUnchecked = "";
        public string VarName { get; private set; }
        public TryConvert<T> Converter { get; private set; }
        public bool Required => false;
        public List<T> Options => collection;

        public SelectManyInputNode(string name, string varName, List<T> options,
            IMetaMessage<MetaInlineKeyboardMarkup> metaMessage = null,
            string checkedSymbol = defaultChecked, string uncheckedSymbol = defaultUnchecked, Func<T, string> btnCallbackFunc = null,
            byte pageSize = 6, FlipperArrowsType flipperArrows = FlipperArrowsType.Double, bool needBack = true, bool useGlobalCallbacks = false)
            : base(name, options,
                  (session, element) => (session.Vars.GetVar<List<T>>(varName)?.Contains(element) ?? false ? // если элемент есть,
                  checkedSymbol : uncheckedSymbol) + element.ToString(session), // то добавляем checkedSymbol, иначе uncheckedSymbol
                  btnCallbackFunc, metaMessage ?? new MetaDoubleKeyboardedMessage(name),
                  pageSize, needBack, flipperArrows, useGlobalCallbacks)
        {
            VarName = varName ?? throw new ArgumentNullException(nameof(varName));
            Children = new List<ITreeNode>(1);
            if (collection.Count == 0) throw new ArgumentException(nameof(options));

            Dictionary<string, T> callbackToValue = new Dictionary<string, T>(collection.Count);
            foreach (T element in collection)
            {
                callbackToValue.Add(callbackFunc(element), element);
            }

            Converter = (string text, out T variable) => callbackToValue.TryGetValue(text, out variable);
        }

        public SelectManyInputNode(string name, string varName, List<T> options, string description,
            string checkedSymbol = defaultChecked, string uncheckedSymbol = defaultUnchecked, Func<T, string> btnCallbackFunc = null,
            byte pageSize = 6, FlipperArrowsType flipperArrows = FlipperArrowsType.Double,
            bool needBack = true, bool useGlobalCallbacks = false)
            : this(name, varName, options, new MetaDoubleKeyboardedMessage(description ?? name), checkedSymbol, uncheckedSymbol,
                  btnCallbackFunc, pageSize, flipperArrows, needBack, useGlobalCallbacks)
        { }

        public void SetVar(ISession session, List<T> variable) => session.Vars.SetVar(VarName, variable);

        public void SetVar(ISession session, T variable)
        {
            List<T> selected = session.Vars.GetVar<List<T>>(VarName);
            if (selected.Contains(variable))
            {
                selected.Remove(variable);
            }
            else
            {
                selected.Add(variable);
            }
        }

        public override Task<Message> SendMessage(ISession session)
        {
            if (!session.Vars.TryGetVar<List<T>>(VarName, out var list) || list == null) SetVar(session, new List<T>(Options.Count));
            return base.SendMessage(session);
        }

        protected override void AddChild(ITreeNode child)
        {
            if (Children.Count > 0) throw new NotImplementedException("Input может иметь только одного ребёнка.");
            base.AddChild(child);
            message.AddNodeButton(child);
        }

        protected bool TrySelectElement(ISession session, Message message)
        {
            if (Converter.Invoke(message.Text, out T variable))
            {
                SetVar(session, variable);
                SendPage(session, null, GetPageByElement(variable)).Wait();
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool TrySelectElement(ISession session, CallbackQuery callbackQuerry)
        {
            if (Converter.Invoke(callbackQuerry.Data, out T variable))
            {
                SetVar(session, variable);
                SendPage(session, callbackQuerry.Message, GetPageByElement(variable)).Wait();
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override bool TryFilter(ISession session, Message message) =>
            base.TryFilter(session, message) || TrySelectElement(session, message);

        protected override bool TryFilter(ISession session, CallbackQuery callbackQuerry) =>
            base.TryFilter(session, callbackQuerry) || TrySelectElement(session, callbackQuerry);
    }
}
