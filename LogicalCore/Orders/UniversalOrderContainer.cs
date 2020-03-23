using System;
using System.Linq;
using System.Collections.Generic;

namespace LogicalCore
{
	/// <summary>
	/// Универсальный контейнер заказа, в который можно поместить всё, что может пригодиться.
	/// </summary>
	public class UniversalOrderContainer
	{
		public static Func<IEnumerable<(Type varType, string varName)>, Func<ISession, UniversalOrderContainer>> generateContainerCreator =
			(IEnumerable<(Type varType, string varName)> variablesInfo) => (session) =>
            {
				var items = new List<(int ID, int Count)>();
				var texts = new List<string>();
				var files = new List<(string FileId, string PreviewId, string Description)>();
				foreach (var (varType, varName) in variablesInfo)
				{
					if (varType == typeof(MetaValuedContainer<decimal>))
					{
						items.AddRange(session.Vars.GetVar<MetaValuedContainer<decimal>>(varName).Select(_pair => (_pair.Key.ID ?? 0, _pair.Value)));
					}
					else
					{
						if (varType == typeof(string))
						{
							texts.Add(session.Vars.GetVar<string>(varName));
						}
						else
						{
							if (varType == typeof((string FileId, string PreviewId, string Description)))
							{
								files.Add(session.Vars.GetVar<(string FileId, string PreviewId, string Description)>(varName));
							}
							else
							{
								texts.Add(session.Vars.GetVar(varType, varName).ToString());
								ConsoleWriter.WriteLine("Попытка отправить необрабатываемый тип данных: " + varType.Name, ConsoleColor.Red);
							}
						}
					}
				}

				return new UniversalOrderContainer(session.TelegramId)
				{
					Items = items,
					Texts = texts,
					Files = files
				};
			};

		public int SessionID { get; }

		public ICollection<(int ID, int Count)> Items { get; set; }

		public ICollection<string> Texts { get; set; }

		public ICollection<(string FileId, string PreviewId, string Description)> Files { get; set; }

		public ICollection<UniversalOrderContainer> Children { get; set; }

		public UniversalOrderContainer(int sessionId)
		{
			SessionID = sessionId;
			Items = new List<(int ID, int Count)>();
			Texts = new List<string>();
			Files = new List<(string FileId, string PreviewId, string Description)>();
			Children = new List<UniversalOrderContainer>();
		}
	}
}
