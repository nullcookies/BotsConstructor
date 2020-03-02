namespace LogicalCore.TreeNodes.TemplateNodes.ConfigurationNodes
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
				DefaultStrings.Language,
				DefaultStrings.Language,
				botWrapper.Languages,
				requestMessage == null ? null : new MetaDoubleKeyboardedMessage(requestMessage),
				pageSize, needBack: needBack),
			new ActionNode(
				(session) => session.Language = session.Vars.GetVar<string>(DefaultStrings.Language),
				answerMessage == null ? null : new MetaInlineMessage(answerMessage)
				)
			)
		{
			TailNode.SetParent(HeadNode);
		}
	}
}
