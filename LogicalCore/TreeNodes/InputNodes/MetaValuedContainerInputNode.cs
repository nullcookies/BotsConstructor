﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LogicalCore.TreeNodes;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LogicalCore
{
    public class MetaValuedContainerInputNode<T> : FlipperNode<MetaValued<T>>, IWithSelectionInput<MetaValued<T>>
    {
        public string VarName { get; private set; }
        public TryConvert<MetaValued<T>> Converter { get; private set; }
        public bool Required => false;
        public List<MetaValued<T>> Options => collection;
        public readonly Action<ISession, MetaValuedContainer<T>> finalisator;
        
        public MetaValuedContainerInputNode(string name, string varName, List<MetaValued<T>> options, IMetaMessage<MetaInlineKeyboardMarkup> metaMessage = null,
            Action<ISession, MetaValuedContainer<T>> endAction = null, Func<ISession, MetaValued<T>, string> btnNameFunc = null, Func<MetaValued<T>, string> btnCallbackFunc = null,
            byte pageSize = 6, FlipperArrowsType flipperArrows = FlipperArrowsType.Double, bool needBack = true, bool useGlobalCallbacks = false)
            : base(name, options, btnNameFunc, btnCallbackFunc ?? ((element) => DefaultStrings.DoNothing), metaMessage ?? new MetaDoubleKeyboardedMessage(name),
                  pageSize, needBack, flipperArrows, useGlobalCallbacks)
        {
            VarName = varName ?? throw new ArgumentNullException(nameof(varName));
            Children = new List<ITreeNode>(1);
            if (collection.Count == 0) throw new ArgumentException(nameof(options));

            Dictionary<string, MetaValued<T>> callbackToValue = new Dictionary<string, MetaValued<T>>(collection.Count);
            foreach (MetaValued<T> element in collection)
            {
                callbackToValue.Add(element.ToString(), element);
            }

            Converter = (string text, out MetaValued<T> variable) => callbackToValue.TryGetValue(text, out variable);
            finalisator = endAction;
        }

        public MetaValuedContainerInputNode(string name, string varName, List<MetaValued<T>> options, string description,
            Action<ISession, MetaValuedContainer<T>> endAction = null, Func<ISession, MetaValued<T>, string> btnNameFunc = null, Func<MetaValued<T>, string> btnCallbackFunc = null,
            byte pageSize = 6, FlipperArrowsType flipperArrows = FlipperArrowsType.Double, bool needBack = true, bool useGlobalCallbacks = false)
            : this(name, varName, options, new MetaDoubleKeyboardedMessage(description ?? name), endAction, btnNameFunc,
                  btnCallbackFunc, pageSize, flipperArrows, needBack, useGlobalCallbacks)
        { }

        public void SetVar(ISession session, MetaValuedContainer<T> variable) => session.Vars.SetVar(VarName, variable);

        public void SetVar(ISession session, MetaValued<T> variable) => IncreaseVar(session, variable);

        public void IncreaseVar(ISession session, MetaValued<T> variable)
        {
            MetaValuedContainer<T> container = session.Vars.GetVar<MetaValuedContainer<T>>(VarName);
            container.Add(variable, 1);
        }

        public void DecreaseVar(ISession session, MetaValued<T> variable)
        {
            MetaValuedContainer<T> container = session.Vars.GetVar<MetaValuedContainer<T>>(VarName);
            container.Add(variable, -1);
        }

        private bool MoreThanZero(ISession session, MetaValued<T> variable) =>
            session.Vars.GetVar<MetaValuedContainer<T>>(VarName).ContainsKey(variable);

        public override Task<Message> SendMessage(ISession session)
        {
            if (!session.Vars.TryGetVar<MetaValuedContainer<T>>(VarName, out var container) || container == null)
                SetVar(session, new MetaValuedContainer<T>(VarName));
            return base.SendMessage(session);
        }

        protected override void AddChild(ITreeNode child)
        {
            if (Children.Count > 0) throw new NotImplementedException("Input может иметь только одного ребёнка.");
            base.AddChild(child);
            message.AddNodeButton(child);
        }

        protected override List<InlineKeyboardButton> GetRowForElement(ISession session, MetaValued<T> element) =>
            new List<InlineKeyboardButton>(4)
            {
                InlineKeyboardButton.WithCallbackData(nameFunc(session, element), callbackFunc(element)),
                InlineKeyboardButton.WithCallbackData(session.Translate(DefaultStrings.Minus), $"{DefaultStrings.Decrease}_{VarName}_{element.ToString()}"),
                InlineKeyboardButton.WithCallbackData(session.Vars.GetVar<MetaValuedContainer<T>>(VarName).
                    TryGetValue(element, out int value) ? value.ToString() : "0", DefaultStrings.DoNothing), // количество, если есть, или 0
                InlineKeyboardButton.WithCallbackData(session.Translate(DefaultStrings.Plus), $"{DefaultStrings.Increase}_{VarName}_{element.ToString()}")

			};

        protected bool TryChangeElement(ISession session, CallbackQuery callbackQuerry)
        {
            string action = ButtonIdManager.GetActionNameFromCallbackData(callbackQuerry.Data);

            bool increase = false;
            if(action == DefaultStrings.Increase)
            {
                increase = true;
            }
            else if(action == DefaultStrings.Decrease)
            {
                increase = false;
            }
            else
            {
                return false;
            }

            string containerName = ButtonIdManager.GetNextSubstring(callbackQuerry.Data, action.Length, out int nextUnder);

            if(containerName == VarName)
            {
                string elementName = ButtonIdManager.GetNextSubstring(callbackQuerry.Data, nextUnder);

                if (Converter.Invoke(elementName, out MetaValued<T> variable))
                {
                    if(increase)
                    {
                        IncreaseVar(session, variable);
                        SendPage(session, callbackQuerry.Message, GetPageByElement(variable)).Wait();
                    }
                    else
                    {
                        if(MoreThanZero(session, variable))
                        {
                            DecreaseVar(session, variable);
                            SendPage(session, callbackQuerry.Message, GetPageByElement(variable)).Wait();
                        }
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        protected override bool TryGoToChild(ISession session, Message message)
        {
            if(base.TryGoToChild(session, message))
            {
                finalisator?.Invoke(session, session.Vars.GetVar<MetaValuedContainer<T>>(VarName));
                return true;
            }
            return false;
        }

        protected override bool TryGoToChild(ISession session, CallbackQuery callbackQuerry)
        {
            if (base.TryGoToChild(session, callbackQuerry))
            {
                finalisator?.Invoke(session, session.Vars.GetVar<MetaValuedContainer<T>>(VarName));
                return true;
            }
            return false;
        }

        protected override bool TryFilter(ISession session, CallbackQuery callbackQuerry) =>
            base.TryFilter(session, callbackQuerry) || TryChangeElement(session, callbackQuerry);
    }
}
