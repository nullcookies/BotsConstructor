//десериализация
//FeedbackBotMarkup test2 = JsonConvert.DeserializeObject<FeedbackBotMarkup>(jsonMarkup);

Для регистрации в мониторе нужно выполнить 
Stub.RegisterInMonitor();




			 //Отправка post-запроса
			    //WebRequest request = WebRequest.Create(url);
                //request.Method = "POST";
                //string data = "botId=654";                
                //byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(data);                
                //request.ContentType = "application/x-www-form-urlencoded";                
                //request.ContentLength = byteArray.Length;

                ////отправить запрос на регистрацию
                //using (Stream dataStream = request.GetRequestStream())
                //{
                //    dataStream.Write(byteArray, 0, byteArray.Length);
                //}

                //try
                //{
                //    WebResponse response = request.GetResponseAsync().Result;
                //    using (Stream stream = response.GetResponseStream())
                //    {
                //        using (StreamReader reader = new StreamReader(stream))
                //        {
                //            Console.WriteLine(reader.ReadToEnd());
                //        }
                //    }
                //    response.Close();
                //    return true;
                //}
                //catch (Exception e)
                //{
                //    Console.WriteLine("Ошибка..." + e.Message);
                //    return false;
                //}


правила для создания разметки
1) В левом верхнем углу всегда есть модуль Root. Корень удалять нельзя.
2) Нужно отображать линию к его потомкам. Она идёт вертикально вниз и горизонтальновправо.
3) Для создания обьекта нужно перетащить его из меню
4) При наведении на обьект 
	1. Загорается линия
	2. У него появляются кнопки (шестерёнка для настроек/ иконка корзины для удаления узла)
5) При удалении узла 
	1. Если раздел, то удаляется вместе со всеми потомками. (Модальное окно с предупреждением)
	1. Если если это узел с одним потомком (input/что-то ещё) то предложить пользователю
		1) "Вынуть" узел из цепи
		2) Удалить вместе со всеми потомками
6)Есть необходимые узлы 
	1. Для продаж - это отправить заказ и товары
	2. Для каталога - товар (текст/песня/файл/видео)
	3. Для фидбека - инпут + подтверждение окончания
7) У товаров не может быть наследников
8) Если у узла есть наследники, то их можно свернуть по нажатию на треугольник

дизайн карточек должен быть сделан на основе bootstrap и иконок
нужно добавить возможность отменять действия

наверное стоит сделать два режима
1) Можно перетаскивать карточки
2) Можно только редактировать содержимое





































