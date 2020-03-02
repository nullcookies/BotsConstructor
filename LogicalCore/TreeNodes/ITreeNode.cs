using System;
using System.Collections.Generic;

namespace LogicalCore.TreeNodes
{
    public interface ITreeNode : IWithTextAndFile, IWithName, ISendingMessage, ISessionInputHandler
    {
        int Id { get; }
        ITreeNode Parent { get; }
        List<ITreeNode> Children { get; }
        void AddChild(ITreeNode child);
        void AddChildWithButtonRules(ITreeNode child, params Predicate<ISession>[] rules);
        void SetParent(ITreeNode parent);
        void SetBackLink(ITreeNode parent);
        bool CanExecute(string action, ISession session);
    }
}
