using System.Collections.Generic;

namespace LogicalCore
{
    public interface IWithCollectionInput<VariableType> : IWithInput<VariableType>
    {
        void SetVar(ISession session, List<VariableType> variable);
    }

    public interface IWithSelectionInput<VariableType> : IWithInput<VariableType>
    {
        List<VariableType> Options { get; }
    }

    public interface IWithInput<VariableType>
    {
        string VarName { get; }

        //bool Memoization { get; }

        bool Required { get; }

        TryConvert<VariableType> Converter { get; }

        void SetVar(ISession session, VariableType variable);

        //T GetVar(Session session);

        //bool TryGetVar(Session session, out T variable);
    }

    public delegate bool TryConvert<T>(string text, out T variable);
}
