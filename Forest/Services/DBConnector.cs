using DeleteMeWebhook.Models;
using LogicalCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataLayer.Models;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;

namespace DeleteMeWebhook.Services
{
	public class DBConnector : IOrdersSendable
	{
		private readonly ApplicationContext contextDb;

		public DBConnector(IConfiguration configuration)
		{

            string connection;

            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (isWindows)
            {
                connection = configuration.GetConnectionString("PostgresConnectionDevelopment");
            }
            else
            {
                connection = configuration.GetConnectionString("PostgresConnectionLinux");
            }


            contextDb = new ApplicationContext(
				new DbContextOptionsBuilder<ApplicationContext>()
				.UseNpgsql(connection)
				.Options
			);
		}

		public async Task<bool> SendOrder(Session session, UniversalOrderContainer order, int statGroupID)
		{
			if (order == null) return false;
			bool noItems = order.Items == null || order.Items.Count == 0;
			bool noTexts = order.Texts == null || order.Texts.Count == 0;
			bool noFiles = order.Files == null || order.Files.Count == 0;
			if (noItems && noTexts && noFiles) return false;

			try
			{
				contextDb.Orders.Add(new Order()
				{
					SenderId = session.telegramId,
					SenderNickname = session.User.FirstName + " " + session.User.LastName,
					BotId = session.BotWrapper.BotID,
					DateTime = DateTime.UtcNow,
					OrderStatusGroupId = statGroupID,
					Container = CreateInventories(order)
				});

				await contextDb.SaveChangesAsync();
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
