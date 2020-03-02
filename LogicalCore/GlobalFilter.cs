using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LogicalCore.TreeNodes;
using Telegram.Bot.Types;

namespace LogicalCore
{
    /// <summary>
    /// Глобальный фильтр действий, которые могут быть вызваны с любого узла.
    /// </summary>
    public class GlobalFilter
    {
        private readonly Dictionary<string, Func<Session, Message, Task>> messageFuncs;
        private readonly Dictionary<string, Func<Session, CallbackQuery, Task>> callbackFuncs;

        public GlobalFilter()
        {
            messageFuncs = new Dictionary<string, Func<Session, Message, Task>>()
            {
                {"/start", async (session, message) =>
                    {
                        await session.BotClient.SendTextMessageAsync(
                            message.Chat.Id,
                            session.Translate(DefaultStrings.Hello));

						session.GoToNode(session.MarkupTree.Root);
                    }
                },
                {"/show", async (session, message) =>
                    {
                        string command = message.Text.Trim();
                        if(command.Length <= 6)
                        {
                            await session.BotClient.SendTextMessageAsync(message.Chat.Id, session.Translate(DefaultStrings.UnknownCommand));
                            return;
                        }
                        string containerName = command.Substring(6);
						if(session.vars.TryGetVar(containerName, out MetaValuedContainer<decimal> container))
						{
							await container.SendMessage(session);
						}
						else
						{
							await session.BotClient.SendTextMessageAsync(message.Chat.Id, session.Translate(DefaultStrings.UnknownCommand));
						}
					}
                }
            };

            callbackFuncs = new Dictionary<string, Func<Session, CallbackQuery, Task>>()
            {
                //Переход к узлу
                {DefaultStrings.GoTo, async (session, callbackQuerry) =>
                    {
                        int nodeId = ButtonIdManager.GetIDFromCallbackData(callbackQuerry.Data);
                        var node = session.MarkupTree.GetNodeById(nodeId);
                        session.GoToNode(node, out var task);
                        await task;
                    }
                },
                //Вызов при нажатии влево/вправо в глобальной листалке или на текущем узле
                {DefaultStrings.ShowPage, async (session, callbackQuerry) =>
                    {
                        int nodeID = ButtonIdManager.GetIDFromCallbackData(callbackQuerry.Data);
						var node = session.MarkupTree.GetNodeById(nodeID);
						if(node is IFlippable flipper && (flipper.GlobalCallbacks || session.CurrentNode == node))
                        {
                            int page = ButtonIdManager.GetPageFromCallbackData(callbackQuerry.Data);
                            await flipper.SendPage(session, callbackQuerry.Message, page);
                        }
                        else
                        {
							await session.BotClient.EditMessageTextAsync(session.telegramId, callbackQuerry.Message.MessageId, session.Translate(DefaultStrings.Error));
                        }
                    }
                },
                //Добавить в контейнере какому-то значению 1
                {DefaultStrings.Plus, async (session, callbackQuerry) =>
                    {
                        string containerName = ButtonIdManager.GetNextSubstring(callbackQuerry.Data, DefaultStrings.Plus.Length, out int nextUnder);
                        string varHash = ButtonIdManager.GetNextSubstring(callbackQuerry.Data, nextUnder);

                        if(session.vars.TryGetVar(containerName, out MetaValuedContainer<decimal> container)
                        && int.TryParse(varHash, out int varHashCode) && container.ContainsKey(varHashCode))
                        {
                            container[varHashCode]++;
                            await container.EditMessage(session, session.telegramId, callbackQuerry.Message.MessageId);
                        }
                        else
                        {
                            await session.BotClient.EditMessageTextAsync(session.telegramId, callbackQuerry.Message.MessageId, session.Translate(DefaultStrings.Error));
                        }
                    }
                },
                //Отнять в контейнере от какого-то значения 1
                {DefaultStrings.Minus, async (session, callbackQuerry) =>
                    {
                        string containerName = ButtonIdManager.GetNextSubstring(callbackQuerry.Data, DefaultStrings.Minus.Length, out int nextUnder);
                        string varHash = ButtonIdManager.GetNextSubstring(callbackQuerry.Data, nextUnder);

                        if(session.vars.TryGetVar(containerName, out MetaValuedContainer<decimal> container)
                        && int.TryParse(varHash, out int varHashCode) && container.ContainsKey(varHashCode))
                        {
                            container[varHashCode]--;
                            await container.EditMessage(session, session.telegramId, callbackQuerry.Message.MessageId);
                        }
                        else
                        {
                            await session.BotClient.EditMessageTextAsync(session.telegramId, callbackQuerry.Message.MessageId, session.Translate(DefaultStrings.Error));
                        }
                    }
                },
                // Вызов при обработке нажатия владельцем
                {DefaultStrings.Owner, async (session, callbackQuerry) =>
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
                                await session.BotClient.SendTextMessageAsync(callbackQuerry.Message.Chat.Id, session.Translate(DefaultStrings.UnknownCommand) + " ActionId=" + actionId);
                            }
                        }
                        else
                        {
                            await session.BotClient.SendTextMessageAsync(callbackQuerry.Message.Chat.Id, session.Translate(DefaultStrings.UnknownCommand) + " ActionId=" + DefaultStrings.Owner);
                        }
                    }
                },
                // Заглушка, когда необходимо по нажатию на кнопку ничего не делать и при этом не отправлять сообщение об ошибке
                {DefaultStrings.DoNothing, async (session, callbackQuerry) => { }}
            };
        }

        private bool CanExecuteAction(Session session, string specialName) => session.CurrentNode.CanExecute(specialName, session);

        public void Filter(Session session, Message message)
        {
            try
            {
                string key = message.Text?.Trim() ?? "";
                int indexOfCommandEnd = key.IndexOf(' ');
                if (indexOfCommandEnd < 0) indexOfCommandEnd = key.Length;
                key = key.Substring(0, indexOfCommandEnd);

                if (messageFuncs.TryGetValue(key, out var func))
                {
                    func.Invoke(session, message).Wait();
                }
                else
                {
                    session.BotClient.SendTextMessageAsync(session.telegramId, session.Translate(DefaultStrings.UnknownCommand)).Wait();
                    session.GoToNode(session.CurrentNode);
                }
            }
            catch (Exception e)
            {
                ConsoleWriter.WriteLine("Не удалось обработать сообщение: " + e.Message, ConsoleColor.Red);
            }
        }

        public void Filter(Session session, CallbackQuery callbackQuerry)
        {
            try
            {
                string key = ButtonIdManager.GetActionNameFromCallbackData(callbackQuerry.Data);

                if (callbackFuncs.TryGetValue(key, out var func))
                {
                    func.Invoke(session, callbackQuerry).Wait();
                }
                else
                {
                    session.BotClient.EditMessageTextAsync(session.telegramId, callbackQuerry.Message.MessageId, session.Translate(DefaultStrings.Error)).Wait();
                }
            }
            catch (Exception)
            {
                try
                {
                    session.BotClient.DeleteMessageAsync(session.telegramId, callbackQuerry.Message.MessageId).Wait();
                    session.GoToNode(session.CurrentNode);
                }
                catch (Exception e)
                {
                    ConsoleWriter.WriteLine("Не удалось удалить сообщение: " + e.Message, ConsoleColor.Red);
                }

                session.BotClient.SendTextMessageAsync(session.telegramId, session.Translate(DefaultStrings.UnknownCommand));
            }
        }
    }
}
