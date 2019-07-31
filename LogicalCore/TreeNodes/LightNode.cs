using System;

namespace LogicalCore
{
    /// <summary>
    /// Лёгкий узел, на который не выполняется переход.
    /// </summary>
    public class LightNode : Node
    {
        public LightNode(string name, MetaInlineMessage metaMessage = null)
            : base(name, metaMessage ?? new MetaInlineMessage(name))
        {
            Children = null;
        }

        public LightNode(string name, string description) : this(name, new MetaInlineMessage(description)) { }

		protected override void AddChild(Node child) =>
			message.AddNodeButton(child);

        public override void AddChildWithButtonRules(Node child, params Predicate<Session>[] rules) =>
			message.AddNodeButton(child, rules);
	}
}
