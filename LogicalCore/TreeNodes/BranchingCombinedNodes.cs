using System;
using System.Collections.Generic;

namespace LogicalCore
{
	/// <summary>
	/// Некоторое количество узлов, соединённых определённым образом и с несколькими точками выхода.
	/// </summary>
	public abstract class BranchingCombinedNodes : Node, ITeleportable
	{
		/// <summary>
		/// Первый узел в цепи (точка входа).
		/// </summary>
		public readonly Node headNode;
		/// <summary>
		/// Последние узлы в цепи (точки выхода).
		/// </summary>
		public readonly List<ActionNode> tailNodes;

		public BranchingCombinedNodes(Node head, List<ActionNode> tails) : base("<Combined>", (IMetaMessage)null)
		{
			headNode = head ?? throw new ArgumentNullException(nameof(head));
			tailNodes = tails ?? throw new ArgumentNullException(nameof(tails));
			if (tailNodes.Count == 0) throw new ArgumentException("Количество конечных узлов должно быть больше 0.");
			foreach (var tailNode in tailNodes)
			{
				tailNode.SetChildrenList(Children);
			}
		}

		public override void AddChildWithButtonRules(Node child, params Predicate<Session>[] rules) =>
			throw new NotSupportedException("У узлов действий не должно быть правил для перехода.");

		protected override void AddChild(Node child)
		{
			if (Children.Count > 0) throw new ArgumentException("Узел действия уже содержит один выходной узел.");
			Children.Add(child);
		}

		public void SetPortal(Node child) => AddChild(child);

		public override void SetParent(Node parent) => headNode.SetParent(parent);
	}
}
