using System.Collections.Generic;

namespace LogicalCore
{
	public class ProductMultiNode<T> : ProductNode<T>
	{
		public ProductMultiNode(string name, List<List<string>> elements, string itemsContainer, List<int> productsIds, List<MetaText> foldersNames,
			string sessionContainer, string addedPrefix = DefaultStrings.PLUS, string addBtnName = DefaultStrings.ADD, MetaDoubleKeyboardedMessage metaMessage = null,
			byte pageSize = 6, bool needBack = true, FlipperArrowsType flipperArrowsType = FlipperArrowsType.Double, bool useGlobalCallbacks = false) :
			base(
				new MultiNode(name, elements, metaMessage, pageSize, needBack, flipperArrowsType, useGlobalCallbacks, foldersNames),
				CreateItemNodes(name, itemsContainer, elements, productsIds,
					sessionContainer, addedPrefix, true, addBtnName)
				)
		{
			foreach (var itemNode in TailNodes)
			{
				itemNode.SetParent(HeadNode);
			}
		}
	}
}
