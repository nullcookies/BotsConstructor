using System;

namespace DataLayer.Models
{
	public class OrderModel
	{
		/// <summary>
		/// ID заказа.
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// ID бота, через который был отправлен заказ.
		/// </summary>
		public int BotId { get; set; }
		/// <summary>
		/// Имя отправителя заказа.
		/// </summary>
		public string Sender { get; set; }
		/// <summary>
		/// Сообщение заказа.
		/// </summary>
		public string Message { get; set; }
		/// <summary>
		/// ID статуса (может быть null).
		/// </summary>
		public int? StatusId { get; set; }
		/// <summary>
		/// ID группы статусов.
		/// </summary>
		public int StatusGroupId { get; set; }
		/// <summary>
		/// Время получения сообщения.
		/// </summary>
		public DateTime DateTime { get; set; }
	}
}
