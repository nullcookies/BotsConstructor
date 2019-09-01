using System;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace LogicalCore
{
    /// <summary>
    /// Глобальный фильтр действий, которые могут быть вызваны с любого узла.
    /// </summary>
    public class GlobalFilter
    {
        private readonly Dictionary<string, Action<Session, Message>> messageActions;
        private readonly Dictionary<string, Action<Session, CallbackQuery>> callbackActions;

        public GlobalFilter()
        {
            messageActions = new Dictionary<string, Action<Session, Message>>()
            {
                {"/start", (session, message) =>
                    {
                        session.BotClient.SendTextMessageAsync(
                            message.Chat.Id,
                            session.Translate("Hello"));

						session.GoToNode(session.MegaTree.root);
                    }
                },
                {"/show", async (session, message) =>
                    {
                        string command = message.Text.Trim();
                        if(command.Length <= 6)
                        {
                            await session.BotClient.SendTextMessageAsync(message.Chat.Id, session.Translate("ThisCommandIsUnknown"));
                            return;
                        }
                        string containerName = command.Substring(6);
						if(session.vars.TryGetVar(containerName, out MetaValuedContainer<decimal> container))
						{
							await container.SendMessage(session);
						}
						else
						{
							await session.BotClient.SendTextMessageAsync(message.Chat.Id, session.Translate("ThisCommandIsUnknown"));
						}
					}
                },
                {"/shotam", (session, message) =>
                    {
                        session.BotClient.SendStickerAsync(
                            message.Chat.Id,
                            "CAADAgADBwADuD87Gip7Mrcgkf0TAg");
                    }
                }
            };

            callbackActions = new Dictionary<string, Action<Session, CallbackQuery>>()
            {
                //Переход к узлу
                {DefaultStrings.GOTO, (session, callbackQuerry) =>
                    {
                        int nodeId = ButtonIdManager.GetIDFromCallbackData(callbackQuerry.Data);
                        Node node = session.MegaTree.GetNodeById(nodeId);
                        session.GoToNode(node);
                    }
                },
                //Вызов при нажатии влево/вправо в глобальной листалке или на текущем узле
                {DefaultStrings.SHOWPAGE, async (session, callbackQuerry) =>
                    {
                        int nodeID = ButtonIdManager.GetIDFromCallbackData(callbackQuerry.Data);
						var node = session.MegaTree.GetNodeById(nodeID);
						if(node is IFlippable flipper && (flipper.GlobalCallbacks || session.CurrentNode == node))
                        {
                            int page = ButtonIdManager.GetPageFromCallbackData(callbackQuerry.Data);
                            await flipper.SendPage(session, callbackQuerry.Message, page);
                        }
                        else
                        {
							//await session.BotClient.EditMessageTextAsync(session.telegramId, callbackQuerry.Message.MessageId, session.Translate("Error"));
                        }
                    }
                },
                //Добавить в контейнере какому-то значению 1
                {DefaultStrings.PLUS, async (session, callbackQuerry) =>
                    {
                        string containerName = ButtonIdManager.GetNextSubstring(callbackQuerry.Data, DefaultStrings.PLUS.Length, out int nextUnder);
                        string varHash = ButtonIdManager.GetNextSubstring(callbackQuerry.Data, nextUnder);

                        if(session.vars.TryGetVar(containerName, out MetaValuedContainer<decimal> container)
                        && int.TryParse(varHash, out int varHashCode) && container.ContainsKey(varHashCode))
                        {
                            container[varHashCode]++;
                            await container.EditMessage(session, session.telegramId, callbackQuerry.Message.MessageId);
                        }
                        else
                        {
                            await session.BotClient.EditMessageTextAsync(session.telegramId, callbackQuerry.Message.MessageId, session.Translate("Error"));
                        }
                    }
                },
                //Отнять в контейнере от какого-то значения 1
                {DefaultStrings.MINUS, async (session, callbackQuerry) =>
                    {
                        string containerName = ButtonIdManager.GetNextSubstring(callbackQuerry.Data, DefaultStrings.MINUS.Length, out int nextUnder);
                        string varHash = ButtonIdManager.GetNextSubstring(callbackQuerry.Data, nextUnder);

                        if(session.vars.TryGetVar(containerName, out MetaValuedContainer<decimal> container)
                        && int.TryParse(varHash, out int varHashCode) && container.ContainsKey(varHashCode))
                        {
                            container[varHashCode]--;
                            await container.EditMessage(session, session.telegramId, callbackQuerry.Message.MessageId);
                        }
                        else
                        {
                            await session.BotClient.EditMessageTextAsync(session.telegramId, callbackQuerry.Message.MessageId, session.Translate("Error"));
                        }
                    }
                },
                // Вызов при обработке нажатия владельцем
                {DefaultStrings.OWNER, (session, callbackQuerry) =>
                    {
                        BotOwner botOwner = session.BotOwner;
                        if(session == botOwner.Session)
                        {
                            string cuttedData = callbackQuerry.Data.Substring(callbackQuerry.Data.IndexOf('_') + 1); // удалить "Owner_"
                            //Owner_ActionID_userID
                            string actionId = ButtonIdManager.GetActionNameFromCallbackData(cuttedData);
                            if(botOwner.actions.TryGetValue(actionId, out var action))
                            {
                                action.Invoke(botOwner.GetSessionByID(ButtonIdManager.GetIDFromCallbackData(cuttedData)));
                            }
                            else
                            {
                                session.BotClient.SendTextMessageAsync(callbackQuerry.Message.Chat.Id, session.Translate("ThisCommandIsUnknown") + " ActionId=" + actionId);
                            }
                        }
                        else
                        {
                            session.BotClient.SendTextMessageAsync(callbackQuerry.Message.Chat.Id, session.Translate("ThisCommandIsUnknown") + " ActionId=" + DefaultStrings.OWNER);
                        }
                    }
                },
                // Заглушка, когда необходимо по нажатию на кнопку ничего не делать и при этом не отправлять сообщение об ошибке
                {DefaultStrings.DONOTHING, (session, callbackQuerry) => { }}
            };
        }

        private bool CanExecuteAction(Session session, string specialName) => session.CurrentNode.CanExecute(specialName, session);

        public void Filter(Session session, Message message)
        {
            string key = message.Text?.Trim() ?? "";
            int indexOfCommandEnd = key.IndexOf(' ');
            if (indexOfCommandEnd < 0) indexOfCommandEnd = key.Length;
            key = key.Substring(0, indexOfCommandEnd);

            if (messageActions.TryGetValue(key, out var action))
            {
                action.Invoke(session, message);
            }
            else
            {
				session.BotClient.SendTextMessageAsync(session.telegramId, session.Translate("ThisCommandIsUnknown"));
				session.GoToNode(session.CurrentNode);
			}
        }

        public void Filter(Session session, CallbackQuery callbackQuerry)
        {
            string key = ButtonIdManager.GetActionNameFromCallbackData(callbackQuerry.Data);

            if (callbackActions.TryGetValue(key, out var action))
            {
                action.Invoke(session, callbackQuerry);
            }
            else
            {
				session.BotClient.EditMessageTextAsync(session.telegramId, callbackQuerry.Message.MessageId, session.Translate("Error"));
			}
        }
    }
}
