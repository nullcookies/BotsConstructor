namespace LogicalCore
{
	public static class UniversalTranslator
	{
		/// <summary>
		/// Выполняет перевод переменной любого типа на язык указанной сессии, если это возможно.
		/// </summary>
		/// <typeparam name="T">Тип переменной.</typeparam>
		/// <param name="variable">Переменная.</param>
		/// <param name="session">Сессия.</param>
		/// <returns>Возвращает переведённую строку.</returns>
		public static string ToString<T>(this T variable, Session session)
		{
			if(variable is ISessionTranslatable translatable)
			{
				return translatable.ToString(session);
			}
			else
			{
				return session.Translate(variable.ToString());
			}
		}
	}
}
