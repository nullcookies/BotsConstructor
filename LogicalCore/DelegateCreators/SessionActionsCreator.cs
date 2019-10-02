using System;
using System.Text;
using System.Collections.Generic;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace LogicalCore
{
	public static class SessionActionsCreator
	{
		public static Action<Session> JoinActions(params Action<Session>[] actions) =>
			(Session session) =>
			{
				foreach (var action in actions)
				{
					action.Invoke(session);
				}
			};

		public static Action<Session> RemoveVariable(Type varType, string varName) =>
			(Session session) =>
			{
				session.vars.RemoveVar(varType, varName);
			};

        public static Action<Session> ClearVariables(IEnumerable<(Type varType, string varName)> variables) =>
            (Session session) =>
            {
                foreach (var (varType, varName) in variables)
                {
                    session.vars.ClearVar(varType, varName);
                }
            };

        public static Action<Session> ClearVariables(params (Type varType, string varName)[] variables) =>
			(Session session) =>
			{
				foreach (var (varType, varName) in variables)
				{
					session.vars.ClearVar(varType, varName);
				}
			};

		public static Action<Session> SendTextToOwner(Type varType, string varName, bool disableNotification = false) =>
			(Session session) =>
			{
				session.BotClient.SendTextMessageAsync(
					session.BotOwner.id,
					session.vars.GetVar(varType, varName).ToString(session.BotOwner.Session),
					ParseMode.Markdown,
					true,
					disableNotification);
			};

		public static Action<Session> SendDocumentToOwner(string onlineFileName,
			Type descVarType = null, string descVarName = null, bool disableNotification = false) =>
			(Session session) =>
			{
				session.BotClient.SendDocumentAsync(
					session.BotOwner.id,
					session.vars.GetVar<InputOnlineFile>(onlineFileName),
					(descVarType == null || descVarName == null) ? null :
					(session.vars.TryGetVar(descVarType, descVarName, out object description) ?
					description.ToString(session.BotOwner.Session) : null),
					disableNotification: disableNotification);
			};

		public static Action<Session> SendOrder(IOrdersSendable sendable, int statusGroupID,
			Func<Session, UniversalOrderContainer> contFunc) =>
			(Session session) =>
			{
				sendable.SendOrder(session, contFunc?.Invoke(session), statusGroupID);
			};

		public static Action<Session> SendNotificationToOwner(bool disableNotification = false, bool writeUser = true,
			string separator = "\n", Func<Session, MetaInlineKeyboardMarkup> keyboardCreator = null, params (Type varType, string varName)[] variables)
		{
			List<string> files = new List<string>();
			var notFiles = new List<(Type type, string name)>();

			foreach (var (varType, varName) in variables)
			{
				if (varType == typeof(InputOnlineFile))
				{
					files.Add(varName);
				}
				else
				{
					notFiles.Add((varType, varName));
				}
			}

			if (files.Count > 0)
			{
				return async (Session session) =>
				{
					bool needSendToOwner = session.BotOwner != null;

					StringBuilder messageBuilder = writeUser ? new StringBuilder(session.Me) : new StringBuilder();

					foreach (var (type, name) in notFiles)
					{
						var variable = session.vars.GetVar(type, name);
						if (variable != null)
						{
							messageBuilder.Append(variable.ToString(session.BotOwner.Session));
							messageBuilder.Append(separator);
						}
					}

					Message msg = null;
					if (needSendToOwner)
					{
						msg = await session.BotClient.SendTextMessageAsync(
							session.BotOwner.id,
							messageBuilder.ToString(),
							ParseMode.Markdown,
							true,
							disableNotification,
							replyMarkup: keyboardCreator?.Invoke(session).Translate(session.BotOwner.Session));
					}

					foreach (var fileName in files)
					{
						var file = session.vars.GetVar<InputOnlineFile>(fileName);
						if(file != null)
						{
							if(needSendToOwner)
							{
								await session.BotClient.SendDocumentAsync(
									session.BotOwner.id,
									file,
									writeUser ? session.Me : null,
									ParseMode.Markdown,
									disableNotification,
									msg.MessageId);
							}
						}
					}
				};
			}
			else
			{
				return async (Session session) =>
				{
					bool needSendToOwner = session.BotOwner != null;

					StringBuilder messageBuilder = writeUser ? new StringBuilder(session.Me) : new StringBuilder();

					foreach (var (type, name) in notFiles)
					{
						var variable = session.vars.GetVar(type, name);
						if(variable != null)
						{
							messageBuilder.Append(variable.ToString(session.BotOwner.Session));
							messageBuilder.Append(separator);
						}
					}

					if(needSendToOwner)
					{
						await session.BotClient.SendTextMessageAsync(
							session.BotOwner.id,
							messageBuilder.ToString(),
							ParseMode.Markdown,
							true,
							disableNotification,
							replyMarkup: keyboardCreator?.Invoke(session).Translate(session.BotOwner.Session));
					}
				};
			}
		}
	}
}
