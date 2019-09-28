//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using LogicalCore;
//using Telegram.Bot.Types.Enums;
//using Telegram.Bot.Types.InputFiles;
//
//namespace Forest
//{
//	public static class Stub
//    {
//		private static readonly Random random = new Random();
//		private static Dictionary<int, MetaValued<int>> Sushi { get; }
//		private static Dictionary<int, MetaValued<int>> Pizza { get; }
//		private static readonly string[] pizzaDiameters = new string[3] { "25 cm", "30 cm", "40 cm" };
//		private static readonly string[] pizzaBorts = new string[3] { "UsualBort", "HotDogBort", "PhiladelphiaBort" };
//		
//		static Stub()
//		{
//			Sushi = new Dictionary<int, MetaValued<int>>
//			{
//				{ 0, new MetaValued<int>(new MetaText("Sushi", " (id0)"), random.Next(2, 5), "Dollar") },
//				{ 1, new MetaValued<int>(new MetaText("Sushi", " (id1)"), random.Next(2, 5), "Dollar") },
//				{ 2, new MetaValued<int>(new MetaText("Sushi", " (id2)"), random.Next(2, 5), "Dollar") },
//				{ 3, new MetaValued<int>(new MetaText("Sushi", " (id3)"), random.Next(2, 5), "Dollar") },
//				{ 4, new MetaValued<int>(new MetaText("Sushi", " (id4)"), random.Next(2, 5), "Dollar") },
//				{ 5, new MetaValued<int>(new MetaText("Sushi", " (id5)"), random.Next(2, 5), "Dollar") },
//				{ 6, new MetaValued<int>(new MetaText("Sushi", " (id6)"), random.Next(2, 5), "Dollar") }
//			};
//
//			int pizzaTypesCount = 8, pizzaDiametersCount = pizzaDiameters.Length, pizzaBortsCount = pizzaBorts.Length;
//			int pizzaSubtypesCount = pizzaDiametersCount * pizzaBortsCount;
//			Pizza = new Dictionary<int, MetaValued<int>>(pizzaTypesCount * pizzaSubtypesCount);
//			
//			for(int i = 0; i < pizzaTypesCount; i++)
//			{
//				for(int j = 0; j < pizzaDiametersCount; j++)
//				{
//					for(int k = 0; k < pizzaBortsCount; k++)
//					{
//						int number = k + j * pizzaBortsCount + i * pizzaSubtypesCount;
//						Pizza.Add(number, new MetaValued<int>(new MetaText("Pizza", " ", pizzaDiameters[j], " ", pizzaBorts[k], " (id", number.ToString(), ")"), random.Next(2, 10), "Dollar"));
//					}
//				}
//			}
//		}
//
//		private static MegaTree GetHardcodedMegaTree(BotWrapper botWrapper)
//        {
//            #region Хардкод мегадерева
//            #region  Чтение разметки
//            //while (true)
//            //{
//            //читаем кол-во узлов
//            //создаем массив такой длинны
//            //в цикле создаем узлы согласно разметке
//            //создать мегадерево
//            //установить связи в цикле
//            //Если есть требование типа "узел = корень, разметка="2,1" "
//            //Задаю настройку разметки, если указано 
//            //}
//            #endregion
//
//            #region Задаю узлы
//            Node nodeRoot = new SimpleNode("Root", needBack: false);
//            MegaTree megaTree = new MegaTree(nodeRoot);
//
//			//var fileInput = new FileInputNode("Settings", "File",
//			//	(string msg, out (string FileId, string PreviewId, string Description) variable) =>
//			//	{
//			//		variable = (null, null, msg);
//			//		return true;
//			//	});
//			//megaTree.AddEdge(nodeRoot, fileInput);
//
//			//var goBack = new ActionNode(null);
//			//megaTree.AddEdge(fileInput, goBack);
//
//			//megaTree.AddEdge(goBack, nodeRoot);
//
//			LanguageNode languageNode = new LanguageNode(botWrapper, "SelectLanguage", "LanguageChanged");
//			
//            megaTree.AddEdge(nodeRoot, languageNode);
//
//            megaTree.AddEdge(languageNode, nodeRoot);
//
//            SelectManyInputNode<MetaText> quizNode = new SelectManyInputNode<MetaText>("Quiz", "QuizAnswers",
//                new List<MetaText>()
//                {
//                    new MetaText("Like", " ", "Pizza"),
//                    new MetaText("Like", " ", "Sushi"),
//                    new MetaText("Like", " ", "Dessert"),
//                    new MetaText("Like", " ", "Drinks"),
//                    new MetaText("Like", " ", "Description"),
//                    new MetaText("Like", " ", "UsualBort"),
//                    new MetaText("Like", " ", "HotDogBort"),
//                    new MetaText("Like", " ", "PhiladelphiaBort"),
//                },
//                "QuizDescription",
//                pageSize: 4);
//
//            ActionNode showQuizResults = new ActionNode((session) =>
//            session.BotClient.SendTextMessageAsync(session.telegramId,
//            string.Join(";\n", session.vars.GetVar<List<MetaText>>("QuizAnswers").
//            Select((metaText) => metaText.ToString(session)))),
//            "QuizEnd");
//
//            megaTree.AddEdge(nodeRoot, quizNode);
//
//            megaTree.AddEdge(quizNode, showQuizResults);
//
//            megaTree.AddEdge(showQuizResults, nodeRoot);
//
//            Node nodeOrder = new SimpleNode("NewOrder", "CanSelectSection");
//
//            megaTree.AddEdge(nodeRoot, nodeOrder,
//                (session) =>
//                session.vars.TryGetVar<MetaValuedContainer<int>>("ShoppingCart", out var products) && products.Count == 0);
//
//
//
//            Node nodePizza = new ChildrenFlipperNode("Pizza");
//            Node nodeSushi = new BlockNode("Sushi");
//            Node nodeDessert = new BlockNode("Dessert");
//            Node nodeDrinks = new BlockNode("Drinks");
//
//            TryConvert<string> notEmptyString = new TryConvert<string>((string text, out string variable) => !string.IsNullOrWhiteSpace(variable = text));
//
//            TextInputNode nodeAddressRequest = new TextInputNode("ConfirmOrder", "Address", notEmptyString, "AddressRequest");
//
//            TryConvert<TimeSpan> checkTime = new TryConvert<TimeSpan>((string text, out TimeSpan variable) =>
//            {
//                bool parsed = DateTime.TryParseExact(text, "H:mm",
//                    System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.NoCurrentDateDefault,
//                    out DateTime dt);
//                variable = dt - dt.Date;
//                return parsed;
//            });
//
//            UsualInputNode<TimeSpan> nodeTimeRequest = new UsualInputNode<TimeSpan>("SpecifyTime", "Time", checkTime, "TimeRequest");
//
//            TextInputNode nodeCommentRequest = new TextInputNode("AddAComment", "Comment", notEmptyString, "AddACommentDescription", false);
//
//            bool notEmptyCart(Session session) => session.vars.TryGetVar<MetaValuedContainer<int>>("ShoppingCart", out var products) && products.Count > 0;
//
//			OwnerNotificationNode notifyOwner = new OwnerNotificationNode("SendNotification", "DeliveryWasSent",
//				keyboardCreator: SessionMetaKeyboardCreator.OwnerAnswerToUser(
//					new List<int>(2) { 3, 2 }, "Delete",
//					("Processing", "ProcessingNotification"),
//					("Transiting", "TransitingNotification"),
//					("Accepted", "AcceptedNotification"),
//					("Denied", "DeniedNotification")),
//				variables: new (Type, string)[]
//				{
//					(typeof(MetaValuedContainer<int>), "ShoppingCart"),
//					(typeof(string), "Address"),
//					(typeof(TimeSpan), "Time"),
//					(typeof(string), "Comment")
//				});
//
//			megaTree.AddEdge(nodeRoot, nodeAddressRequest, notEmptyCart);
//            megaTree.AddEdge(nodeAddressRequest, nodeTimeRequest);
//            megaTree.AddEdge(nodeTimeRequest, nodeCommentRequest);
//			megaTree.AddEdge(nodeCommentRequest, notifyOwner);
//			megaTree.AddEdge(notifyOwner, nodeRoot);
//
//
//			megaTree.AddEdge(nodeOrder, nodePizza);
//            megaTree.AddEdge(nodeOrder, nodeSushi);
//            megaTree.AddEdge(nodeOrder, nodeDessert);
//            megaTree.AddEdge(nodeOrder, nodeDrinks);
//
//
//            MetaValuedContainerInputNode<int> pizzaConstructor = new MetaValuedContainerInputNode<int>("PizzaConstructor", "ConstructedPizza",
//                new List<MetaValued<int>>(6)
//                {
//                    new MetaValued<int>("Meat", 3, "Dollar"),
//                    new MetaValued<int>("Tomato", 1, "Dollar"),
//                    new MetaValued<int>("Mushrooms", 2, "Dollar"),
//                    new MetaValued<int>("Corn", 1, "Dollar"),
//                    new MetaValued<int>("Pepper", 1, "Dollar"),
//                    new MetaValued<int>("Broccoli", 2, "Dollar")
//                },
//                "MakeYourOwnPizza",
//                (session, container) =>
//                {
//                    MetaText pizzaDesc = new MetaText("ConstructedPizza", ": ");
//                    int price = 0;
//                    foreach (var ingredientPair in container)
//                    {
//                        MetaValued<int> ingredient = ingredientPair.Key;
//                        int ingredientCount = ingredientPair.Value;
//                        pizzaDesc.Append(ingredient.Text);
//                        pizzaDesc.Append(" (");
//                        pizzaDesc.Append(ingredientCount);
//                        pizzaDesc.Append("); ");
//                        price += ingredient.Value * ingredientCount;
//                    }
//                    pizzaDesc.Append("=");
//                    MetaValued<int> constructedPizza = new MetaValued<int>(pizzaDesc, price, "Dollar", false);
//                    session.vars.GetVar<MetaValuedContainer<int>>("ShoppingCart").Add(constructedPizza, 1);
//                    container.Clear();
//                    session.BotClient.SendTextMessageAsync(session.telegramId, session.vars.GetVar<MetaValuedContainer<int>>("ShoppingCart").ToString(session));
//                });
//
//            ActionNode endPizzaConstruction = new ActionNode(null, "PizzaConstructed");
//
//            megaTree.AddEdge(nodePizza, pizzaConstructor);
//
//            megaTree.AddEdge(pizzaConstructor, endPizzaConstruction);
//
//            megaTree.AddEdge(endPizzaConstruction, nodePizza);
//
//            MetaText inline = new MetaText("*", "Pizza", "*");
//            for (int i = 0; i < 8; i++)
//            {
//                MetaDoubleKeyboardedMessage metaDouble =
//                    new MetaDoubleKeyboardedMessage(
//                        metaReplyText: "ChoosePizzaSize",
//                        metaInlineText: inline,
//                        messageType: MessageType.Photo,
//                        messageFile: new InputOnlineFile(
//                            new FileStream(
//                                GetFullFilePathByFileName($"/pizza{i + 1}.jpg"),
//                                FileMode.Open,
//                                FileAccess.Read,
//                                FileShare.Read)),
//                        parsing: ParseMode.Markdown,
//                        replyMsgFirst: false);
//
//				SimpleNode pizzaType = new SimpleNode("Pizza", metaDouble);
//
//                megaTree.AddEdge(nodePizza, pizzaType);
//
//                LightNode desc = new LightNode("Description", new MetaInlineMessage(new MetaText("*", "Pizza", i, "*\n", "TestDescPizza"), parsing: ParseMode.Markdown));
//                metaDouble.DownButtonsLocation = false;
//                megaTree.AddEdge(pizzaType, desc);
//                metaDouble.DownButtonsLocation = true;
//
//				for(int j = 0; j < pizzaDiameters.Length; j++)
//				{
//					SimpleNode sizeNode = new SimpleNode(pizzaDiameters[j], "ChooseBortType");
//					megaTree.AddEdge(pizzaType, sizeNode);
//
//					for (int k = 0; k < pizzaBorts.Length; k++)
//					{
//						SimpleNode bortNode = new SimpleNode(pizzaBorts[k]);
//						megaTree.AddEdge(sizeNode, bortNode);
//
//						int number = k + j * pizzaBorts.Length + i * pizzaBorts.Length * pizzaDiameters.Length;
//						MetaText addedText = Pizza[number].ToMetaText();
//						addedText.Append("\n", "Added");
//						ItemNode<int> pizzaItemNode = new ItemNode<int>("Pizza", number, "ShoppingCart",
//							new MetaInlineMessage(addedText));
//						megaTree.AddEdge(bortNode, pizzaItemNode);
//						megaTree.AddEdge(pizzaItemNode, nodePizza);
//					}
//
//					sizeNode.SetButtonsLocation(ElementsLocation.Zd);
//				}
//
//				pizzaType.SetButtonsLocation(ElementsLocation.Zd);
//			}
//
//            
//            for (int i = 0; i < 7; i++)
//            {
//				MetaText text = Sushi[i].ToMetaText();
//				text.Append("\n", "TestDescSushi");
//                MetaInlineMessage msg = new MetaInlineMessage(
//					text,
//                    MessageType.Photo,
//                    new InputOnlineFile(
//                            new FileStream(
//                                GetFullFilePathByFileName($"/sushi{i + 1}.jpg"),
//                                FileMode.Open,
//                                FileAccess.Read,
//                                FileShare.Read)));
//
//                var thisSushi = new LightNode("Sushi", msg);
//                
//                megaTree.AddEdge(nodeSushi, thisSushi);
//
//				MetaText addedText = Sushi[i].ToMetaText();
//				addedText.Append("\n", "Added");
//				var addThisSushi = new ItemNode<int>("Sushi", i, "ShoppingCart", new MetaInlineMessage(addedText));
//				megaTree.AddEdge(thisSushi, addThisSushi);
//				megaTree.AddEdge(addThisSushi, nodeSushi);
//			}
//
//            #endregion
//
//            #region Уточняю разметку кнопок, если нужно
//            nodeRoot.SetButtonsLocation(ElementsLocation.Zd);
//
//            nodeOrder.SetButtonsLocation(ElementsLocation.Zd);
//
//            #endregion
//            #endregion
//            return megaTree;
//        }
//
//		public static BotWrapper CreateBot(string token, string link = null)
//		{
//			BotWrapper botWrapper = new BotWrapper(0, link, token,
//				new TextMessagesManager(GetTranslatorDictionary(), "Русский"), new GlobalFilter(),
//				new VariablesContainer());
//			botWrapper.MegaTree = GetHardcodedMegaTree(botWrapper);
//			botWrapper.InitializeSessionVars = (VariablesContainer vars) =>
//			{
//				vars.SetVar(new MetaValuedContainer<int>("ShoppingCart", finalFunc: (dict) =>
//				{
//					if (dict.Count == 0) return new MetaText("Cost", "0");
//
//					MetaValued<int> result = new MetaValued<int>();
//
//					bool first = true;
//
//					foreach (var pair in dict)
//					{
//						if (first)
//						{
//							result = new MetaValued<int>("Cost", 0, pair.Key.Unit, pair.Key.UseSpaceForUnit, pair.Key.UnitBeforeValue);
//							first = false;
//						}
//
//						result.Value += pair.Key.Value * pair.Value;
//					}
//
//					return result;
//				}));
//			};
//			botWrapper.globalVars.SetVar("Sushi", Sushi);
//			botWrapper.globalVars.SetVar("Pizza", Pizza);
//			botWrapper.SetOwner(389063743); //389063743 440090552
//			return botWrapper;
//		}
//
//      
//
//        public static Dictionary<string, Dictionary<string, string>> GetTranslatorDictionary()
//        {
//            Dictionary<string, Dictionary<string, string>>  msgDict = new Dictionary<string, Dictionary<string, string>>
//            {
//                { "Emoji", new Dictionary<string, string>
//                    {
//                        { "PressButton", "🔴⬇️" },
//                        { "Language", "🌍" },
//                        { "SelectLanguage", "🌍?" },
//                        { "LanguageChanged", "🔄" },
//                        { "Hello", "👋🤗! 📦➡️🏠 🍽🤤?" },
//                        { "OnlyText", "😯?" },
//                        { "More", "▶️ ▶️ ▶️" },
//                        { "Back", "◀️ ◀️ ◀️" },
//                        { "Next", "▶️ 📋 ▶️" },
//                        { "Previous", "◀️ 📋 ◀️" },
//                        { "ShowCart", "🛒👀" },
//						{ "Add", "➕" },
//						{ "Added", "➕✅" },
//						{ "AddToCart", "🛒➕" },
//                        { "Teleport", "🛒⬇️" },
//                        { "Description", "📖📋" },
//                        { "Root", "(🌳)" },
//                        { "NewOrder", "🆕🗃" },
//                        { "CanSelectSection", "🤗:🍕,🍣,🍮,🍸?" },
//                        { "Settings", "⚙️ 🛠 ⚙️" },
//                        { "Address", "🏠: " },
//                        { "Pizza", "🍕 🍕 🍕" },
//                        { "Sushi", "🍣 🍣 🍣" },
//                        { "Dessert", "🍮 🍮 🍮" },
//                        { "Drinks", "🍸 🍸 🍸" },
//                        { "MyAddress", "👇 🏠" },
//                        { "ConfirmOrder", "✅ 🗃" },
//                        { "WhenAndWhereDeliver", "🤗: ⌚, 📦➡️🏠?" },
//                        { "SavedAddress", "📦➡️(💾🏠)" },
//                        { "NewAddress", "📦➡️(🆕🏠)" },
//                        { "EnterNewAddress", "⚙️ 🛠 🆕🏠" },
//                        { "TimeClarification", "⚙️ 🛠 ⌚" },
//                        { "FastDelivery", "🏃💨 ⌚?" },
//                        { "Yes", "👍 👍 👍" },
//                        { "No", "👎 👎 👎" },
//                        { "WhenDeliver", "⌚?" },
//                        { "SendDelivery", "📦 ➡️ 🏠?" },
//                        { "DeliveryWasSent", "🚚📦 ➡️ 🏠!" },
//                        { "Sad", "🙁😢" },
//                        { "TestDescPizza", "Ⓥ🍕(🚫🥩): 🍅, 🍄, 🧅, 🌽, 🌶️, 🥦, 🍆, 🌿." },
//                        { "TestDescSushi", "🍣: 🐟, 🧀, 🥑, 🍚, 🥒." },
//                        { "ChoosePizzaSize", "🍕: 📏?" },
//                        { "ChooseBortType", "🍕⚪?" },
//                        { "UsualBort", "🍕⚪" },
//                        { "HotDogBort", "🍕🌭" },
//                        { "PhiladelphiaBort", "🍕(39.952583, -75.165222)" },
//                        { "25 cm", "🍕 25 cm 📏" },
//                        { "30 cm", "🍕 30 cm 📏" },
//                        { "40 cm", "🍕 40 cm 📏" },
//                        {"ThisCommandIsUnknown","🧐❓" },
//                        {"Error", "❌😢" },
//                        {"CartContent", "🛒:" },
//                        {"Empty", "❌" },
//                        {"SendNotification", "📋➡️🤖" },
//                        {"OwnerNotified", "✅ 📋➡️🤖" },
//                        { "HelloForOwner", "🤖🎛️: ➡️📋." },
//                        {"TooLongQueue", "❌😢: 👥👥👥 ➡️ ⌚🤖" },
//                        {"Status", "📋🖥: " },
//                        {"NotConsidered", "👀👀👀" },
//                        {"Processing", "⌛" },
//                        {"ProcessingNotification", "📋 ➡️ ⌛" },
//                        {"Transiting", "🚚" },
//                        {"TransitingNotification", "🚚📦" },
//                        {"Accepted", "✅" },
//                        {"AcceptedNotification", "📋✅" },
//                        {"Denied", "❌" },
//                        {"DeniedNotification", "📋❌" },
//                        {"Delete", "🗑" },
//                        { "AddressRequest", "👇 🏠 ❓"},
//                        { "AddAComment", "➕ 📚"},
//                        { "Comment", "📚: "},
//                        { "AddACommentDescription", "✍  📚"},
//                        {"Time", "🕐" },
//                        {"SpecifyTime", "✍️🕐" },
//                        {"TimeRequest", "🙏 👉 🚚 🕰 👯 👯 📁.\n 📊: 1⃣5⃣:3⃣0⃣" },
//                        {"Dollar", "💵" },
//                        {"Cost", "💰: " },
//                        {"Plus", "➕" },
//                        {"Minus", "➖" },
//                        {"Quiz", "❓❓❓" },
//                        {"QuizDescription", "❓❓❓: ☑️☑️☑️" },
//                        {"QuizEnd", "❓🥳🎉☑️" },
//                        {"Like", "😋👍" },
//                        {"PizzaConstructor", "⚙️ 🍕 ⚙️" },
//                        {"MakeYourOwnPizza", "👷 🏗 ⚙️ 🍕 🛠" },
//                        {"PizzaConstructed", "🛠 🍕 🛠" },
//                        {"ConstructedPizza", "🍕 ⚙️ 🍕" },
//                        {"Meat", "🥩" },
//                        {"Tomato", "🍅" },
//                        {"Mushrooms", "🍄" },
//                        {"Corn", "🌽" },
//                        {"Pepper", "🌶️" },
//                        {"Broccoli", "🥦" }
//                    }
//                },
//                { "Русский", new Dictionary<string, string>
//                    {
//                        { "PressButton", "Нажмите на кнопку." },
//                        { "Language", "Выбрать язык" },
//                        { "SelectLanguage", "Выберите язык." },
//                        { "LanguageChanged", "Язык был изменён." },
//                        { "Hello", "Привет🤗. Хочешь заказать еду?🍽🤤" },
//                        { "OnlyText", "😯 Я понимаю только текст!" },
//                        { "More", "▶️Ещё▶️" },
//                        { "Back", "◀️Назад◀️" },
//                        { "Next", "▶️Дальше▶️" },
//                        { "Previous", "◀️Предыдущие◀️" },
//                        { "ShowCart", "🛒👀Показать корзину🛒👀" },
//						{ "Add", "Добавить➕" },
//						{ "Added", "➕Добавлено✅" },
//						{ "AddToCart", "Добавить в корзину🛒➕" },
//                        { "Teleport", "В корзину" },
//                        { "Description", "Описание" },
//                        { "Root", "Корень" },
//                        { "NewOrder", "Новый заказ🆕🗃" },
//                        { "CanSelectSection", "Тут можно выбрать раздел🤗" },
//                        { "Settings", "⚙️Настройки⚙️" },
//                        { "Address", "Адрес: " },
//                        { "Pizza", "🍕Пицца🍕" },
//                        { "Sushi", "🍣Суши🍣" },
//                        { "Dessert", "🍮Десерты🍮" },
//                        { "Drinks", "🍸Напитки🍸" },
//                        { "MyAddress", "Мой адрес👇 🏠" },
//                        { "ConfirmOrder", "Подтвердить заказ ✅ 🗃" },
//                        { "WhenAndWhereDeliver", "Тут нужно указать куда и когда нужно доставить заказ 🤗" },
//                        { "SavedAddress", "Использовать сохраненный адрес" },
//                        { "NewAddress", "Ввести новый адрес" },
//                        { "EnterNewAddress", "🆕Введите новый адрес🏠" },
//                        { "TimeClarification", "Уточнение времени" },
//                        { "FastDelivery", "Делать доставку в ближайшее время?" },
//                        { "Yes", "Да." },
//                        { "No", "Нет." },
//                        { "WhenDeliver", "Когда Вам нужно доставить товар?" },
//                        { "SendDelivery", "Отправлять заказ?" },
//                        { "DeliveryWasSent", "Заказ отправлен." },
//                        { "Sad", "🙁Жаль...😢" },
//                        { "TestDescPizza", "Пицца для веганов, соус на томатной основе, помидоры, шампиньоны, лук маринованный, кукуруза, перец болгарский, брокколи, кабачок, специи." },
//                        { "TestDescSushi", "Классический ролл с лососем, сливочным сыром, авокадо, икрой тобико и огурцом." },
//                        { "ChoosePizzaSize", "Тут можно выбрать размер пиццы.📏" },
//                        { "ChooseBortType", "Тут можно выбрать тип борта.🦕" },
//                        { "UsualBort", "Обычный борт" },
//                        { "HotDogBort", "Хот-дог борт" },
//                        { "PhiladelphiaBort", "Филадельфия" },
//                        { "25 cm", "25 см" },
//                        { "30 cm", "30 см" },
//                        { "40 cm", "40 см" },
//                        {"ThisCommandIsUnknown", "Мне ничуть не известна такая команда 🧐🤔 " },
//                        {"Error", "Похоже, произошла какая-то ошибка. ❌😢" },
//                        {"CartContent", "Содержимое корзины:" },
//                        {"Empty", "(пусто)" },
//                        {"SendNotification", "Отправить заказ 📋➡️🤖" },
//                        {"OwnerNotified","✅ Отлично! Ваш заказ был отправлен владельцу бота на обработку. 📋➡️🤖" },
//                        { "HelloForOwner", "Бот запущен. Вы были распознаны как владелец бота. 🤖🎛️\nУведомления о заказах от пользователей будут приходить в этот чат. ➡️📋" },
//                        {"TooLongQueue", "👥👥👥 Слишком длинная очередь, заказ был отменён. ❌😢\nПопробуйте позже.⌚" },
//                        {"Status", "*Состояние:* " },
//                        {"NotConsidered", "Требуется рассмотреть 👀" },
//                        {"Processing", "В обработке ⌛" },
//                        {"ProcessingNotification", "Ваш заказ обрабатывается. Подождите. ⌛" },
//                        {"Transiting", "В пути 🚚" },
//                        {"TransitingNotification", "Ваш заказ находится в пути. 🚚" },
//                        {"Accepted", "Принят ✅" },
//                        {"AcceptedNotification", "Ваш заказ был принят. ✅" },
//                        {"Denied", "Отменён ❌" },
//                        {"DeniedNotification", "Ваш заказ был отменён. ❌" },
//                        {"Delete", "Удалить 🗑" },
//                        { "AddressRequest", "Будьте добры указать ваш адрес 👇 🏠 ❓.\nНапример: 221B Baker Street"},
//                        { "AddAComment", "Добавить комментарий"},
//                        { "Comment", "\nКомментарий: "},
//                        { "AddACommentDescription", "Будьте добры указать комментарий ➕ 📚.\nНапример: Доставьте пиццу холодной, пожалуйста."},
//                        {"Time", "\nВремя: " },
//                        {"SpecifyTime", "Указать время ✍️🕐" },
//                        {"TimeRequest", "Будьте добры указать время доставки 🙏 🕐 ❓.\nФормат ХХ:ХХ Например: 15:30, 9:50, 1:45" },
//                        {"Dollar", "$" },
//                        {"Cost", "Стоимость: " },
//                        {"Plus", "➕" },
//                        {"Minus", "➖" },
//                        {"Quiz", "Опрос" },
//                        {"QuizDescription", "❓Выберите правильные ответы.☑️" },
//                        {"QuizEnd", "❓Вы ответили на все вопросы!☑️" },
//                        {"Like", "Мне нравится" },
//                        {"PizzaConstructor", "⚙️Конструктор пиццы🍕" },
//                        {"MakeYourOwnPizza", "🏗Тут можно создать свою собственную пиццу!🍕" },
//                        {"PizzaConstructed", "🛠Пицца собрана!🍕" },
//                        {"ConstructedPizza", "⚙️🍕Собранная пицца🍕⚙️" },
//                        {"Meat", "Мясо" },
//                        {"Tomato", "Помидоры" },
//                        {"Mushrooms", "Грибы" },
//                        {"Corn", "Кукуруза" },
//                        {"Pepper", "Перец" },
//                        {"Broccoli", "Брокколи" }
//                    }
//                }
//            };
//
//            return msgDict;
//        }
//
//        public static string GetFullFilePathByFileName(string fileName)
//        {
//            #region Dich
//            string file = "дичь";
//
//            string currentDirectory = Environment.CurrentDirectory;
//            DirectoryInfo directoryInfo = new DirectoryInfo(currentDirectory);
//
//            string projectName = currentDirectory.Split("\\").Last().Split("/").Last();
//
//            //ConsoleWriter.WriteLine("directory name = "+directoryInfo.FullName, ConsoleColor.Green);
//            //ConsoleWriter.WriteLine($"projectName = {projectName}", ConsoleColor.Green);
//
//            try
//            {
//                while (directoryInfo.Name != projectName)
//                {
//                    directoryInfo = directoryInfo.Parent;
//                    //ConsoleWriter.WriteLine($"currentDirectory = {currentDirectory}");
//                }
//            }
//            catch (Exception exep)
//            {
//                ConsoleWriter.WriteLine(exep.Message);
//            }
//            //ConsoleWriter.WriteLine("dirr info == null " + directoryInfo == null, ConsoleColor.Green);
//            DirectoryInfo directoryFiles = null;
//
//
//            foreach (var directory in directoryInfo.GetDirectories())
//            {
//                //ConsoleWriter.WriteLine($"directory.Name = {directory.Name}");
//                if (directory.Name == "Files")
//                {
//                    directoryFiles = directory;
//                    break;
//                }
//            }
//
//            if (directoryFiles != null)
//            {
//                file = directoryFiles.FullName + fileName;
//            }
//            else
//            {
//                ConsoleWriter.WriteLine("default action");
//                throw new Exception("В директории TelegramBot должна быть директория Files с картинкой пингвина");
//            }
//
//            if (file != null && file != "дичь")
//            {
//                return file;
//            }
//
//            throw new Exception();
//            #endregion
//        }
//    }
//}