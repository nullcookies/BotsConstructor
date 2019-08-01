using System;

namespace LogicalCore
{
    /// <summary>
    /// Лёгкий узел, на который не выполняется переход.
    /// </summary>
    public class LightNode : Node
    {
        public LightNode(string name, MetaInlineMessage metaMessage = null)
            : base(name, metaMessage ?? new MetaInlineMessage(name)) { }

        public LightNode(string name, string description) : this(name, new MetaInlineMessage(description)) { }

		public override void SetParent(Node parent)
		{
			base.SetParent(parent);
			foreach (var child in Children)
			{
				child.SetBackLink(Parent);
			}
		}

		public override void SetBackLink(Node parent)
		{
			base.SetBackLink(parent);
			foreach (var child in Children)
			{
				child.SetBackLink(Parent);
			}
		}

		protected override void AddChild(Node child)
		{
			Children.Add(child);
			child.SetBackLink(Parent);
			message.AddNodeButton(child);
		}

        public override void AddChildWithButtonRules(Node child, params Predicate<Session>[] rules)
		{
			Children.Add(child);
			child.SetBackLink(Parent);
			message.AddNodeButton(child, rules);
		}
	}
}
