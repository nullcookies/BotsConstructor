using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LogicalCore
{
	public enum FlipperArrowsType
    {
        /// <summary>
        /// По две кнопки сверху и снизу.
        /// </summary>
        Double,
        /// <summary>
        /// Две кнопки снизу.
        /// </summary>
        Down,
        /// <summary>
        /// Две кнопки сверху.
        /// </summary>
        Up,
        /// <summary>
        /// Влево сверху, вправо снизу.
        /// </summary>
        Vertical
    }

    public class FlipperNode<T> : CollectionNode<T>, IFlippable
    {
        public readonly FlipperArrowsType arrowsType;
        public bool GlobalCallbacks { get; }
        protected readonly Func<Session, T, string> nameFunc;
        protected readonly Func<T, string> callbackFunc;
		protected virtual int MaxPage => (collection.Count - 1) / pageSize;
		protected virtual bool OnePage => pageSize >= collection.Count;

		public FlipperNode(string name, List<T> elements, Func<Session, T, string> btnNameFunc = null, Func<T, string> btnCallbackFunc = null,
            IMetaMessage<MetaInlineKeyboardMarkup> metaMessage = null, byte pageSize = 6, bool needBack = true,
            FlipperArrowsType flipperArrows = FlipperArrowsType.Double, bool useGlobalCallbacks = true)
            : base(name, pageSize, metaMessage ?? new MetaDoubleKeyboardedMessage(name), elements, needBack)
        {
            nameFunc = btnNameFunc ?? ((session, element) => element.ToString(session));
            callbackFunc = btnCallbackFunc ?? ((element) => element.ToString());
            arrowsType = flipperArrows;
            GlobalCallbacks = useGlobalCallbacks;
		}

        public FlipperNode(string name, List<T> elements, string description,
            Func<Session, T, string> btnNameFunc = null, Func<T, string> btnCallbackFunc = null,
            byte pageSize = 6, bool needBack = true, FlipperArrowsType flipperArrows = FlipperArrowsType.Double, bool useGlobalCallbacks = true)
            : this(name, elements, btnNameFunc, btnCallbackFunc, description == null ? null : new MetaDoubleKeyboardedMessage(description),
                  pageSize, needBack, flipperArrows, useGlobalCallbacks) { }

        public override async Task<Message> SendMessage(Session session) =>
            await await base.SendMessage(session).
                ContinueWith(async (prevTask) => await SendPage(session, await prevTask),
                TaskContinuationOptions.NotOnFaulted);

        public virtual async Task<Message> SendPage(Session session, Message divisionMessage, int pageNumber = 0)
        {
            if (divisionMessage == null) return await SendMessage(session);

            if (!GlobalCallbacks) session.BlockNodePosition = Math.Min((pageNumber + 1) * pageSize, collection.Count);

            return await EditMessage(session, divisionMessage, pageNumber);
        }

        // Этот метод используется, когда страницы можно листать только находясь на текущем узле (globalCallbacks = false)
        protected virtual async void SendPageBySessionInfo(Session session, bool goForward, Message divisionMessage)
        {
            if (divisionMessage == null)
            {
                await SendMessage(session);
                return;
            }

            GetStartFinish(session, goForward, out int start, out int finish);

            session.BlockNodePosition = finish;

            int pageNumber = start / pageSize;

            await EditMessage(session, divisionMessage, pageNumber);
        }

        protected virtual async Task<Message> EditMessage(Session session, Message divisionMessage, int page)
        {
	        try
	        {
		        return await session.BotClient.EditMessageReplyMarkupAsync(
			        session.telegramId,
			        divisionMessage.MessageId,
			        GetInlineMarkup(session, page));
	        }
	        catch (Exception e)
	        {
		        ConsoleWriter.WriteLine(e.Message, ConsoleColor.Red);
		        return divisionMessage;
	        }
        }

		protected virtual InlineKeyboardMarkup GetInlineMarkup(Session session, int page)
        {
            if (page < 0) page = 0;

			GetStartFinish(page, out int start, out int finish);

			// Если у нас элементов только на 1 страницу, то можно стрелки навигации не добавлять
			if (OnePage)
            {
                var buttons = new List<List<InlineKeyboardButton>>();

                FillInlineButtons(session, buttons, start, finish);

				AddSpecialRow(session, page, buttons);

				return new InlineKeyboardMarkup(buttons);
            }

			GetNearbyPages(page, out int leftPage, out int rightPage);

            string callbackDataPrevious;
            string callbackDataNext;

            if(GlobalCallbacks)
            {
                callbackDataPrevious = $"{DefaultStrings.ShowPage}_{Id}_{leftPage}";
				callbackDataNext = $"{DefaultStrings.ShowPage}_{Id}_{rightPage}";
            }
            else
            {
                callbackDataPrevious = $"{DefaultStrings.Previous}_{Id}";
                callbackDataNext = $"{DefaultStrings.Next}_{Id}";
            }

            InlineKeyboardButton previous = InlineKeyboardButton.WithCallbackData(session.Translate(DefaultStrings.Previous), callbackDataPrevious);
            InlineKeyboardButton next = InlineKeyboardButton.WithCallbackData(session.Translate(DefaultStrings.Next), callbackDataNext);

            //Строки для листания
            List<InlineKeyboardButton> topRow = null, bottomRow = null;
            bool needTop = false, needBottom = false;

            switch (arrowsType)
            {
                case FlipperArrowsType.Double:
                    topRow = bottomRow = new List<InlineKeyboardButton>(2) { previous, next };
                    needTop = needBottom = true;
                    break;
                case FlipperArrowsType.Down:
                    bottomRow = new List<InlineKeyboardButton>(2) { previous, next };
                    needBottom = true;
                    break;
                case FlipperArrowsType.Up:
                    topRow = new List<InlineKeyboardButton>(2) { previous, next };
                    needTop = true;
                    break;
                case FlipperArrowsType.Vertical:
                    topRow = new List<InlineKeyboardButton>(1) { previous };
                    bottomRow = new List<InlineKeyboardButton>(1) { next };
                    needTop = needBottom = true;
                    break;
                default:
                    throw new NotImplementedException("Неподдерживаемый тип.");
            }

            List<List<InlineKeyboardButton>> inlineKeyboardButtons = new List<List<InlineKeyboardButton>>();

            if (needTop) inlineKeyboardButtons.Add(topRow);

            FillInlineButtons(session, inlineKeyboardButtons, start, finish);

            if (needBottom) inlineKeyboardButtons.Add(bottomRow);

			AddSpecialRow(session, page, inlineKeyboardButtons);

			var inlineKeyboard = new InlineKeyboardMarkup(inlineKeyboardButtons);
            return inlineKeyboard;
        }

		protected virtual void AddSpecialRow(Session session, int page, List<List<InlineKeyboardButton>> inlineKeyboardButtons)
		{ }

		protected virtual void FillInlineButtons(Session session, List<List<InlineKeyboardButton>> buttons, int from, int to)
		{
			int index = 0;
			var inlineKeyboard = (message as IMetaMessage<MetaInlineKeyboardMarkup>)?.MetaKeyboard
				.TranslateMarkup(session).InlineKeyboard;
			if (inlineKeyboard != null)
				foreach (var row in inlineKeyboard)
				{
					buttons.Insert(index++, new List<InlineKeyboardButton>(row));
				}

			for (int i = from; i < to; i++)
			{
				buttons.Add(GetRowForElement(session, collection[i]));
			}
		}

		protected virtual void GetNearbyPages(int currentPage, out int leftPage, out int rightPage)
		{
			leftPage = currentPage - 1;
			if (leftPage < 0) leftPage = MaxPage;
			
			rightPage = currentPage + 1;
			if (rightPage > MaxPage) rightPage = 0;
		}

		protected virtual void GetStartFinish(int currentPage, out int start, out int finish)
		{
			start = currentPage * pageSize;
			if (start >= collection.Count) start = 0;

			finish = start + pageSize;
			if (finish > collection.Count) finish = collection.Count;
		}

		protected int GetPageByElement(T element) => collection.IndexOf(element) / pageSize;

        protected virtual List<InlineKeyboardButton> GetRowForElement(Session session, T element) =>
            new List<InlineKeyboardButton>(1) { InlineKeyboardButton.WithCallbackData(nameFunc(session, element), callbackFunc(element)) };

        //Отправка узлов, обработка кнопок, ...
        protected override void SendNext(Session session, Message divisionMessage) => SendPageBySessionInfo(session, true, divisionMessage);

        protected override void SendPrevious(Session session, Message divisionMessage) => SendPageBySessionInfo(session, false, divisionMessage);
    }
}
