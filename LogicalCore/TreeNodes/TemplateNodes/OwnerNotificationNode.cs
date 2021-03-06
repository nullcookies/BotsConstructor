﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace LogicalCore.TreeNodes.TemplateNodes
{
	/// <summary>
	/// Оповещает владельца специальным сообщением, сформированным из данных сессии.
	/// </summary>
	public class OwnerNotificationNode : ActionNode
	{
        public OwnerNotificationNode(string name, MetaInlineMessage metaMessage = null, IOrdersSendable sendable = null,
            int statusGroupID = 0, Func<ISession, UniversalOrderContainer> containerCreator = null,
            IEnumerable<(Type varType, string varName)> variables = null)
            : base(SessionActionsCreator.JoinActions(
                SessionActionsCreator.SendOrder(sendable, statusGroupID, containerCreator),
                SessionActionsCreator.ClearVariables(variables)
            ), metaMessage, name)
        { }

        public OwnerNotificationNode(string name, MetaInlineMessage metaMessage = null, IOrdersSendable sendable = null,
			int statusGroupID = 0, Func<ISession, UniversalOrderContainer> containerCreator = null,
			params (Type varType, string varName)[] variables)
			: base(SessionActionsCreator.JoinActions(
				SessionActionsCreator.SendOrder(sendable, statusGroupID, containerCreator),
				SessionActionsCreator.ClearVariables(variables)
				), metaMessage, name) { }

		public OwnerNotificationNode(string name, string description, IOrdersSendable sendable = null,
			int statusGroupID = 0, Func<ISession, UniversalOrderContainer> containerCreator = null,
			params (Type varType, string varName)[] variables)
			: this(name, description == null ? null : new MetaInlineMessage(description), sendable, statusGroupID,
				  containerCreator, variables) { }

		public OwnerNotificationNode(string name, MetaInlineMessage metaMessage = null, bool disableNotification = false,
			bool writeUser = true, string separator = "\n", Func<ISession, MetaInlineKeyboardMarkup> keyboardCreator = null,
			params (Type varType, string varName)[] variables)
			: base(SessionActionsCreator.JoinActions(
				SessionActionsCreator.SendNotificationToOwner(disableNotification, writeUser, separator,
					keyboardCreator,
					variables),
				SessionActionsCreator.ClearVariables(variables)
				), metaMessage, name) { }

		public OwnerNotificationNode(string name, string description, bool disableNotification = false,
			bool writeUser = true, string separator = "\n", Func<ISession, MetaInlineKeyboardMarkup> keyboardCreator = null,
			params (Type varType, string varName)[] variables)
			: this(name, description == null ? null : new MetaInlineMessage(description), disableNotification,
				  writeUser, separator, keyboardCreator, variables) { }

		protected override Task<Message> SendMarkupIfNoChildren(ISession session) => session.MarkupTree.Root.SendMessage(session);
	}
}
