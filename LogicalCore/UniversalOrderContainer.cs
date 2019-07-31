using System.Collections.Generic;

namespace LogicalCore
{
	/// <summary>
	/// Универсальный контейнер заказа, в который можно поместить всё, что может пригодиться.
	/// </summary>
	public class UniversalOrderContainer
	{
		public int SessionID { get; }

		public ICollection<(int ID, int Count)> Items { get; set; }

		public ICollection<string> Texts { get; set; }

		public ICollection<string> FilesIDs { get; set; }

		public ICollection<UniversalOrderContainer> Children { get; set; }

		public UniversalOrderContainer(int sessionId)
		{
			SessionID = sessionId;
			Items = new List<(int ID, int Count)>();
			Texts = new List<string>();
			FilesIDs = new List<string>();
			Children = new List<UniversalOrderContainer>();
		}
	}
}
