using System;
using LogicalCore.TreeNodes;

namespace LogicalCore
{
    public interface IVariablesContainer
    {
        T GetVar<T>(string varName);
        object GetVar(Type varType, string varName);
        bool TryGetVar<T>(string varName, out T variable);
        bool TryGetVar(Type varType, string varName, out object variable);
        void SetVar<T>(string varName, T varValue);
        void SetVar<T>(T withName) where T : IWithName;
        bool RemoveVar<T>(string varName);
        bool RemoveVar(Type varType, string varName);
        bool ClearVar(Type varType, string varName);


    }
}