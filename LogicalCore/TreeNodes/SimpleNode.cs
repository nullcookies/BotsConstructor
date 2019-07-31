namespace LogicalCore
{
    /// <summary>
    /// Обычный узел с разметкой.
    /// </summary>
    public class SimpleNode : NormalNode
    {
        public SimpleNode(string name, IMetaMessage metaMessage = null, bool needBack = true) : base(name, metaMessage, needBack) { }

        public SimpleNode(string name, string description, bool needBack = true) : this(name, new MetaMessage(description), needBack) { }

        protected override void AddChild(Node child)
        {
            base.AddChild(child);
            message.AddNodeButton(child);
        }
    }
}
