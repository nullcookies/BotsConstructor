using LogicalCore.TreeNodes;

namespace LogicalCore
{
    public class ChildrenFlipperNode : FlipperNode<ITreeNode>
    {
        public ChildrenFlipperNode(string name, IMetaMessage<MetaInlineKeyboardMarkup> metaMessage = null, byte pageSize = 6,
            bool needBack = true, FlipperArrowsType flipperArrows = FlipperArrowsType.Double, bool useGlobalCallbacks = true)
            : base(name, null,
                  (session, child) => session.Translate(child.Name),
                  (child) => ButtonIdManager.GetInlineButtonId(child),
                  metaMessage ?? new MetaDoubleKeyboardedMessage(name),
                  pageSize, needBack, flipperArrows, useGlobalCallbacks)
        {
            Children = collection;
        }

        public ChildrenFlipperNode(string name, string description, byte pageSize = 6,
            bool needBack = true, FlipperArrowsType flipperArrows = FlipperArrowsType.Double, bool useGlobalCallbacks = true)
            : this(name, new MetaDoubleKeyboardedMessage(description), pageSize, needBack, flipperArrows, useGlobalCallbacks) { }
    }
}
