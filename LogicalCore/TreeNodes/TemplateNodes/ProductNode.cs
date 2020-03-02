using System;
using System.Collections.Generic;

namespace LogicalCore.TreeNodes.TemplateNodes
{
	public abstract class ProductNode<T> : BranchingCombinedNodes
	{
		public ProductNode(ITreeNode head, List<ActionNode> tails) : base(head, tails) { }

		protected static List<ActionNode> CreateItemNodes(string name, string itemsContainer, List<List<string>> sections,
			List<int> productsIds, string sessionContainer, string addedPrefix, bool useBtnName = false, string addBtnName = DefaultStrings.Add)
		{
			List<MetaText> elements = new List<MetaText>();
			string[] protoString = new string[sections.Count - 1];

			RecursiveAdder();

			void RecursiveAdder(int index = 0)
			{
				if(index < sections.Count - 1)
				{
					for (int i = 0; i < sections[index].Count; i++)
					{
						protoString[index] = sections[index][i];
						RecursiveAdder(index + 1);
					}
				}
				else
				{
					for(int i = 0; i < sections[index].Count; i++)
					{
						MetaText metaText = new MetaText(addedPrefix, " ", name, " ");
						foreach (var part in protoString)
						{
							metaText.Append(part, " ");
						}
						metaText.Append(sections[index][i]);
						elements.Add(metaText);
					}
				}
			}

			if (elements.Count != productsIds.Count) throw new ArgumentException("Количество элементов не совпадает с количеством продуктов.");
			List<ActionNode> nodes = new List<ActionNode>();

			if(useBtnName)
			{
				for (int i = 0; i < elements.Count; i++)
				{
					nodes.Add(new ItemNode<T>(itemsContainer, productsIds[i], sessionContainer,
						new MetaInlineMessage(elements[i]), addBtnName));
				}
			}
			else
			{
				var lastSection = sections[sections.Count - 1];
				for (int i = 0, j = 0; i < elements.Count; i++, j++)
				{
					if (j >= lastSection.Count) j = 0;
					nodes.Add(new ItemNode<T>(itemsContainer, productsIds[i], sessionContainer,
						new MetaInlineMessage(elements[i]), lastSection[j]));
				}
			}

			return nodes;
		}
	}
}
