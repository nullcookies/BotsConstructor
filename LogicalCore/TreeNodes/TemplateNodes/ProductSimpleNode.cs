using System;
using System.Collections.Generic;

namespace LogicalCore
{
	public class ProductSimpleNode<T> : ProductNode<T>
	{
		public ProductSimpleNode(string name, List<List<string>> elements, string itemsContainer, List<int> productsIds, List<MetaReplyMessage> foldersMsgs,
			string sessionContainer, string addedPrefix = DefaultStrings.PLUS, string addBtnName = DefaultStrings.ADD,
			MetaInlineMessage metaMessage = null, bool needBack = true) :
			base(
				CreateHead(name, foldersMsgs[0], metaMessage),
				CreateItemNodes(name, itemsContainer, elements, productsIds,
					sessionContainer, addedPrefix)
				)
		{
			if(foldersMsgs.Count != elements.Count) throw new ArgumentException("Количество названий разделов не совпадает с количеством секций.");
			int itemNumber = 0;
			RecursiveSections(HeadNode);

			void RecursiveSections(Node parent, int index = 0)
			{
				if (index < elements.Count - 1)
				{
					for (int i = 0; i < elements[index].Count; i++)
					{
						var foldersMsg = foldersMsgs[index + 1];
						var clonedMsg = new MetaMessage<MetaKeyboardMarkup<Telegram.Bot.Types.ReplyMarkups.KeyboardButton>>(foldersMsg.Text,
							foldersMsg.Type, foldersMsg.File, foldersMsg.MetaKeyboard.Clone(), foldersMsg.parseMode);
						SimpleNode folder = new SimpleNode(elements[index][i], clonedMsg);
						folder.SetParent(parent);
						RecursiveSections(folder, index + 1);
					}
				}
				else
				{
					for (int i = 0; i < elements[index].Count; i++)
					{
						TailNodes[itemNumber].SetParent(parent);
						TailNodes[itemNumber++].SetBackLink(HeadNode);
					}
				}
			}
		}

		private static Node CreateHead(string name, MetaReplyMessage folderMsg, MetaInlineMessage previewMsg)
		{
			if(previewMsg != null && (previewMsg.Text.ToString() != name || previewMsg.File != null))
			{
				return new SimpleNode(name, new MetaMultiMessage(previewMsg, folderMsg) { DefaultMessageIndex = 1 });
			}
			else
			{
				return new SimpleNode(name, folderMsg);
			}
		}
	}
}
