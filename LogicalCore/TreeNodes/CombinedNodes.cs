using System;

namespace LogicalCore.TreeNodes
{
	/// <summary>
	/// Некоторое количество узлов, соединённых определённым образом.
	/// </summary>
	public abstract class CombinedNodes : Node, ICombined, ITeleportable
	{
		/// <summary>
		/// Первый узел в цепи (точка входа).
		/// </summary>
		public ITreeNode HeadNode { get; }
		/// <summary>
		/// Последний узел в цепи (точка выхода).
		/// </summary>
		public ActionNode TailNode { get; }

		public CombinedNodes(Node head, ActionNode tail) : base("<Combined>", (IMetaMessage)null)
		{
			HeadNode = head ?? throw new ArgumentNullException(nameof(head));
			TailNode = tail ?? throw new ArgumentNullException(nameof(tail));
			Children = TailNode.Children;
		}

		public override void AddChildWithButtonRules(ITreeNode child, params Predicate<Session>[] rules) =>
			throw new NotSupportedException("У узлов действий не должно быть правил для перехода.");

		protected override void AddChild(ITreeNode child)
		{
			if (Children.Count > 0) throw new ArgumentException("Узел действия уже содержит один выходной узел.");
			Children.Add(child);
		}

		public void SetPortal(ITreeNode child) => AddChild(child);

		public override void SetParent(ITreeNode parent) => HeadNode.SetParent(parent);
	}
}
