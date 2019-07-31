using System;
using System.Collections;
using System.Collections.Generic;

namespace LogicalCore
{
    /// <summary>
    /// Контейнер переменных любых типов (словарь: {(<see cref="Type"/> тип, <see cref="string"/> название), <see cref="object"/> значение}).
    /// </summary>
    public class VariablesContainer
    {
        /// <summary>
        /// Словарь переменных {(<see cref="Type"/> тип, <see cref="string"/> название), <see cref="object"/> значение}.
        /// </summary>
        private readonly Dictionary<(Type type, string name), object> vars;

        public VariablesContainer()
        {
            vars = new Dictionary<(Type type, string name), object>();
        }

        /// <summary>
        /// Позволяет получить переменную.
        /// </summary>
        /// <typeparam name="T">Тип переменной.</typeparam>
        /// <param name="varName">Название переменной.</param>
        /// <returns>Возвращает переменную с указанным типом и названием.</returns>
        public T GetVar<T>(string varName) => (T)vars[(typeof(T), varName)];

		/// <summary>
		/// Позволяет получить переменную.
		/// </summary>
		/// <param name="varType">Тип переменной.</param>
		/// <param name="varName">Название переменной.</param>
		/// <returns>Возвращает переменную указанного типа и названия.</returns>
		public object GetVar(Type varType, string varName) => vars.GetValueOrDefault((varType, varName));

		/// <summary>
		/// Позволяет получить переменную, если такая есть в словаре.
		/// </summary>
		/// <typeparam name="T">Тип переменной.</typeparam>
		/// <param name="varName">Название переменной.</param>
		/// <param name="variable">Переменная.</param>
		/// <returns>Возвращает true, если такая переменная есть, иначе false.</returns>
		public bool TryGetVar<T>(string varName, out T variable)
        {
            if(vars.TryGetValue((typeof(T), varName), out object objVar))
            {
                variable = (T)objVar;
                return true;
            }
            else
            {
                variable = default(T);
                return false;
            }
        }

		/// <summary>
		/// Позволяет получить переменную, если такая есть в словаре.
		/// </summary>
		/// <param name="varType">Тип переменной.</param>
		/// <param name="varName">Название переменной.</param>
		/// <param name="variable">Переменная.</param>
		/// <returns>Возвращает true, если такая переменная есть, иначе false.</returns>
		public bool TryGetVar(Type varType, string varName, out object variable) =>
			vars.TryGetValue((varType, varName), out variable);

		/// <summary>
		/// Позволяет установить или изменить значение переменной.
		/// </summary>
		/// <typeparam name="T">Тип переменной.</typeparam>
		/// <param name="varName">Название переменной.</param>
		/// <param name="varValue">Новое значение переменной.</param>
		public void SetVar<T>(string varName, T varValue) => vars[(typeof(T), varName)] = varValue;

		/// <summary>
		/// Позволяет установить или изменить значение контейнера.
		/// </summary>
		/// <typeparam name="T">Тип контейнера.</typeparam>
		/// <param name="container">Контейнер.</param>
		public void SetVar<T>(MetaValuedContainer<T> container) =>
			vars[(typeof(MetaValuedContainer<T>), container.name)] = container;

		/// <summary>
		/// Позволяет удалить переменную.
		/// </summary>
		/// <typeparam name="T">Тип переменной.</typeparam>
		/// <param name="varName">Название переменной.</param>
		/// <returns>Возвращает true, если такая переменная была, иначе false.</returns>
		public bool RemoveVar<T>(string varName) => vars.Remove((typeof(T), varName));

		/// <summary>
		/// Позволяет удалить переменную.
		/// </summary>
		/// <param name="varType">Тип переменной.</param>
		/// <param name="varName">Название переменной.</param>
		/// <returns>Возвращает true, если такая переменная была, иначе false.</returns>
		public bool RemoveVar(Type varType, string varName) => vars.Remove((varType, varName));

		/// <summary>
		/// Позволяет отчистить переменную.
		/// </summary>
		/// <param name="varType">Тип переменной.</param>
		/// <param name="varName">Название переменной.</param>
		/// <returns>Возвращает true, если такая переменная была, иначе false.</returns>
		public bool ClearVar(Type varType, string varName)
		{
			if(vars.TryGetValue((varType, varName), out object variable))
			{
				if(variable is IList list)
				{
					list.Clear();
				}
				else
				{
					if (variable is IDictionary dictionary)
					{
						dictionary.Clear();
					}
					else
					{
						if (variable is IClearable clearable)
						{
							clearable.Clear();
						}
						else
						{
							vars[(varType, varName)] = varType.IsValueType ? Activator.CreateInstance(varType) : null;
						}
					}
				}

				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
