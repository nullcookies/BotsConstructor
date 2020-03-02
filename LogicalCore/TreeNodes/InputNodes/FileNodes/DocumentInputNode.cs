using System;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace LogicalCore
{
	public class DocumentInputNode : FileInputNode
    {
        public DocumentInputNode(string name, string varName, TryConvert<(string FileId, string PreviewId, string Description)> converter,
            IMetaMessage metaMessage = null, bool required = true, bool needBack = true)
            : base(name, varName, converter, metaMessage, required, needBack) { }

        public DocumentInputNode(string name, string varName, TryConvert<(string FileId, string PreviewId, string Description)> converter,
            string description, bool required = true, bool needBack = true)
            : this(name, varName, converter, new MetaMessage(description ?? name), required, needBack) { }

		protected override bool TryGoToChild(ISession session, Message message)
		{
			if (!base.TryGoToChild(session, message))
			{
				if (Converter.Invoke(message.Text, out (string FileId, string PreviewId, string Description) variable))
				{
					if(message.Type == MessageType.Document)
					{
						variable.PreviewId = message.Document.Thumb?.FileId;
						variable.FileId = message.Document.FileId;
					}
					else
					{
						return false;
					}

					SetVar(session, variable);
					GoToChildNode(session, Children[0]);
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return true;
			}
		}
	}
}
