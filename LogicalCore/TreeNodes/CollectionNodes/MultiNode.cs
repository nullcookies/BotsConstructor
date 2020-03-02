using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LogicalCore.TreeNodes;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LogicalCore
{
	public class MultiNode : FlipperNode<string>
	{
		private readonly Dictionary<string, int> elemToSection;
		private readonly (int Size, int Increment, int Count)[] sections;
		private readonly List<MetaText> sectionsNames;
		//(sections.Length - 1) / pageSize + 1 - количество страниц для флиппера
		//Children.Count - количество разных состояний (равно произведению всех длин секций)
		//MaxPage - максимально возможное состояние страницы, учитывающее комбинации предыдущих (-1, потому что Max, а не Count)
		protected override int MaxPage => ((sections.Length - 1) / pageSize + 1) * Children.Count - 1;
		protected override bool OnePage => pageSize >= sections.Length;

		public MultiNode(string name, List<List<string>> elements, IMetaMessage<MetaInlineKeyboardMarkup> metaMessage = null,
			byte pageSize = 6, bool needBack = true, FlipperArrowsType flipperArrows = FlipperArrowsType.Double, bool useGlobalCallbacks = false, List<MetaText> foldersNames = null) :
			base(name, elements.SelectMany(_list => _list).ToList(), null, (elem) => DefaultStrings.DoNothing, metaMessage,
				pageSize, needBack, flipperArrows, useGlobalCallbacks)
		{
			if(elements == null) throw new ArgumentNullException(nameof(elements));
			elemToSection = new Dictionary<string, int>();
			sections = new (int Size, int Increment, int Count)[elements.Count];
			sectionsNames = foldersNames;
			if(sectionsNames != null && sectionsNames.Count != elements.Count) throw new ArgumentException("Количество названий секций не совпадает с количеством секций.");
			int sectionSize = 1;
			int sectionIncrement = 1;
			for(int i = elements.Count - 1; i >= 0; i--)
			{
				var list = elements[i];
				if (list.Count == 0) throw new ArgumentException("Количество элементов в секциях должно быть больше 0.");
				sectionSize *= list.Count;
				foreach (var element in list)
				{
					elemToSection.Add(element, sectionSize);
				}
				sections[i] = (sectionSize, sectionIncrement, list.Count);
				sectionIncrement = sectionSize;
			}
		}

		public MultiNode(string name, List<List<string>> elements, string description,
			byte pageSize = 6, bool needBack = true, FlipperArrowsType flipperArrows = FlipperArrowsType.Double, bool useGlobalCallbacks = false) :
			this(name, elements, description == null ? null : new MetaDoubleKeyboardedMessage(description),
				pageSize, needBack, flipperArrows, useGlobalCallbacks) { }

		public override async Task<Message> SendPage(Session session, Message divisionMessage, int pageNumber = 0)
		{
			if (divisionMessage == null) return await SendMessage(session);

			if (!GlobalCallbacks) session.BlockNodePosition = pageNumber;

			return await EditMessage(session, divisionMessage, pageNumber);
		}

		protected override async void SendPageBySessionInfo(Session session, bool goForward, Message divisionMessage)
		{
			if (divisionMessage == null)
			{
				await SendMessage(session);
				return;
			}

			int pageNumber;
			int currentPage = session.BlockNodePosition;

			if (goForward)
			{
				pageNumber = currentPage + Children.Count;
				if (pageNumber > MaxPage) pageNumber = 0;
			}
			else
			{
				pageNumber = currentPage - Children.Count;
				if (pageNumber < 0) pageNumber = MaxPage;
			}

			session.BlockNodePosition = pageNumber;

			await EditMessage(session, divisionMessage, pageNumber);
		}

		protected override void GetNearbyPages(int currentPage, out int leftPage, out int rightPage)
		{
			leftPage = currentPage - Children.Count;
			if (leftPage < 0) leftPage = MaxPage;

			rightPage = currentPage + Children.Count;
			if (rightPage > MaxPage) rightPage = 0;
		}

		protected override void GetStartFinish(int currentPage, out int start, out int finish)
		{
			int flipperPage = currentPage / Children.Count;

			start = flipperPage * pageSize;
			if (start >= sections.Length) start = 0;

			finish = start + pageSize;
			if (finish > sections.Length) finish = sections.Length;
		}

		protected override async Task<Message> EditMessage(Session session, Message divisionMessage, int page)
		{
			return await session.BotClient.EditMessageTextAsync(
				session.telegramId,
				divisionMessage.MessageId,
				GetText(session, page),
				Telegram.Bot.Types.Enums.ParseMode.Default,
				true,
				GetInlineMarkup(session, page));
		}

		protected override void AddSpecialRow(Session session, int page, List<List<InlineKeyboardButton>> inlineKeyboardButtons)
		{
            ITreeNode child = Children[page % Children.Count];

			base.AddSpecialRow(session, page, inlineKeyboardButtons);

			inlineKeyboardButtons.Add(new List<InlineKeyboardButton>(1)
			{ InlineKeyboardButton.WithCallbackData(nameFunc(session, child.Name), ButtonIdManager.GetInlineButtonId(child)) });
		}

		private string GetText(Session session, int page)
		{
			MetaText metaText = new MetaText();

			int sectionPrefix = 0;
			for (int i = 0; i < sections.Length; i++)
			{
				var (Size, Increment, Count) = sections[i];
				int sectionNumber = page / Increment;
				int index = sectionPrefix + sectionNumber;
				if (sectionsNames != null)
				{
					metaText.Append(sectionsNames[i], ": ", collection[index], "\n");
				}
				else
				{
					metaText.Append(collection[index], " ");
				}
				page = page - sectionNumber * Increment;
				sectionPrefix += Count;
			}

			return metaText.ToString(session);
		}

		protected override void FillInlineButtons(Session session, List<List<InlineKeyboardButton>> buttons, int from, int to)
		{
			int index = 0;
			foreach (var row in (message as IMetaMessage<MetaInlineKeyboardMarkup>)?.
				MetaKeyboard.TranslateMarkup(session).InlineKeyboard)
			{
				buttons.Insert(index++, new List<InlineKeyboardButton>(row));
			}

			for (int i = from; i < to; i++)
			{
				buttons.Add(GetRowForSection(session, i));
			}
		}

		protected List<InlineKeyboardButton> GetRowForSection(Session session, int section)
		{
			int currentPage = session.BlockNodePosition;
			var (sectionSize, sectionIncrement, sectionCount) = sections[section];

			int sectionNumber = currentPage / sectionSize;

			int decreasedPage = currentPage - sectionIncrement;
			if (decreasedPage < 0 || decreasedPage / sectionSize != sectionNumber) decreasedPage += sectionSize;

			int increasedPage = currentPage + sectionIncrement;
			if (increasedPage > MaxPage || increasedPage / sectionSize != sectionNumber) increasedPage -= sectionSize;

			int elementIndex = 0;
			for(int i = 0; i < section; i++)
			{
				elementIndex += sections[i].Count;
			}
			elementIndex += (currentPage - sectionNumber * sectionSize) / sectionIncrement;
			string element = collection[elementIndex];

			return new List<InlineKeyboardButton>(3)
			{
				InlineKeyboardButton.WithCallbackData(session.Translate(DefaultStrings.Previous), $"{DefaultStrings.ShowPage}_{Id}_{decreasedPage}"),
				InlineKeyboardButton.WithCallbackData(nameFunc(session, element), callbackFunc(element)),
				InlineKeyboardButton.WithCallbackData(session.Translate(DefaultStrings.Next), $"{DefaultStrings.ShowPage}_{Id}_{increasedPage}")
			};
		}
	}
}
