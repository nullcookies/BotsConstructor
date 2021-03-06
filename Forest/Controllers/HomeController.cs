﻿using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer;
using Forest.Services;
using LogicalCore;
using LogicalCore.TreeNodes;
using LogicalCore.TreeNodes.TemplateNodes;
using Microsoft.AspNetCore.Mvc;
using MyLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Forest.Controllers
{
    public class HomeController : Controller
    {
	    
	    
	    //Шо это?
		private readonly TryConvert<(string FileId, string PreviewId, string Description)> noFileCheck = 
			(string text, out (string FileId, string PreviewId, string Description) variable) =>
		{
			variable = default;
			return true;
		};

		private static readonly MetaText priceUnit = new MetaText("грн.");


		private readonly ApplicationContext _contextDb;
		private readonly DbConnector connector;
        private readonly SimpleLogger _logger;

		public HomeController(ApplicationContext context, DbConnector dBConnector, SimpleLogger logger)
        {
            _contextDb = context;
			connector = dBConnector;
            _logger = logger;
        }
		

		

		[HttpPost]
		public IActionResult RunNewBot(int botId)
		{
			try
			{
				_logger.Log(LogLevel.INFO, Source.FOREST, $"Лес. Запуск бота. botId={botId}");

				JObject answer = null;
				var bot = _contextDb.Bots.Find(botId);
				string botUsername = null;


				//Без разметки нельзя
				if (bot.Markup == null)
				{
					answer = new JObject()
						{
							{ "success", false},
							{"failMessage", "Нет разметки." }
						};
					return Json(answer);
				}

				//Без токена нельзя
				if (bot.Token == null)
				{
					answer = new JObject()
						{
							{ "success", false},
							{"failMessage", "Нет токена." }
						};
					return Json(answer);
				}

				//Токен нормальный?
				try
				{
					botUsername = new TelegramBotClient(bot.Token).GetMeAsync().Result.Username;

					var the_link_on_which_the_server_is_running =
						new TelegramBotClient(bot.Token).GetWebhookInfoAsync().Result.Url;

					//Бот уже запущен с вебхуком
					if (!string.IsNullOrEmpty(the_link_on_which_the_server_is_running))
					{
						_logger.Log(LogLevel.WARNING, Source.FOREST, "Запуск бота поверх запущенного webhook-а");
					}
				}
				catch (Exception ee)
				{
					_logger.Log(LogLevel.ERROR, Source.FOREST, "Не удалось узнать botUsername" +
						" у бота с botId" + botId + ". Возможно у бота сохранён битый токен. " + ee.Message);

					answer = new JObject()
						{
							{ "success", false},
							{"failMessage", "Ошибка обработки токена." }
						};
					return Json(answer);
				}

				BotsStorage.BotsDictionary.TryGetValue(botUsername, out IBot _botWrapper);

				//Если найден бот в контейнере
				if (_botWrapper != null)
				{
					_logger.Log(LogLevel.LOGICAL_DATABASE_ERROR, Source.FOREST, $"Лес. Попытка запуска бота, которые уже работает в этом лесу. botId={botId}");

					answer = new JObject()
						{
							{ "success", false},
							{"failMessage", "Бот с username = "+botUsername+" уже запущен." }
						};
					return Json(answer);
				}
				//создание сериализованного объекта дерева
				string link = "https://botsconstructor.com:88/"+botUsername;
//				string link = "https://2264b324.ngrok.io/"+botUsername;
				BotWrapper botWrapper = new BotWrapper(botId, link, bot.Token);
				JArray allNodes = JsonConvert.DeserializeObject<JArray>(bot.Markup);

				List<OwnerNotificationNode> ownerNotificationNodes = new List<OwnerNotificationNode>();
				List<ITreeNode> inputNodes = new List<ITreeNode>();

				if (allNodes.Count == 0)
				{
					answer = new JObject()
							{
								{ "success", false},
								{"failMessage", "Разметка пуста." }
							};
					return Json(answer);
				}

				Dictionary<int, MetaValued<decimal>> Products = new Dictionary<int, MetaValued<decimal>>();
				int allNodesCount = allNodes.Count;
				var rootParams = allNodes[0]["parameters"];
				if ((int)rootParams["type"] != (int)NodeType.Root)
				{
					return StatusCode(403, "First node is not root node.");
				}
				var markupTree = new MarkupTree(new SimpleNode(((string)rootParams["Name"]).Trim(), GetReplyMsgFromParams(rootParams), false));
				botWrapper.MarkupTree = markupTree;
				var treeNodes = new ITreeNode[allNodesCount];
				treeNodes[0] = markupTree.Root;
				var variablesInfo = new HashSet<(Type type, string name)>()
				{
					(typeof(MetaValuedContainer<decimal>), "ShoppingCart")
				};
				for (int i = 1; i < allNodesCount; i++)
				{
					var nodeParams = allNodes[i]["parameters"];
					string nodeName = ((string)nodeParams["Name"]).Trim();
					ITreeNode node = null;
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

							if (metaTexts.Count != prices.Count)
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

							_contextDb.Items.AddRange(productItems);
							_contextDb.SaveChanges();

							List<int> IDs = productItems.Select((_item) => _item.Id).ToList();

							for (int j = 0; j < metaTexts.Count; j++)
							{
								Products.Add(IDs[j], new MetaValued<decimal>(metaTexts[j], prices[j], priceUnit, id: IDs[j]));
							}

							if (IDs.Count > 1)
							{
								switch ((DisplayType)(int)nodeParams["displayType"])
								{
									case DisplayType.Simple:
										List<MetaReplyMessage> foldersMsgs = nodeParams["properties"].Select(GetReplyMsgFromParams).ToList();
										node = new ProductSimpleNode<decimal>(nodeName, elements, "Products", IDs, foldersMsgs, "ShoppingCart", "Добавлено: ", "Добавить", GetInlineMsgFromParams(nodeParams));
										var parentNode = treeNodes[(int)allNodes[i]["parentId"]];
										if (parentNode is BlockNode)
										{
                                            ITreeNode middleNode = new LightNode(nodeName, GetInlineMsgFromParams(nodeParams));
											middleNode.AddChildWithButtonRules(((ICombined)node).HeadNode);
											((ITeleportable)node).SetPortal(parentNode);
											node = middleNode;
										}
										break;
									case DisplayType.Multi:
										List<MetaText> foldersNames = nodeParams["properties"].Select((section) => new MetaText(((string)section["Name"]).Trim())).ToList();
										node = new ProductMultiNode<decimal>(nodeName, elements, "Products", IDs, foldersNames, "ShoppingCart", "Добавлено: ", "Добавить", GetDoubleMsgFromParams(nodeParams));
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
							inputNodes.Add(node);
							break;
						case NodeType.SendOrder:
							node = new OwnerNotificationNode(nodeName, GetInlineMsgFromParams(nodeParams), connector, (int)nodeParams["statusGroupId"],
									UniversalOrderContainer.generateContainerCreator(variablesInfo),
									variablesInfo);
							ownerNotificationNodes.Add((OwnerNotificationNode)node);
							break;
						default:
							return StatusCode(403, "Incorrect node type.");
					}

					treeNodes[i] = node;
					markupTree.AddEdge(treeNodes[(int)allNodes[i]["parentId"]], node);
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
						string name = ((string)parameters["Name"]).Trim();
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
						return new MetaDoubleKeyboardedMessage(metaReplyText: (string)parameters["message"], metaInlineText: ((string)parameters["Name"]).Trim(),
							useReplyMsgForFile: true, messageType: GetMessageTypeByFileId(fileId), messageFile: fileId);
					}
					else
					{
						return new MetaDoubleKeyboardedMessage(metaReplyText: (string)parameters["message"], metaInlineText: ((string)parameters["Name"]).Trim());
					}
				}

				botWrapper.InitializeSessionVars = (vars) =>
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
				botWrapper.GlobalVars.SetVar("Products", Products);

				
				//there are no data entry nodes in the markup that are not in the same branch
		

				if (!CheckForLackOfShit(ownerNotificationNodes, inputNodes))
				{
					answer = new JObject()
					{
						{ "success", false},
						{"failMessage", "В разметке есть узлы типа \"Ввод данных\", которые находятся не в одной ветке" }
					};
					return Json(answer);
				}
				
				
				bool synchronization_was_successful =
					RecordOfTheLaunchOfTheBotWasMadeSuccessfully(botId);

				if (!synchronization_was_successful)
				{
					answer = new JObject()
							{
								{ "success", false},
								{"failMessage", "Не удалось сделать запись в БД о запущенном боте." }
							};
					return Json(answer);
				}

				BotsStorage.RunAndRegisterBot(botWrapper);

				answer = new JObject()
				{
					{ "success", true}
				};

				return Json(answer);
			}
			catch (Exception ex)
			{
				var answer = new JObject()
				{
					{ "success", false},
					{"failMessage", ex.Message }
				};

				return Json(answer);
			}
		}
		
		bool CheckForLackOfShit(List<OwnerNotificationNode> ownerNotificationNodes, List<ITreeNode> inputNodes)
		{
			if (inputNodes.Count > 0 )
			{
				switch (ownerNotificationNodes.Count)
				{
					case 0:
						return false;
					case 1:
					{
						var notificationNode = ownerNotificationNodes[0];
                        ITreeNode current = notificationNode.Parent;
						while (current != null)
						{
							inputNodes.Remove(current);
							current = current.Parent;
						}

						return inputNodes.Count == 0;
					}
					default:
						return false;
				}
			}

			return true;

		}
		
		
		
        /// <summary>
        /// Создание записи в БД, чтобы знать где запущен бот
        /// </summary>
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

            BotLaunchRecord blr = new BotLaunchRecord()
            {
                BotId = botId,
                BotStatus = BotStatus.STARTED,
                Time= DateTime.UtcNow
            };


            //Выбор записи из БД
            RouteRecord rrDb = _contextDb.RouteRecords.Where(_rr => _rr.BotId == botId).SingleOrDefault();

            if (rrDb != null)
            {
                //В базе уже запись о том, что бот запущен
                _logger.Log(LogLevel.LOGICAL_DATABASE_ERROR, Source.FOREST, $"Лес. Запуск бота. В БД уже существует запись о том, что бот запущен. botId={botId}");

                //перезапись значения
                _logger.Log(LogLevel.LOGICAL_DATABASE_ERROR, Source.FOREST, $"Лес. Запуск бота. Перезапись значения. botId={botId}");

                rrDb.ForestLink = rr.ForestLink;
            }
            else
            {
                _logger.Log(LogLevel.INFO, Source.FOREST, $" Создание новой записи о запущеном боте" + $"{rr.BotId}  {rr.ForestLink}");
                _contextDb.RouteRecords.Add(rr);
            }
            _contextDb.BotLaunchRecords.Add(blr);

            _contextDb.BotWorkLogs.Add(new BotWorkLog() { BotId = botId, InspectionTime = DateTime.UtcNow });
            
            _contextDb.SaveChanges();

            return true;
        }


        //Если сменить токен боту во время работы, то его будет невозможно остановить
        [HttpPost]
        public IActionResult StopBot(int botId)
        {
            //TODO Авторизация через бд

            BotDB botDb = _contextDb.Bots.Find(botId);

            string botUsername = new TelegramBotClient(botDb.Token).GetMeAsync().Result.Username;

            if ( BotsStorage.BotsDictionary.TryGetValue(botUsername, out IBot botWrapper))
            {
                Console.WriteLine("  BotsContainer.BotsDictionary.TryGetValue(botUsername, out BotWrapper botWrappe              ");
                if (botWrapper != null)
                {
                    botWrapper.Stop();
                    Console.WriteLine("         if (botWrapper != null)        ");

                    //удаление бота из памяти
                    BotsStorage.BotsDictionary.Remove(botUsername);
                    Console.WriteLine("         BotsContainer.BotsDictionary.Remove(bot       ");

                    //очистка БД
                    RouteRecord rr = _contextDb.RouteRecords.Find(botId);
                    if (rr != null)
                    {
                        _contextDb.RouteRecords.Remove(rr);
                        _contextDb.SaveChanges();
                    }
                    else
                    {
                        _logger.Log(LogLevel.LOGICAL_DATABASE_ERROR, Source.FOREST, "Куда успела деться RouteRecord?");
                        Console.WriteLine("     ogger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, Куда успела деться RouteRecord ? )   ");

                        
                    }
                
                   
                    //ответ сайту 
                    return Ok();
                }
                else
                {
                    _logger.Log(LogLevel.LOGICAL_DATABASE_ERROR, Source.FOREST, $"Лес принял запрос на остановку " +
                        $"бота, которого у него нет. botId={botId} botUsername={botUsername}. В словаре" +
                        $" ботов хранился botWrapper==null.");
                }
            }
            else
            {
                _logger.Log(LogLevel.LOGICAL_DATABASE_ERROR, Source.FOREST, $"Лес принял запрос на остановку бота," +
                    $" которого у него нет. botId={botId} botUsername={botUsername}");
                
                //попытка удалить неправильный route record

                var maliciousRouteRecord = _contextDb.RouteRecords.SingleOrDefault(_rr => _rr.BotId == botId);
                if (maliciousRouteRecord != null)
                {
	                _contextDb.RouteRecords.Remove(maliciousRouteRecord);
	                _contextDb.SaveChanges();
                }
            }
            return StatusCode(500);
        }
    }



}


