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
