﻿using System;
using System.Collections.Generic;
using LogicalCore.TreeNodes;

namespace LogicalCore
{
    public class MarkupTree : IMarkupTree
    {
        public ITreeNode Root { get; }
        
        private readonly Dictionary<int, ITreeNode> nodeIdDict;

        public ITreeNode GetNodeById(int nodeId) => nodeIdDict[nodeId];

        public MarkupTree(ITreeNode nodeRoot)
        {
            Root = nodeRoot;
            nodeIdDict = new Dictionary<int, ITreeNode>
            {
                { Root.Id, Root }
            };
            ConsoleWriter.WriteLine($"К общему списку узлов добавлен корень с именем {Root.Name}", ConsoleColor.DarkGray);
        }

		public void AddEdge(ITreeNode parent, ITreeNode child)
        {
            if (parent != child)
            {
                if(parent is ITeleportable teleportable)
                {
                    teleportable.SetPortal(child);
                }
                else
                {
                    child.SetParent(parent);
                }

				AddChildrenIfNeed(child);

				ConsoleWriter.WriteLine($"К общему списку узлов добавлен узел с именем {child.Name}", ConsoleColor.DarkGray);
            }
            else
            {
                throw new Exception("Нельзя добавлять связь между одним и тем же узлом. " +
                    "Связь должна быть добавлена только между разными узлами");
            }
		}

		public void AddEdge(ITreeNode parent, ITreeNode child, params Predicate<ISession>[] rules)
        {
            if (parent != child)
            {
                parent.AddChildWithButtonRules(child, rules);
				AddChildrenIfNeed(child);

                ConsoleWriter.WriteLine($"К общему списку узлов добавлен узел с именем {child.Name}", ConsoleColor.DarkGray);
            }
            else
            {
                throw new Exception("Нельзя добавлять связь между одним и тем же узлом. " +
                    "Связь должна быть добавлена только между разными узлами");
            }
		}

		private void AddChildrenIfNeed(ITreeNode node)
		{
			if(node is ICombined combined)
			{
				node = combined.HeadNode;
			}

			if (!nodeIdDict.TryAdd(node.Id, node))
			{
				ConsoleWriter.WriteLine($"Узел {node.Name} с ID {node.Id} встречается в дереве несколько раз. Возможная причина - порталы.", ConsoleColor.Yellow);
			}

			if(node.Children != null)
			{
				foreach (var child in node.Children)
				{
					if (!nodeIdDict.ContainsKey(child.Id)) AddChildrenIfNeed(child);
				}
			}
		}

		public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                int primary = 486187739;

                foreach (var key in nodeIdDict.Keys)
                {
                    hash = hash * primary + nodeIdDict[key].GetHashCode();
                }

                return hash;
            }
        }
    }
}