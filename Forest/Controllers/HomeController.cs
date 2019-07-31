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

            

            //проверка возможности запуска
            var bot = _context.Bots.Find(botId);




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
            JArray testObj = JsonConvert.DeserializeObject<JArray>(bot.Markup);

			if(testObj.Count == 0)
			{
				return StatusCode(403, "Zero objects count.");
			}

			Dictionary<int, MetaValued<decimal>> Products = new Dictionary<int, MetaValued<decimal>>();

			var nodesDepthLine = new LogicalCore.Node[testObj.Count()];
			MegaTree megaTree = null;

			int productIdStub = 0;
            for (int y = 0; y < testObj[0].Count(); y++)
            {
                for (int x = 0; x < testObj.Count(); x++)
                {
					var nodeJson = testObj[x][y];
                    if (nodeJson.HasValues)
                    {
						LogicalCore.Node logicalNode = null;
                        string nodeName = (string)nodeJson["nodeName"];
                        int _x = (int)nodeJson["x"];
                        int _y = (int)nodeJson["y"];
						NodeType nodeType = (NodeType)(int)nodeJson["nodeType"];
                        Node node = new Node() { NodeName = nodeName, X = _x, Y = _y };

                        switch (nodeType)
                        {
							case NodeType.Unknown:
								return StatusCode(403, "Unknown node type.");

							case NodeType.Root:
								logicalNode = node.ToSimpleNode(false);
								megaTree = new MegaTree(logicalNode);
								break;

                            case NodeType.NewOrder:
								logicalNode = node.ToSimpleNode();
								megaTree.AddEdge(nodesDepthLine[x - 1], logicalNode);
								break;

                            case NodeType.Section:
								node.NodeParams = new CollectionNodeParams()
								{
									Message = (string)nodeJson["nodeParams"]["message"],
									ScrollingMethod = (ScrollingMethod)(int)nodeJson["nodeParams"]["scrollingMethod"],
								};

								logicalNode = node;
								megaTree.AddEdge(nodesDepthLine[x - 1], logicalNode);
								break;

                            case NodeType.Product:
                                long productId = (long)nodeJson["nodeParams"]["_productId"];
                                string message = (string)nodeJson["nodeParams"]["message"];
                                string fullDescription = (string)nodeJson["nodeParams"]["fullDescription"];
                                List<Characteristic> chars = new List<Characteristic>();
                                JArray characteristics = (JArray)nodeJson["nodeParams"]["characteristics"];

                                foreach (var characteristic in characteristics)
                                {
                                    string charName = (string)characteristic["characteristicName"];
                                    Characteristic temChar = new Characteristic() { CharacteristicName = charName };

                                    for (int qq = 0; qq < characteristic["arrOfValues"].Count(); qq++)
                                    {
                                        temChar.Values.Add((string)characteristic["arrOfValues"][qq]);
                                    }
                                    chars.Add(temChar);

                                }
                                JArray pricesJson = (JArray)nodeJson["nodeParams"]["prices"];
                                List<string> prices = new List<string>();
                                for (int ww = 0; ww < pricesJson.Count(); ww++)
                                {
                                    prices.Add((string)pricesJson[ww]);
                                }

                                node.NodeParams = new ProductNodeParams()
                                {
                                    ProductId = productId,
									Message = message,
                                    FullDescription = fullDescription,
                                    Characteristics = chars
                                };

								string[] protoString = new string[chars.Count - 1];

								RecursiveAdder();

								void RecursiveAdder(int index = 0)
								{
									if (index < chars.Count - 1)
									{
										for (int i = 0; i < chars[index].Values.Count; i++)
										{
											protoString[index] = chars[index].Values[i];
											RecursiveAdder(index + 1);
										}
									}
									else
									{
										for (int i = 0; i < chars[index].Values.Count; i++)
										{
											MetaText metaText = new MetaText(node.NodeName, " ");
											foreach (var part in protoString)
											{
												metaText.Append(part, " ");
											}
											metaText.Append(chars[index].Values[i]);
											Products.Add(productIdStub,
												new MetaValued<decimal>(metaText, 160.15m, "american hryvnia", id: productIdStub));
											chars[index].IDs.Add(productIdStub);
											productIdStub++;
										}
									}
								}

								logicalNode = node;
								megaTree.AddEdge(nodesDepthLine[x - 1], logicalNode);
								megaTree.AddEdge(logicalNode, nodesDepthLine[x - 1]);
								break;

                            case NodeType.ConfirmOrder:
								logicalNode = node;
								megaTree.AddEdge(nodesDepthLine[x - 1], logicalNode);
								break;

                            case NodeType.Input:
                                bool answerIsRequired = (bool)nodeJson["nodeParams"]["answerIsRequired"];

                                Dictionary<string, bool> expectedResponseFormat = new Dictionary<string, bool>();
                                var expFormats = nodeJson["nodeParams"]["expectedResponseFormat"];
                                foreach (var pair in expFormats)
                                {
                                    var test89274658 = (string)pair["nameOfValue"];
                                    var test892658 = (bool)pair["value"];
                                    //expectedResponseFormat.Add(((JProperty)pair).Name, (bool)((JProperty)pair).Value);
                                    expectedResponseFormat.Add(test89274658, test892658);
                                }
                                string _message = (string)nodeJson["nodeParams"]["message"];

                                node.NodeParams = new InputNodeParams()
                                {
                                    Message = _message,
                                    AnswerIsRequired = answerIsRequired,
                                    ExpectedResponseFormat = expectedResponseFormat
                                };

								logicalNode = node;
								megaTree.AddEdge(nodesDepthLine[x - 1], logicalNode);
								break;

                            case NodeType.SendOrder:
                                string message3 = (string)nodeJson["nodeParams"]["message"];
                                node.NodeParams = new SendOrderParams() { Message = message3, Sendable = connector };

								logicalNode = node;
								megaTree.AddEdge(nodesDepthLine[x - 1], logicalNode);
								break;

                        }
						
						nodesDepthLine[x] = logicalNode;
					}
                }
            }

			BotWrapper botWrapper = new BotWrapper(botId, null, bot.Token, null, null, null)
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

            RouteRecord existingRouteRecord = _context.RouteRecords.Where(_rr => _rr.BotId == botId).SingleOrDefault();

            if (existingRouteRecord != null)
            {
                //В базе уже запись о том, что бот запущен
                _logger.Log(LogLevelMyDich.LOGICAL_DATABASE_ERROR, $"Лес. Запуск бота. В БД уже существует запись о том, что бот запущен. botId={botId}");
                return false;
            }


            string domain = HttpContext.Request.Host.Value;
            var link = $"http://{domain}";

            RouteRecord rr = new RouteRecord()
            {
                BotId = botId,
                ForestLink = link
            };

            _context.RouteRecords.Add(rr);
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

                        _logger.SaveNow();
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


    class Node
    {
        public string NodeName { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public Params NodeParams { get; set; }

		public SimpleNode ToSimpleNode(bool needBack = true)
		{
			string file = NodeParams?.File;
			return new SimpleNode(NodeName, new MetaReplyMessage(NodeParams?.Message ?? NodeName,
				string.IsNullOrWhiteSpace(file) ? MessageType.Text : MessageType.Document,
				file), needBack);
		}

		private static readonly TryConvert<string> notEmptyString =
			new TryConvert<string>((string text, out string variable) =>
			!string.IsNullOrWhiteSpace(variable = text));

		public static implicit operator LogicalCore.Node (Node node)
		{
			Params parameters = node.NodeParams;
			if (parameters == null)
			{
				return new SimpleNode(node.NodeName, new MetaReplyMessage(node.NodeName, MessageType.Text));
			}
			else
			{
				if(parameters is CollectionNodeParams collParams)
				{
					switch (collParams.ScrollingMethod)
					{
						case ScrollingMethod.SendingAll:
							return new BlockNode(node.NodeName, new MetaReplyMessage(collParams.Message,
								string.IsNullOrWhiteSpace(collParams.File) ? MessageType.Text : MessageType.Document,
								collParams.File));
						case ScrollingMethod.SendingOnlySelected:
							return new ChildrenFlipperNode(node.NodeName, new MetaDoubleKeyboardedMessage(metaReplyText: collParams.Message,
								metaInlineText: node.NodeName, messageType: string.IsNullOrWhiteSpace(collParams.File) ? MessageType.Text : MessageType.Document,
								messageFile: collParams.File));
						default:
							throw new NotImplementedException("Unknown scrolling method.");
					}
				}
				else
				{
					if(parameters is SendOrderParams sendParams)
					{
						return new OwnerNotificationNode(node.NodeName, new MetaInlineMessage(sendParams.Message,
								string.IsNullOrWhiteSpace(sendParams.File) ? MessageType.Text : MessageType.Document,
								sendParams.File), sendParams.Sendable, 1,
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
					}
					else
					{
						if(parameters is InputNodeParams inputParams)
						{
							//inputParams.ExpectedResponseFormat? I tak soidёt!
							return new UsualInputNode(node.NodeName, "InputVar", notEmptyString, new MetaReplyMessage(inputParams.Message,
								string.IsNullOrWhiteSpace(inputParams.File) ? MessageType.Text : MessageType.Document,
								inputParams.File));
						}
						else
						{
							if(parameters is ProductNodeParams productParams)
							{
								int productId = unchecked((int)productParams.ProductId);
								//long to int? Всё равно потом заменю

								//return new ProductSimpleNode<decimal>(node.NodeName, productParams.Characteristics.
								//	Select(_chars => _chars.Values).ToList(), "Products", productParams.Characteristics.
								//	SelectMany(_chars => _chars.IDs).ToList(), productParams.Characteristics.
								//	Select(_chars => _chars.CharacteristicName).ToList(), "ShoppingCart");

								return new ProductMultiNode<decimal>(node.NodeName, productParams.Characteristics.
									Select(_chars => _chars.Values).ToList(), "Products", productParams.Characteristics.
									SelectMany(_chars => _chars.IDs).ToList(), "ShoppingCart", "Добавлено: ", "Добавить",
									new MetaDoubleKeyboardedMessage(productParams.FullDescription, productParams.Message));
							}
							else
							{
								throw new NotImplementedException("Unknown type.");
							}
						}
					}
				}
			}
		}
    }

    internal class Params
    {
        public string File { get; set; }
        public string Message { get; set; }
    }
    class CollectionNodeParams : Params
    {
        public ScrollingMethod ScrollingMethod { get; set; }
    }
    class SendOrderParams : Params
    {
		public IOrdersSendable Sendable { get; set; }
    }
    class InputNodeParams : Params
    {
        public bool AnswerIsRequired { get; set; }
        public Dictionary<string, bool> ExpectedResponseFormat { get; set; } = new Dictionary<string, bool>();
    }
    class ProductNodeParams : Params
    {
        public long ProductId { get; set; }
        public string FullDescription { get; set; }
        public List<Characteristic> Characteristics { get; set; } = new List<Characteristic>();
    }
    class Characteristic
    {
        public string CharacteristicName { get; set; }
        public List<string> Values { get; set; } = new List<string>();
		public List<int> IDs { get; set; } = new List<int>();
	}

    enum ScrollingMethod
    {
        SendingAll,
        SendingOnlySelected
    }

	enum NodeType
	{
		Unknown = 0,
		Root = 1,
		NewOrder = 2,
		Section = 3,
		Product = 4,
		ConfirmOrder = 5,
		Input = 6,
		SendOrder = 7
	}

}


