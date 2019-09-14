using System;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace LogicalCore
{
	public class AudioInputNode : FileInputNode
    {
        public AudioInputNode(string name, string varName, TryConvert<(string FileId, string PreviewId, string Description)> converter,
            IMetaMessage metaMessage = null, bool required = true, bool needBack = true)
            : base(name, varName, converter, metaMessage, required, needBack) { }

        public AudioInputNode(string name, string varName, TryConvert<(string FileId, string PreviewId, string Description)> converter,
            string description, bool required = true, bool needBack = true)
            : this(name, varName, converter, new MetaMessage(description ?? name), required, needBack) { }

		protected override bool TryGoToChild(Session session, Message message)
		{
			if (!base.TryGoToChild(session, message))
			{
				if (Converter.Invoke(message.Text, out (string FileId, string PreviewId, string Description) variable))
				{
					switch (message.Type)
					{
						case MessageType.Unknown:
							ConsoleWriter.WriteLine($"Пользователь {message.From.Username} отправил сообщение неизвестного типа.", ConsoleColor.Red);
							return false;
						case MessageType.Audio:
							variable.PreviewId = message.Audio.Thumb?.FileId;
							variable.FileId = message.Audio.FileId;
							variable.Description = variable.Description ?? "";
							if(!string.IsNullOrWhiteSpace(message.Audio.Title))
							{
								variable.Description += "\n" + message.Audio.Title;
							}
							if (!string.IsNullOrWhiteSpace(message.Audio.Performer))
							{
								variable.Description += "\n" + message.Audio.Performer;
							}
							break;
						case MessageType.Voice:
							variable.FileId = message.Voice.FileId;
							break;
						case MessageType.Document:
							if (message.Document.MimeType.StartsWith("audio"))
							{
								variable.PreviewId = message.Document.Thumb?.FileId;
								variable.FileId = message.Document.FileId;
							}
							else
							{
								return false;
							}
							break;
						default:
							ConsoleWriter.WriteLine($"Пользователь {message.From.Username} отправил сообщение неподдерживаемого типа {message.Type}.", ConsoleColor.DarkYellow);
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
