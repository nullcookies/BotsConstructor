﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DeleteMeWebhook.Models;
using DeleteMeWebhook.Services;
using LogicalCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using DataLayer.Models;
using DataLayer.Services;
using System.Threading;

namespace DeleteMeWebhook.Controllers
{
    public class HomeController : Controller
    {
		private readonly TryConvert<(string FileId, string PreviewId, string Description)> noFileCheck = (string text, out (string FileId, string PreviewId, string Description) variable) =>
		{
			variable = default((string FileId, string PreviewId, string Description));
			return true;
		};

		//TODO: реализовать возможность менять для бота
		private static readonly MetaText priceUnit = new MetaText("грн.");

		private readonly ApplicationContext _context;
		private readonly DBConnector connector;
        private readonly StupidLogger _logger;

		public HomeController(ApplicationContext context, DBConnector dBConnector, StupidLogger logger)
        {
            _context = context;
			connector = dBConnector;
            _logger = logger;
        }

        [Route("{telegramBotUsername}")]
        public IActionResult Index([FromBody]Update update)
        {

            string botUsername = RouteData.Values["telegramBotUsername"].ToString();

            BotsContainer.BotsDictionary[botUsername]?.AcceptUpdate(update);
            if (BotsContainer.BotsDictionary.TryGetValue(botUsername, out BotWrapper botWrapper))
            {
                botWrapper.AcceptUpdate(update);
            }
            else
            {
                ConsoleWriter.WriteLine("Пришло обновление для бота, которого нет в списке онлайн ботов", ConsoleColor.Red);
                ConsoleWriter.WriteLine("BotUsername="+botUsername, ConsoleColor.Red);
            }

            return Ok();
        }

        [HttpPost]
        public IActionResult RunNewBot(int botId)
        {
            //Аунтефикация

            _logger.Log(LogLevelMyDich.INFO, $"Лес. Запуск бота. botId={botId}");
            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
            Console.WriteLine($"Лес. Запуск бота. botId={botId}");
            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
            //проверка возможности запуска
            var bot = _context.Bots.Find(botId);
            string botUsername = new TelegramBotClient(bot.Token).GetMeAsync().Result.Username;
            //По токену узнать имя 
            BotsContainer.BotsDictionary.TryGetValue(botUsername, out BotWrapper _botWrapper);

            if (_botWrapper != null)
            {
                _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, $"Лес. Попытка запуска бота, которые уже работает в этом лесу. botId={botId}");
                return StatusCode(403, "Такой бот уже запущен");
            }

            ////проверить наличие адекватной разметки
            if (bot.Markup == null)
            {
                return StatusCode(403, "Markup was null.");
            }

            ////проверить наличие токена
            if (bot.Token == null)
            {
                return StatusCode(403, "Token was null.");
            }

			BotWrapper botWrapper = new BotWrapper(botId, null, bot.Token);
			////проверить наличие достаточного кол-ва денег

			var owner = _context.Accounts.Find(bot.OwnerId);
            //var money = owner.Money;

            //создание сериализованного объекта дерева
            JArray allNodes = JsonConvert.DeserializeObject<JArray>(bot.Markup);

			if(allNodes.Count == 0)
			{
				return StatusCode(403, "Zero objects count.");
			}

			Dictionary<int, MetaValued<decimal>> Products = new Dictionary<int, MetaValued<decimal>>();
			int allNodesCount = allNodes.Count();
			var rootParams = allNodes[0]["parameters"];
			if((int)rootParams["type"] != (int)NodeType.Root)
			{
				return StatusCode(403, "First node is not root node.");
			}
			MegaTree megaTree = new MegaTree(new SimpleNode((string)rootParams["name"], GetReplyMsgFromParams(rootParams), false));
			botWrapper.MegaTree = megaTree;
			var treeNodes = new Node[allNodesCount];
			treeNodes[0] = megaTree.root;
			var variablesInfo = new List<(Type type, string name)>()
			{
				(typeof(MetaValuedContainer<decimal>), "ShoppingCart")
			};
			for (int i = 1; i < allNodesCount; i++)
			{
				var nodeParams = allNodes[i]["parameters"];
				string nodeName = (string)nodeParams["name"];
				Node node = null;
				switch ((NodeType)(int)nodeParams["type"])
				{
					case NodeType.Info:
						node = new SimpleNode(nodeName, GetReplyMsgFromParams(nodeParams));
						break;
					case NodeType.Section:
						switch ((CollectionType)(int)nodeParams["collType"])
						{
							case CollectionType.Block:
								node = new BlockNode(nodeName, GetReplyMsgFromParams(nodeParams));
								break;
							case CollectionType.Flipper:
								node = new ChildrenFlipperNode(nodeName, GetDoubleMsgFromParams(nodeParams));
								break;
							default:
								return StatusCode(403, "Incorrect section node's collection type.");
						}
						break;
					case NodeType.Product:
						List<List<string>> elements = nodeParams["properties"].Select((section) => section["types"].ToObject<List<string>>()).ToList();
						List<decimal> prices = nodeParams["values"].ToObject<List<decimal>>();

						List<MetaText> metaTexts = new List<MetaText>();
						string[] protoString = new string[elements.Count - 1];

						RecursiveAdder();

						void RecursiveAdder(int index = 0)
						{
							if (index < elements.Count - 1)
							{
								for (int j = 0; j < elements[index].Count; j++)
								{
									protoString[index] = elements[index][j];
									RecursiveAdder(index + 1);
								}
							}
							else
							{
								for (int j = 0; j < elements[index].Count; j++)
								{
									MetaText metaText = new MetaText(nodeName, " ");
									foreach (var part in protoString)
									{
										metaText.Append(part, " ");
									}
									metaText.Append(elements[index][j]);
									metaTexts.Add(metaText);
								}
							}
						}

						if(metaTexts.Count != prices.Count)
						{
							return StatusCode(403, "Incorrect product node's parameters and prices numbers.");
						}

						Item[] productItems = new Item[metaTexts.Count];
						for (int j = 0; j < metaTexts.Count; j++)
						{
							productItems[j] = new Item()
							{
								BotId = botId,
								Name = metaTexts[j].ToString(),
								Value = prices[j]
							};
						}

						_context.Items.AddRange(productItems);
						_context.SaveChanges();

						List<int> IDs = productItems.Select((_item) => _item.Id).ToList();

						for(int j = 0; j < metaTexts.Count; j++)
						{
							Products.Add(IDs[j], new MetaValued<decimal>(metaTexts[j], prices[j], priceUnit, id: IDs[j]));
						}

						if(IDs.Count > 1)
						{
							switch ((DisplayType)(int)nodeParams["displayType"])
							{
								case DisplayType.Simple:
									List<MetaReplyMessage> foldersMsgs = nodeParams["properties"].Select((section) => GetReplyMsgFromParams(section)).ToList();
									node = new ProductSimpleNode<decimal>(nodeName, elements, "Products", IDs, foldersMsgs, "ShoppingCart", "Добавлено: ", "Добавить", GetDoubleMsgFromParams(nodeParams));
									Node parentNode = treeNodes[(int)allNodes[i]["parentId"]];
									if (parentNode is BlockNode)
									{
										Node middleNode = new LightNode(nodeName, GetInlineMsgFromParams(nodeParams));
										middleNode.AddChildWithButtonRules(((ICombined)node).HeadNode);
										((ITeleportable)node).SetPortal(parentNode);
										node = middleNode;
									}
									break;
								case DisplayType.Multi:
									node = new ProductMultiNode<decimal>(nodeName, elements, "Products", IDs, "ShoppingCart", "Добавлено: ", "Добавить", GetDoubleMsgFromParams(nodeParams));
									break;
								default:
									return StatusCode(403, "Incorrect product node's display type.");
							}
						}
						else
						{
							node = new ItemNode<decimal>("Products", IDs[0], "ShoppingCart", GetInlineMsgFromParams(nodeParams), nodeName);
						}
						break;
					case NodeType.Input:
						switch ((InputType)(int)nodeParams["inputType"])
						{
							case InputType.Text:
								node = new TextInputNode(nodeName, nodeName,
									(string text, out string variable) => !string.IsNullOrWhiteSpace(variable = text),
									GetReplyMsgFromParams(nodeParams));
								variablesInfo.Add((typeof(string), nodeName));
								break;
							case InputType.Time:
								node = new TimeInputNode(nodeName, nodeName, GetReplyMsgFromParams(nodeParams));
								variablesInfo.Add((typeof(TimeSpan), nodeName));
								break;
							case InputType.Image:
								node = new ImageInputNode(nodeName, nodeName, noFileCheck, GetReplyMsgFromParams(nodeParams));
								variablesInfo.Add((typeof((string FileId, string PreviewId, string Description)), nodeName));
								break;
							case InputType.Audio:
								node = new AudioInputNode(nodeName, nodeName, noFileCheck, GetReplyMsgFromParams(nodeParams));
								variablesInfo.Add((typeof((string FileId, string PreviewId, string Description)), nodeName));
								break;
							case InputType.Video:
								node = new VideoInputNode(nodeName, nodeName, noFileCheck, GetReplyMsgFromParams(nodeParams));
								variablesInfo.Add((typeof((string FileId, string PreviewId, string Description)), nodeName));
								break;
							case InputType.Document:
								node = new DocumentInputNode(nodeName, nodeName, noFileCheck, GetReplyMsgFromParams(nodeParams));
								variablesInfo.Add((typeof((string FileId, string PreviewId, string Description)), nodeName));
								break;
							default:
								return StatusCode(403, "Incorrect input node's input type.");
						}
						break;
					case NodeType.SendOrder:
						//TODO: группы статусов
						node = new OwnerNotificationNode(nodeName, GetInlineMsgFromParams(nodeParams), connector, 1,
								UniversalOrderContainer.generateContainerCreator(variablesInfo),
								variables: variablesInfo.ToArray());
						break;
					default:
						return StatusCode(403, "Incorrect node type.");
				}

				treeNodes[i] = node;
				megaTree.AddEdge(treeNodes[(int)allNodes[i]["parentId"]], node);
			}

			MetaText GetMetaTextFromParams(JToken parameters)
			{
				string message = (string)parameters["message"];
				if (!string.IsNullOrWhiteSpace(message))
				{
					return new MetaText(message);
				}
				else
				{
					string name = (string)parameters["name"];
					if (!string.IsNullOrWhiteSpace(name))
					{
						return new MetaText(name);
					}
					else
					{
						ConsoleWriter.WriteLine("Обнаружен узел без названия. Используется стандартное название.", ConsoleColor.Yellow);
						return new MetaText("node");
					}
				}
			}

			MessageType GetMessageTypeByFileId(string fileId)
			{
				string filePath = botWrapper.BotClient.GetFileAsync(fileId).Result.FilePath;
				switch (filePath.Split('/')[0])
				{
					case "photos":
						return MessageType.Photo;
					case "stickers":
						return MessageType.Sticker;
					case "documents":
						return MessageType.Document;
					case "voice":
						return MessageType.Voice;
					case "video_notes":
						return MessageType.VideoNote;
					case "music":
						return MessageType.Audio;
					case "videos":
						return MessageType.Video;
					case "animations":
						return MessageType.Video;
					default:
						ConsoleWriter.WriteLine("Неизвестный тип сообщения, путь: " + filePath, ConsoleColor.Red);
						return MessageType.Unknown;
				}
			}

			MetaReplyMessage GetReplyMsgFromParams(JToken parameters)
			{
				string fileId = (string)parameters["fileId"];
				if (!string.IsNullOrWhiteSpace(fileId))
				{
					return new MetaReplyMessage(new MetaText(GetMetaTextFromParams(parameters)), GetMessageTypeByFileId(fileId), fileId);
				}
				else
				{
					return new MetaReplyMessage(new MetaText(GetMetaTextFromParams(parameters)));
				}
			}

			MetaInlineMessage GetInlineMsgFromParams(JToken parameters)
			{
				string fileId = (string)parameters["fileId"];
				if (!string.IsNullOrWhiteSpace(fileId))
				{
					return new MetaInlineMessage(new MetaText(GetMetaTextFromParams(parameters)), GetMessageTypeByFileId(fileId), fileId);
				}
				else
				{
					return new MetaInlineMessage(new MetaText(GetMetaTextFromParams(parameters)));
				}
			}

			MetaDoubleKeyboardedMessage GetDoubleMsgFromParams(JToken parameters)
			{
				string fileId = (string)parameters["fileId"];
				if (!string.IsNullOrWhiteSpace(fileId))
				{
					return new MetaDoubleKeyboardedMessage(metaReplyText: (string)parameters["message"], metaInlineText: (string)parameters["name"],
						useReplyMsgForFile: true, messageType: GetMessageTypeByFileId(fileId), messageFile: fileId);
				}
				else
				{
					return new MetaDoubleKeyboardedMessage(metaReplyText: (string)parameters["message"], metaInlineText: (string)parameters["name"]);
				}
			}

			botWrapper.InitializeSessionVars = (VariablesContainer vars) =>
			{
				vars.SetVar(new MetaValuedContainer<decimal>("ShoppingCart", finalFunc: (dict) =>
				{
					if (dict.Count == 0) return new MetaText("Cost", "0.00");

					MetaValued<decimal> result = new MetaValued<decimal>();

					bool first = true;

					foreach (var pair in dict)
					{
						if (first)
						{
							result = new MetaValued<decimal>("Cost", 0, pair.Key.Unit, pair.Key.UseSpaceForUnit, pair.Key.UnitBeforeValue);
							first = false;
						}

						result.Value += pair.Key.Value * pair.Value;
					}

					return result;
				}));
			};
			botWrapper.globalVars.SetVar("Products", Products);

            bool synchronization_was_successful = RecordOfTheLaunchOfTheBotWasMadeSuccessfully(botId);

            if (!synchronization_was_successful)
            {
                return StatusCode(500);
            }

            Stub.RunAndRegisterBot(botWrapper);

			return Ok();
        }

        /// <summary>
        /// Создание записи в БД, чтобы знать где запущен бот
        /// </summary>
        /// <param name="botId"></param>
        /// <returns></returns>
        private bool RecordOfTheLaunchOfTheBotWasMadeSuccessfully(int botId)
        {
            //Создание новой записи
            string domain = HttpContext.Request.Host.Value;
            var link = $"http://{domain}";
            RouteRecord rr = new RouteRecord()
            {
                BotId = botId,
                ForestLink = link
            };

            //Выбор записи из БД
            RouteRecord rrDb = _context.RouteRecords.Where(_rr => _rr.BotId == botId).SingleOrDefault();

            if (rrDb != null)
            {
                //В базе уже запись о том, что бот запущен
                _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, $"Лес. Запуск бота. В БД уже существует запись о том, что бот запущен. botId={botId}");

                //перезапись значения
                _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, $"Лес. Запуск бота. Перезапись значения. botId={botId}");

                rrDb.ForestLink = rr.ForestLink;

                //return false;
            }
            else
            {
                Console.WriteLine("Создание новой записи о запущеном боте" + $"{rr.BotId}  {rr.ForestLink}");
                _context.RouteRecords.Add(rr);
            }

            _context.SaveChanges();
            return true;
        }


        [HttpPost]
        public IActionResult StopBot(int botId)
        {
            //TODO Авторизация через бд

            BotDB botDb = _context.Bots.Find(botId);
            BotWrapper _botWrapper = new BotWrapper(botId, null, botDb.Token);
            string botUsername = _botWrapper.BotClient.GetMeAsync().Result.Username;

            if ( BotsContainer.BotsDictionary.TryGetValue(botUsername, out BotWrapper botWrapper))
            {
                Console.WriteLine("  BotsContainer.BotsDictionary.TryGetValue(botUsername, out BotWrapper botWrappe              ");
                if (botWrapper != null)
                {
                    botWrapper.Stop();
                    Console.WriteLine("         if (botWrapper != null)        ");

                    //удаление бота из памяти
                    BotsContainer.BotsDictionary.Remove(botUsername);
                    Console.WriteLine("         BotsContainer.BotsDictionary.Remove(bot       ");

                    //очистка БД
                    RouteRecord rr = _context.RouteRecords.Find(botId);
                    if (rr != null)
                    {
                        _context.RouteRecords.Remove(rr);
                        _context.SaveChanges();
                    }
                    else
                    {
                        _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, "Куда успела деться RouteRecord?");
                        Console.WriteLine("     ogger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, Куда успела деться RouteRecord ? )   ");

                        
                    }
                
                   
                    //ответ сайту 
                    return Ok();
                }
                else
                {
                    _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, $"Лес принял запрос на остановку " +
                        $"бота, которого у него нет. botId={botId} botUsername={botUsername}. В словаре" +
                        $" ботов хранился botWrapper==null.");
                }
            }
            else
            {
                _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, $"Лес принял запрос на остановку бота," +
                    $" которого у него нет. botId={botId} botUsername={botUsername}");
            }
            return StatusCode(500);
        }
    }

	enum NodeType
	{
		Unknown = 0,
		Root = 1,
		Info = 2,
		Section = 3,
		Product = 4,
		Input = 5,
		SendOrder = 6
	}

	enum CollectionType
    {
		Unknown = 0,
		Block = 1,
        Flipper = 2
	}

	enum DisplayType
	{
		Unknown = 0,
		Simple = 1,
		Multi = 2
	}

	enum InputType
	{
		Unknown = 0,
		Text = 1,
		Time = 2,
		Image = 3,
		Audio = 4,
		Video = 5,
		Document = 6
	}

}


