using System;
using System.Collections.Generic;

namespace LogicalCore
{
	/// <summary>
	/// Некоторое количество узлов, соединённых определённым образом и с несколькими точками выхода.
	/// </summary>
	public abstract class BranchingCombinedNodes : Node, ICombined, ITeleportable
	{
		/// <summary>
		/// Первый узел в цепи (точка входа).
		/// </summary>
		public Node HeadNode { get; }
		/// <summary>
		/// Последние узлы в цепи (точки выхода).
		/// </summary>
		public List<ActionNode> TailNodes { get; }

		public BranchingCombinedNodes(Node head, List<ActionNode> tails) : base("<Combined>", (IMetaMessage)null)
		{
			HeadNode = head ?? throw new ArgumentNullException(nameof(head));
			TailNodes = tails ?? throw new ArgumentNullException(nameof(tails));
			if (TailNodes.Count == 0) throw new ArgumentException("Количество конечных узлов должно быть больше 0.");
			foreach (var tailNode in TailNodes)
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

		public override void SetParent(Node parent) => HeadNode.SetParent(parent);
	}
}
