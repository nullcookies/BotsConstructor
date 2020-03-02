using System;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace LogicalCore
{
	public abstract class FileInputNode : UsualInputNode<(string FileId, string PreviewId, string Description)>
    {
        public FileInputNode(string name, string varName, TryConvert<(string FileId, string PreviewId, string Description)> converter,
            IMetaMessage metaMessage = null, bool required = true, bool needBack = true)
            : base(name, varName, converter, metaMessage, required, needBack, false) { }

        public FileInputNode(string name, string varName, TryConvert<(string FileId, string PreviewId, string Description)> converter,
            string description, bool required = true, bool needBack = true)
            : this(name, varName, converter, new MetaMessage(description ?? name), required, needBack) { }

		protected override bool TryGoToChild(ISession session, Message message)
		{
			var child = Children[0];
			if (KeyboardActionsManager.CheckNeeding(this.message.MetaKeyboard?.CanShowButton(child.Name, session) ?? false,
					this.message.HaveReplyKeyboard, session, message, child.Name))
			{
				GoToChildNode(session, child);
				return true;
			}
			else
			{
				return false;
				#region Тестирование типов файлов
				//if (Converter.Invoke(message.Text, out (string FileId, string PreviewId, string Description) variable))
				//{
				//	switch (message.MessageType) //Просто чтобы видеть список обрабатываемых типов и для тестирования
				//	{
				//		case MessageType.Unknown:
				//			ConsoleWriter.WriteLine($"Пользователь {message.From.Username} отправил сообщение неизвестного типа.", ConsoleColor.Red);
				//			return false;
				//		case MessageType.Text:
				//			return false;
				//		case MessageType.Photo:
				//			variable.PreviewId = message.Photo[0].FileId;
				//			if(message.Photo.Length > 1)
				//			{
				//				variable.FileId = message.Photo[1].FileId;
				//			}
				//			else
				//			{
				//				variable.FileId = message.Photo[0].FileId;
				//			}
				//			break;
				//		case MessageType.Audio:
				//			variable.PreviewId = message.Audio.Thumb?.FileId;
				//			variable.FileId = message.Audio.FileId;
				//			variable.Description = variable.Description ?? "";
				//			if (!string.IsNullOrWhiteSpace(message.Audio.Title))
				//			{
				//				variable.Description += "\n" + message.Audio.Title;
				//			}
				//			if (!string.IsNullOrWhiteSpace(message.Audio.Performer))
				//			{
				//				variable.Description += "\n" + message.Audio.Performer;
				//			}
				//			break;
				//		case MessageType.Video:
				//			variable.PreviewId = message.Video.Thumb?.FileId;
				//			variable.FileId = message.Video.FileId;
				//			break;
				//		case MessageType.Voice:
				//			variable.FileId = message.Voice.FileId;
				//			break;
				//		case MessageType.Document:
				//			variable.PreviewId = message.Document.Thumb?.FileId;
				//			variable.FileId = message.Document.FileId;
				//			break;
				//		case MessageType.Sticker:
				//			variable.Description = message.Sticker.Emoji;
				//			variable.PreviewId = message.Sticker.Thumb?.FileId;
				//			variable.FileId = message.Sticker.FileId;
				//			break;
				//		case MessageType.VideoNote:
				//			variable.PreviewId = message.VideoNote.Thumb?.FileId;
				//			variable.FileId = message.VideoNote.FileId;
				//			break;
				//		//case MessageType.Location:
				//		//	break;
				//		//case MessageType.Contact:
				//		//	break;
				//		//case MessageType.Venue:
				//		//	break;
				//		default:
				//			ConsoleWriter.WriteLine($"Пользователь {message.From.Username} отправил сообщение неподдерживаемого типа {message.MessageType}.", ConsoleColor.DarkYellow);
				//			return false;
				//	}
				//	//if(Memoization)
				//	try
				//	{
				//		var desc = "*Description:* " + variable.Description;
				//		session.BotClient.SendTextMessageAsync(session.telegramId, desc, ParseMode.Markdown);
				//		var fileId = "*FileId:* " + variable.FileId;
				//		session.BotClient.SendTextMessageAsync(session.telegramId, fileId, ParseMode.Markdown);
				//		var previewId = "*PreviewId:* " + variable.PreviewId;
				//		session.BotClient.SendTextMessageAsync(session.telegramId, previewId, ParseMode.Markdown);
				//		var filePath = "*File file_path:* " + session.BotClient.GetFileAsync(variable.FileId).Result.FilePath;
				//		session.BotClient.SendTextMessageAsync(session.telegramId, filePath, ParseMode.Markdown);
				//		ConsoleWriter.WriteLine(filePath, ConsoleColor.Magenta);
				//		if (variable.PreviewId != null)
				//		{
				//			var previewPath = "*Preview file_path:* " + session.BotClient.GetFileAsync(variable.FileId).Result.FilePath;
				//			ConsoleWriter.WriteLine(previewPath, ConsoleColor.Magenta);
				//			session.BotClient.SendTextMessageAsync(session.telegramId, previewPath, ParseMode.Markdown);
				//		}
				//	}
				//	catch (Exception ex)
				//	{
				//		ConsoleWriter.WriteLine(ex.Message, ConsoleColor.Red);
				//		session.BotClient.SendTextMessageAsync(session.telegramId, ex.Message, ParseMode.Markdown);
				//	}
				//	SetVar(session, variable);
				//	GoToChildNode(session, Children[0]);
				//	return true;
				//}
				//else
				//{
				//	return false;
				//}
				#endregion
			}
		}

		// Файл нельзя передать через Callback в этот узел
		protected override bool TryGoToChild(ISession session, CallbackQuery callbackQuerry) =>
			base.TryGoToChild(session, callbackQuerry);
	}
}
