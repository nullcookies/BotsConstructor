using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace LogicalCore
{
	/// <summary>
	/// Некоторое количество узлов, соединённых определённым образом.
	/// </summary>
	public abstract class CombinedNodes : Node, ICombined, ITeleportable
	{
		/// <summary>
		/// Первый узел в цепи (точка входа).
		/// </summary>
		public Node HeadNode { get; }
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
