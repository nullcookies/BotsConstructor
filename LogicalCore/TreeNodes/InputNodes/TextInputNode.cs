namespace LogicalCore
{
	public class TextInputNode : UsualInputNode<string>
    {
        public TextInputNode(string name, string varName, TryConvert<string> converter,
            IMetaMessage metaMessage = null, bool required = true, bool needBack = true, bool useCallbacks = false)
            : base(name, varName, converter, metaMessage, required, needBack, useCallbacks) { }

        public TextInputNode(string name, string varName, TryConvert<string> converter,
            string description = null, bool required = true, bool needBack = true, bool useCallbacks = false)
            : this(name, varName, converter, new MetaMessage(description ?? name), required, needBack, useCallbacks) { }
    }
}
