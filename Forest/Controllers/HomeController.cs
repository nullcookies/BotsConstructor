using System;
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
			if((int)rootParams["type"] != 1)
			{
				return StatusCode(403, "First node is not root node.");
			}
			MegaTree megaTree = new MegaTree(new SimpleNode((string)rootParams["name"], GetReplyMsgFromParams(rootParams), false));
			var treeNodes = new LogicalCore.Node[allNodesCount];
			treeNodes[0] = megaTree.root;
			for(int i = 1; i < allNodesCount; i++)
			{
				var nodeParams = allNodes[i]["parameters"];
				LogicalCore.Node node = null;
				switch ((NodeType)(int)nodeParams["type"])
				{
					case NodeType.Info:
						node = new SimpleNode((string)nodeParams["name"], GetReplyMsgFromParams(nodeParams));
						break;
					case NodeType.Section:
						switch ((CollectionType)(int)nodeParams["collType"])
						{
							case CollectionType.Block:
								node = new BlockNode((string)nodeParams["name"], GetReplyMsgFromParams(nodeParams));
								break;
							case CollectionType.Flipper:
								node = new ChildrenFlipperNode((string)nodeParams["name"], GetDoubleMsgFromParams(nodeParams));
								break;
							default:
								return StatusCode(403, "Incorrect section node's collection type.");
						}
						break;
					case NodeType.Product:
						//TODO: записывать / получать из БД ID элементов
						switch ((DisplayType)(int)nodeParams["displayType"])
						{
							//case DisplayType.Simple:
							//	node = new ProductSimpleNode<decimal>();
							//	break;
							//case DisplayType.Multi:
							//	node = new ProductMultiNode<decimal>((string)nodeParams["name"], productParams.Characteristics.
							//			Select(_chars => _chars.Values).ToList(), "Products", productParams.Characteristics.
							//			SelectMany(_chars => _chars.IDs).ToList(), "ShoppingCart", "Добавлено: ", "Добавить",
							//			new MetaDoubleKeyboardedMessage(productParams.FullDescription, productParams.Message));
							//	break;
							default:
								return StatusCode(403, "Incorrect product node's display type.");
						}
						break;
					case NodeType.Input:
						switch ((InputType)(int)nodeParams["inputType"])
						{
							case InputType.Text:
								node = new TextInputNode((string)nodeParams["name"], (string)nodeParams["name"],
									(string text, out string variable) => !string.IsNullOrWhiteSpace(variable = text),
									GetReplyMsgFromParams(nodeParams));
								break;
							case InputType.Time:
								node = new TimeInputNode((string)nodeParams["name"], (string)nodeParams["name"], GetReplyMsgFromParams(nodeParams));
								break;
							case InputType.Image:
								node = new ImageInputNode((string)nodeParams["name"], (string)nodeParams["name"], noFileCheck, GetReplyMsgFromParams(nodeParams));
								break;
							case InputType.Audio:
								node = new AudioInputNode((string)nodeParams["name"], (string)nodeParams["name"], noFileCheck, GetReplyMsgFromParams(nodeParams));
								break;
							case InputType.Video:
								node = new VideoInputNode((string)nodeParams["name"], (string)nodeParams["name"], noFileCheck, GetReplyMsgFromParams(nodeParams));
								break;
							case InputType.Document:
								node = new DocumentInputNode((string)nodeParams["name"], (string)nodeParams["name"], noFileCheck, GetReplyMsgFromParams(nodeParams));
								break;
							default:
								return StatusCode(403, "Incorrect input node's input type.");
						}
						break;
					case NodeType.SendOrder:
						//TODO: группы статусов
						node = new OwnerNotificationNode((string)nodeParams["name"], GetInlineMsgFromParams(nodeParams), connector, 1,
								(Session session) => new UniversalOrderContainer(session.telegramId)
								{
									Items = session.vars.GetVar<MetaValuedContainer<decimal>>("ShoppingCart").
									Select(_pair => (_pair.Key.ID ?? 0, _pair.Value)).ToArray() // TODO: проверять на null
								},
								variables: new (Type, string)[]
								{
									(typeof(MetaValuedContainer<decimal>), "ShoppingCart")//,
									//(typeof(string), "Address"),
									//(typeof(TimeSpan), "Time"),
									//(typeof(string), "Comment")
								});
						break;
					default:
						return StatusCode(403, "Incorrect node type.");
				}

				treeNodes[i] = node;
				megaTree.AddEdge(treeNodes[(int)allNodes[i]["parentId"]], node);
			}

			MetaReplyMessage GetReplyMsgFromParams(JToken parameters)
			{
				if (!string.IsNullOrWhiteSpace((string)parameters["fileId"]))
				{
					return new MetaReplyMessage(new MetaText((string)parameters["message"]), MessageType.Document, (string)parameters["fileId"]);
				}
				else
				{
					return new MetaReplyMessage(new MetaText((string)parameters["message"]));
				}
			}

			MetaInlineMessage GetInlineMsgFromParams(JToken parameters)
			{
				if (!string.IsNullOrWhiteSpace((string)parameters["fileId"]))
				{
					return new MetaInlineMessage(new MetaText((string)parameters["message"]), MessageType.Document, (string)parameters["fileId"]);
				}
				else
				{
					return new MetaInlineMessage(new MetaText((string)parameters["message"]));
				}
			}

			MetaDoubleKeyboardedMessage GetDoubleMsgFromParams(JToken parameters) => new MetaDoubleKeyboardedMessage(metaReplyText: (string)parameters["message"], metaInlineText: (string)parameters["name"],
				messageType: string.IsNullOrWhiteSpace((string)parameters["fileId"]) ? MessageType.Text : MessageType.Document, messageFile: (string)parameters["fileId"]);
			
			BotWrapper botWrapper = new BotWrapper(botId, null, bot.Token)
			{
				MegaTree = megaTree
			};
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
			foreach (var pair in Products) // Продукты должны добавляться в другом месте!!!
			{
				Item item = _context.Items.Find(pair.Key);
				bool isNew = item == null;
				item = item ?? new Item();
				item.BotId = botId;
				item.Name = pair.Value.Text.ToString();
				item.Value = pair.Value.Value;
				if(isNew)
				{
					_context.Items.Add(item);
				}
				else
				{
					_context.Items.Attach(item).State = EntityState.Modified;
				}
			}
			_context.SaveChanges();

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


