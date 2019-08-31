using System;
using System.Collections.Generic;

namespace LogicalCore
{
	public class MegaTree
    {
        public readonly Node root;
        
        private readonly Dictionary<int, Node> nodeIdDict;

        internal Node GetNodeById(int nodeId) => nodeIdDict[nodeId];

        public MegaTree(Node nodeRoot)
        {
            root = nodeRoot;
            nodeIdDict = new Dictionary<int, Node>
            {
                { root.id, root }
            };
            ConsoleWriter.WriteLine($"К общему списку узлов добавлен корень с именем {root.name}", ConsoleColor.DarkGray);
        }

		public void AddEdge(Node parent, Node child)
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

				ConsoleWriter.WriteLine($"К общему списку узлов добавлен узел с именем {child.name}", ConsoleColor.DarkGray);
            }
            else
            {
                throw new Exception("Нельзя добавлять связь между одним и тем же узлом. " +
                    "Связь должна быть добавлена только между разными узлами");
            }
		}

		public void AddEdge(Node parent, Node child, params Predicate<Session>[] rules)
        {
            if (parent != child)
            {
                parent.AddChildWithButtonRules(child, rules);
				AddChildrenIfNeed(child);

                ConsoleWriter.WriteLine($"К общему списку узлов добавлен узел с именем {child.name}", ConsoleColor.DarkGray);
            }
            else
            {
                throw new Exception("Нельзя добавлять связь между одним и тем же узлом. " +
                    "Связь должна быть добавлена только между разными узлами");
            }
		}

		private void AddChildrenIfNeed(Node node)
		{
			if (!nodeIdDict.TryAdd(node.id, node))
			{
				ConsoleWriter.WriteLine($"Узел {node.name} с ID {node.id} встречается в дереве несколько раз. Возможная причина - порталы.", ConsoleColor.Yellow);
			}

			if(node.Children != null)
			{
				foreach (var child in node.Children)
				{
					if (!nodeIdDict.ContainsKey(child.id)) AddChildrenIfNeed(child);
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