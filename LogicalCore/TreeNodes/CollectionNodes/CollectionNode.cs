using System.Collections.Generic;
using Telegram.Bot.Types;

namespace LogicalCore
{
    /// <summary>
    /// Узел, хранящий коллекцию чего-либо. Показывает их страницами, а не на клавиатуре.
    /// </summary>
    public abstract class CollectionNode<T> : NormalNode
    {
        public readonly byte pageSize;
        public bool NeedNextButton { get; protected set; }
        public bool NeedPreviousButton { get; protected set; }
        protected readonly List<T> collection;

        public CollectionNode(string name, byte pageSize, IMetaMessage metaMessage, List<T> elements = null, bool needBack = false)
            : base(name, metaMessage, needBack)
        {
            this.pageSize = pageSize;
            NeedNextButton = true;
            NeedPreviousButton = true;
            collection = elements ?? new List<T>();
        }

        public CollectionNode(string name, byte pageSize, string description, List<T> elements = null, bool needBack = false)
            : this(name, pageSize, new MetaMessage(description), elements, needBack) { }

        protected void GetStartFinish(Session session, bool goForward, out int start, out int finish)
        {
            if (goForward)
            {
                // Вправо
                start = session.BlockNodePosition;

                if (start >= collection.Count) start = 0;

                finish = start + pageSize;

                if (finish > collection.Count) finish = collection.Count;
            }
            else
            {
                // Влево
                finish = session.BlockNodePosition - pageSize;

                if (finish <= 0)
                {
                    finish = collection.Count;
                    // Если, например, из 8 элементов мы показываем по 6, значит, на последней странице должно быть 2
                    int currentPageSize = collection.Count % pageSize;

                    if (currentPageSize == 0) currentPageSize = pageSize;

                    start = finish - currentPageSize;
                }
                else
                {
                    start = finish - pageSize;
                }

                if (start < 0) start = 0;
            }
        }

        protected abstract void SendNext(Session session, Message divisionMessage = null);

        protected abstract void SendPrevious(Session session, Message divisionMessage = null);

        protected bool TryShowNext(Session session, Message message)
        {
            if (KeyboardActionsManager.CheckNeeding(NeedNextButton, this.message.HaveReplyKeyboard, session, message, DefaultStrings.NEXT))
            {
                SendNext(session);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool TryShowNext(Session session, CallbackQuery callbackQuerry)
        {
            if (KeyboardActionsManager.CheckNeeding(NeedNextButton, message.HaveInlineKeyboard, session, callbackQuerry, DefaultStrings.NEXT, () =>
                ButtonIdManager.GetIDFromCallbackData(callbackQuerry.Data) == id))
            {
                SendNext(session, callbackQuerry.Message);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool TryShowPrevious(Session session, Message message)
        {
            if (KeyboardActionsManager.CheckNeeding(NeedPreviousButton, this.message.HaveReplyKeyboard, session, message, DefaultStrings.PREVIOUS))
            {
                SendPrevious(session);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool TryShowPrevious(Session session, CallbackQuery callbackQuerry)
        {
            if (KeyboardActionsManager.CheckNeeding(NeedPreviousButton, message.HaveInlineKeyboard, session, callbackQuerry, DefaultStrings.PREVIOUS, () =>
                ButtonIdManager.GetIDFromCallbackData(callbackQuerry.Data) == id))
            {
                SendPrevious(session, callbackQuerry.Message);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override bool TryFilter(Session session, Message message) =>
            base.TryFilter(session, message) || TryShowNext(session, message) || TryShowPrevious(session, message);

        protected override bool TryFilter(Session session, CallbackQuery callbackQuerry) =>
            base.TryFilter(session, callbackQuerry) || TryShowNext(session, callbackQuerry) || TryShowPrevious(session, callbackQuerry);
    }
}
