using System;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using DataLayer.Models;
using LogicalCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Forest.Services
{
	public class DbConnector : IOrdersSendable
	{
		private readonly ApplicationContext _contextDb;

		public DbConnector(IConfiguration configuration, IHostingEnvironment environment)
		{
            _contextDb = new ApplicationContext(
				new DbContextOptionsBuilder<ApplicationContext>()
				.UseNpgsql(DbContextFactory.GetConnectionString(environment))
				.Options
			);
		}

		public async Task<bool> SendOrder(Session session, UniversalOrderContainer order, int statGroupId)
		{
			if (order == null) return false;
			bool noItems = order.Items == null || order.Items.Count == 0;
			bool noTexts = order.Texts == null || order.Texts.Count == 0;
			bool noFiles = order.Files == null || order.Files.Count == 0;
			if (noItems && noTexts && noFiles) return false;

			try
			{
				_contextDb.Orders.Add(new Order()
				{
					SenderId = session.telegramId,
					SenderNickname = session.User.FirstName + " " + session.User.LastName,
					BotId = session.BotWrapper.BotID,
					DateTime = DateTime.UtcNow,
					OrderStatusGroupId = statGroupId,
					Container = CreateInventories(order)
				});

				await _contextDb.SaveChangesAsync();
				return true;
			}
			catch (Exception ex)
			{
				ConsoleWriter.WriteLine(ex.Message, ConsoleColor.Red);
				return false;
			}

			Inventory CreateInventories(UniversalOrderContainer currentOrder, Inventory parent = null)
			{
				Inventory currentInventory = new Inventory()
				{
					Parent = parent,
					SessionId = session.telegramId,
					Items = currentOrder.Items.Select(tuple => new InventoryItem()
					{
						ItemId = tuple.ID,
						Count = tuple.Count
					}).ToArray(),
					Texts = currentOrder.Texts.Select(text => new SessionText()
					{
						Text = text
					}).ToArray(),
					Files = currentOrder.Files.Select(file => new SessionFile()
					{
						FileId = file.FileId,
						PreviewId = file.PreviewId,
						Description = file.Description
					}).ToArray()
				};

				foreach (var child in currentOrder.Children)
				{
					Inventory childInventory = CreateInventories(currentOrder, currentInventory);
				}

				return currentInventory;
			}
		}
	}
}
