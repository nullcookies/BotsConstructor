using System.Collections.Generic;

namespace website.Models
{
	public class OrderStatusesModel
	{
		/// <summary>
		/// Словарь статусов {ID статуса,(название кнопки, сообщение отправки)}.
		/// </summary>
		public Dictionary<int, (string name, string message)> Statuses { get; set; }
		/// <summary>
		/// Словарь групп статусов {ID группы, {название группы, список ID статусов}}.
		/// </summary>
		public Dictionary<int, (string name, List<int> statuses)> StatusGroups { get; set; }

		public OrderStatusesModel()
		{
			Statuses = new Dictionary<int, (string name, string message)>();
			StatusGroups = new Dictionary<int, (string name, List<int> statuses)>();
		}
	}
}
