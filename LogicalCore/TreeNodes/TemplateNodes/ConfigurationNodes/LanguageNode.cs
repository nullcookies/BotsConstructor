namespace LogicalCore
{
	/// <summary>
	/// Узел, позволяющий сменить язык сессии.
	/// </summary>
	public class LanguageNode : CombinedNodes
	{
		public LanguageNode(BotWrapper botWrapper, MetaText requestMessage = null, MetaText answerMessage = null,
			byte pageSize = 6, bool needBack = true) : base
			(
			new SelectSingleInputNode<string>(
				DefaultStrings.LANGUAGE,
				DefaultStrings.LANGUAGE,
				botWrapper.Languages,
				requestMessage == null ? null : new MetaDoubleKeyboardedMessage(requestMessage),
				pageSize, needBack: needBack),
			new ActionNode(
				(session) => session.Language = session.vars.GetVar<string>(DefaultStrings.LANGUAGE),
				answerMessage == null ? null : new MetaInlineMessage(answerMessage)
				)
			)
		{
			tailNode.SetParent(headNode);
		}
	}
}
