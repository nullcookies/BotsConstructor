using System;
using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace LogicalCore
{
	public class ImageInputNode : FileInputNode
    {
		public readonly bool useStickers;

        public ImageInputNode(string name, string varName, TryConvert<(string FileId, string PreviewId, string Description)> converter,
            IMetaMessage metaMessage = null, bool required = true, bool needBack = true, bool stickers = false)
            : base(name, varName, converter, metaMessage, required, needBack)
		{
			useStickers = stickers;
		}

        public ImageInputNode(string name, string varName, TryConvert<(string FileId, string PreviewId, string Description)> converter,
            string description, bool required = true, bool needBack = true, bool stickers = false)
            : this(name, varName, converter, new MetaMessage(description ?? name), required, needBack, stickers) { }

		protected override bool TryGoToChild(ISession session, Message message)
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
						case MessageType.Photo:
							variable.PreviewId = message.Photo[0].FileId;
							variable.FileId = message.Photo.LastOrDefault().FileId;
							break;
						case MessageType.Document:
							if(message.Document.MimeType.StartsWith("image"))
							{
								variable.PreviewId = message.Document.Thumb?.FileId;
								variable.FileId = message.Document.FileId;
							}
							else
							{
								return false;
							}
							break;
						case MessageType.Sticker:
							if(useStickers)
							{
								variable.Description = message.Sticker.Emoji;
								variable.FileId = message.Sticker.FileId;
								variable.PreviewId = message.Sticker.Thumb?.FileId ?? message.Sticker.FileId;
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
