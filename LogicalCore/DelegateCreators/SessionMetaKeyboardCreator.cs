using System;
using System.Collections.Generic;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace LogicalCore
{
	public static class SessionMetaKeyboardCreator
	{
		public static Func<Session, MetaInlineKeyboardMarkup> OwnerAnswerToUser(List<int> buttonsInRows = null, string deleteButtonName = null, params (string button, MetaText answer)[] tuples)
		{
			bool firstTime = true;

			if (buttonsInRows == null) buttonsInRows = new List<int>();
			var baseKeyboard = new List<List<(string name, string callback)>>(buttonsInRows.Count)
			{ new List<(string name, string callback)>() };

			bool needDelButton = !string.IsNullOrWhiteSpace(deleteButtonName);
			int row = 0, column = 0;
			foreach ((string button, MetaText answer) in tuples)
			{
				if (needDelButton && deleteButtonName == button) needDelButton = false;

				if (row < buttonsInRows.Count)
				{
					if(column < buttonsInRows[row])
					{
						baseKeyboard[row].Add((button, $"{DefaultStrings.Owner}_{button}_"));
						column++;
					}
					else
					{
						baseKeyboard.Add(new List<(string name, string callback)>()
						{ (button, $"{DefaultStrings.Owner}_{button}_") });
						column = 1;
						row++;
					}
				}
				else
				{
					baseKeyboard.Add(new List<(string name, string callback)>(1)
						{ (button, $"{DefaultStrings.Owner}_{button}_") });
				}
			}

			if(needDelButton)
			{
				int lastRow = baseKeyboard.Count - 1;
				if(lastRow < buttonsInRows.Count && baseKeyboard[lastRow].Count < buttonsInRows[lastRow])
				{
					baseKeyboard[lastRow].Add((deleteButtonName, $"{DefaultStrings.Owner}_{deleteButtonName}_"));
				}
				else
				{
					baseKeyboard.Add(new List<(string name, string callback)>(1)
					{ (deleteButtonName, $"{DefaultStrings.Owner}_{deleteButtonName}_") });
				}
			}

			return (Session session) =>
			{
				if (firstTime)
				{
					foreach ((string button, MetaText answer) in tuples)
					{
						session.BotOwner.actions.TryAdd(button,
							(userSession) =>
							{
								BotOwner botOwner = userSession.BotOwner;
								User sender = userSession.User;
								CallbackQuery callback = botOwner.Session.LastCallback;

								string text = callback.Message.Text; // берём старый текст
									int firstLineIndex = text.IndexOf('\n'); // первая строка - ник пользователя
																			 // обновляем ссылку (она удаляется)
								if (sender != null)
								{
									text = $"[{sender.FirstName} {sender.LastName}](tg://user?id={sender.Id})\n{text.Substring(firstLineIndex + 1)}";
								}
								else
								{
									text = $"[{text.Substring(0, firstLineIndex)}](tg://user?id={userSession.telegramId})\n{text.Substring(firstLineIndex + 1)}";
								}

								text = text.Substring(0, text.LastIndexOf('\n') + 1); // удаляем последнюю строку статуса
									text += botOwner.Session.Translate(button); // добавляем новую строку статуса

									userSession.BotClient.EditMessageTextAsync(
												callback.Message.Chat.Id,
												callback.Message.MessageId,
												text,
												ParseMode.Markdown,
												disableWebPagePreview: true,
												replyMarkup: GetMarkup(userSession.telegramId));

								string answerMsg = answer.ToString(userSession);

								if (!string.IsNullOrWhiteSpace(answerMsg))
								{
									userSession.BotClient.SendTextMessageAsync(userSession.telegramId, answerMsg);
								}
							});
					}

					if (deleteButtonName != null)
					{
						void deleteMsg(Session userSession) =>
						 userSession.BotClient.DeleteMessageAsync(
							 userSession.BotOwner.Session.LastCallback.Message.Chat.Id,
							 userSession.BotOwner.Session.LastCallback.Message.MessageId);

						if(session.BotOwner.actions.TryGetValue(deleteButtonName, out Action<Session> currentAction))
						{
							session.BotOwner.actions[deleteButtonName] = SessionActionsCreator.JoinActions(currentAction, deleteMsg);
						}
						else
						{
							session.BotOwner.actions[deleteButtonName] = deleteMsg;
						}
					}

					firstTime = false;
				}

				var keyboard = new List<List<(InlineKeyboardButton button, List<Predicate<Session>> rules)>>(baseKeyboard.Count);

				int index = 0;
				foreach (var btnRow in GetMarkup(session.telegramId).InlineKeyboard)
				{
					keyboard.Add(new List<(InlineKeyboardButton button, List<Predicate<Session>> rules)>());
					foreach(var btn in btnRow)
					{
						keyboard[index].Add((btn, null));
					}
					index++;
				}

				return new MetaInlineKeyboardMarkup(keyboard);

				InlineKeyboardMarkup GetMarkup(int senderID)
				{
					InlineKeyboardButton[][] buttons = new InlineKeyboardButton[baseKeyboard.Count][];

					for(int i = 0; i < baseKeyboard.Count; i++)
					{
						var currentList = baseKeyboard[i];
						buttons[i] = new InlineKeyboardButton[currentList.Count];

						for (int j = 0; j < currentList.Count; j++)
						{
							(string name, string callback) = currentList[j];
							buttons[i][j] = InlineKeyboardButton.WithCallbackData(
								session.BotOwner.Session.Translate(name), callback + senderID);
						}
					}

					InlineKeyboardMarkup keyboardMarkup = new InlineKeyboardMarkup(buttons);

					return keyboardMarkup;
				}
			};
		}
	}
}
