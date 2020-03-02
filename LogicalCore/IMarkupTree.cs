using System;
using LogicalCore.TreeNodes;

namespace LogicalCore
{
    public interface IMarkupTree
    {
        ITreeNode Root { get; }
        ITreeNode GetNodeById(int nodeId);
        void AddEdge(ITreeNode parent, ITreeNode child);
        void AddEdge(ITreeNode parent, ITreeNode child, params Predicate<Session>[] rules);
    }
}