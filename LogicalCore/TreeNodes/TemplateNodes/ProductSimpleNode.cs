using System;
using System.Collections.Generic;

namespace LogicalCore
{
	public class ProductSimpleNode<T> : ProductNode<T>
	{
		public ProductSimpleNode(string name, List<List<string>> elements, string itemsContainer, List<int> productsIds, List<string> foldersNames,
			string sessionContainer, string addedPrefix = DefaultStrings.PLUS, string addBtnName = DefaultStrings.ADD,
			IMetaMessage<MetaInlineKeyboardMarkup> metaMessage = null, bool needBack = true) :
			base(
				new SimpleNode(name, new MetaReplyMessage(foldersNames[0], ":")),
				CreateItemNodes(name, itemsContainer, elements, productsIds,
					sessionContainer, addedPrefix)
				)
		{
			if(foldersNames.Count != elements.Count) throw new ArgumentException("Количество названий разделов не совпадает с количеством секций.");
			int itemNumber = 0;
			RecursiveSections(headNode);

			void RecursiveSections(Node parent, int index = 0)
			{
				if (index < elements.Count - 1)
				{
					for (int i = 0; i < elements[index].Count; i++)
					{
						SimpleNode folder = new SimpleNode(elements[index][i], new MetaReplyMessage(foldersNames[index + 1], ":"));
						folder.SetParent(parent);
						RecursiveSections(folder, index + 1);
					}
				}
				else
				{
					for (int i = 0; i < elements[index].Count; i++)
					{
						tailNodes[itemNumber++].SetParent(parent);
					}
				}
			}
		}
	}
}
