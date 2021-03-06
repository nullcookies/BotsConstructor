﻿using System.Collections.Generic;

namespace LogicalCore.TreeNodes.TemplateNodes
{
	/// <summary>
	/// Узел, позволяющий добавить привязанный к нему элемент для сессии.
	/// </summary>
	/// <typeparam name="T">Тип в <see cref="MetaValued{T}"/>.</typeparam>
	/// <remarks>Берёт из глобального словаря элемент, соответствующий указанному ID и добавляет его в указанный контейнер сессии.</remarks>
	public class ItemNode<T> : ActionNode
	{
		public ItemNode(string itemsContainer, int itemID, string sessionContainer, MetaInlineMessage metaMessage = null, string name = null)
			: base((session) =>
				{
					var item = session.BotWrapper.GlobalVars.GetVar<Dictionary<int, MetaValued<T>>>(itemsContainer)[itemID];
					var container = session.Vars.GetVar<MetaValuedContainer<T>>(sessionContainer);
					if(container == null)
					{
						container = new MetaValuedContainer<T>(sessionContainer);
						session.Vars.SetVar(container);
					}
					container.Add(item, 1);
				}, metaMessage, name ?? DefaultStrings.Add) { }

		public ItemNode(string itemsContainer, int itemID, string sessionContainer, string description, string name = null)
			: this(itemsContainer, itemID, sessionContainer, description == null ? null : new MetaInlineMessage(description), name) { }
	}
}
