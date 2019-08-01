using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace LogicalCore
{
    public class MetaMessage : MetaMessage<IMetaReplyMarkup>
    {
        public MetaMessage(
            MetaText metaText = null,
            MessageType messageType = MessageType.Text,
            InputOnlineFile messageFile = null,
            IMetaReplyMarkup messageKeyboard = null,
            ParseMode parsing = ParseMode.Default) :
            base(metaText, messageType, messageFile, messageKeyboard, parsing)
        {
            MetaKeyboard = MetaKeyboard ?? new MetaReplyKeyboardMarkup();
        }

        public MetaMessage(params string[] textData) : base(textData)
        {
            MetaKeyboard = MetaKeyboard ?? new MetaReplyKeyboardMarkup();
        }
    }

    public class MetaInlineMessage : MetaMessage<MetaInlineKeyboardMarkup>
    {
        public MetaInlineMessage(
            MetaText metaText = null,
            MessageType messageType = MessageType.Text,
            InputOnlineFile messageFile = null,
            MetaInlineKeyboardMarkup messageKeyboard = null,
            ParseMode parsing = ParseMode.Default) :
            base(metaText, messageType, messageFile, messageKeyboard, parsing)
        {
            MetaKeyboard = MetaKeyboard ?? new MetaInlineKeyboardMarkup();
        }

        public MetaInlineMessage(params string[] textData) : base(textData)
        {
            MetaKeyboard = MetaKeyboard ?? new MetaInlineKeyboardMarkup();
        }
    }

    public class MetaReplyMessage : MetaMessage<MetaReplyKeyboardMarkup>
    {
        public MetaReplyMessage(
            MetaText metaText = null,
            MessageType messageType = MessageType.Text,
            InputOnlineFile messageFile = null,
            MetaReplyKeyboardMarkup messageKeyboard = null,
            ParseMode parsing = ParseMode.Default) :
            base(metaText, messageType, messageFile, messageKeyboard, parsing)
        {
            MetaKeyboard = MetaKeyboard ?? new MetaReplyKeyboardMarkup();
        }

        public MetaReplyMessage(params string[] textData) : base(textData)
        {
            MetaKeyboard = MetaKeyboard ?? new MetaReplyKeyboardMarkup();
        }
    }

    public class MetaMessage<KeyboardType> : IMetaMessage<KeyboardType>, IMetaMessage where KeyboardType : class, IMetaReplyMarkup
    {
        public MessageType Type { get; }
        public MetaText Text { get; }
        public InputOnlineFile File { get; private set; }
        public KeyboardType MetaKeyboard { get; protected set; }
        IMetaReplyMarkup IMetaMessage.MetaKeyboard => MetaKeyboard;
        public bool HaveReplyKeyboard => MetaKeyboard?.HaveReplyKeyboard ?? false;
        public bool HaveInlineKeyboard => MetaKeyboard?.HaveInlineKeyboard ?? false;
        public readonly ParseMode parseMode;

        public MetaMessage(
            MetaText metaText = null,
            MessageType messageType = MessageType.Text,
            InputOnlineFile messageFile = null,
            KeyboardType messageKeyboard = null,
            ParseMode parsing = ParseMode.Default)
        {
            Type = messageType;
            Text = metaText ?? new MetaText();
            if (messageType != MessageType.Text && messageFile == null)
                throw new ArgumentNullException(nameof(File), "Отсутствие файла разрешено только при MessageType.Text.");
            File = messageFile;
            MetaKeyboard = messageKeyboard;
            parseMode = parsing;
        }

        public MetaMessage(params string[] textData) : this(metaText: textData) { }

        /// <summary>
        /// Добавляет кнопку для узла.
        /// </summary>
        /// <param name="node">Узел, для которого необходимо добавить кнопку.</param>
        /// <param name="rules">Список правил, при выполнении которых кнопка должна быть показана.</param>
        public void AddNodeButton(Node node, params Predicate<Session>[] rules) => MetaKeyboard.AddNodeButton(node, rules);

        /// <summary>
        /// Добавляет кнопку для узла в указанную строку.
        /// </summary>
        /// <param name="rowNumber">Строка, в которую необходимо добавить кнопку.</param>
        /// <param name="node">Узел, для которого необходимо добавить кнопку.</param>
        /// <param name="rules">Список правил, при выполнении которых кнопка должна быть показана.</param>
        public void AddNodeButton(int rowNumber, Node node, params Predicate<Session>[] rules) => MetaKeyboard.AddNodeButton(rowNumber, node, rules);

        /// <summary>
        /// Вставляет кнопку "Назад" в указанное место.
        /// </summary>
        /// <param name="parent">Родитель узла, для которого нужна кнопка.</param>
        /// <param name="rowNumber">Номер строки.</param>
        /// <param name="columnNumber">Номер столбца.</param>
        public void InsertBackButton(Node parent, int rowNumber = 0, int columnNumber = 0)
            => MetaKeyboard.InsertBackButton(parent, rowNumber, columnNumber);

        /// <summary>
        /// Добавляет кнопку "Дальше" в указанное место.
        /// </summary>
        /// <param name="rowNumber">Номер строки.</param>
        public void AddNextButton(int rowNumber = 1)
            => MetaKeyboard.AddNextButton(rowNumber);

        /// <summary>
        /// Вставляет кнопку "Предыдущие" в указанное место.
        /// </summary>
        /// <param name="rowNumber">Номер строки.</param>
        /// <param name="columnNumber">Номер столбца.</param>
        public void InsertPreviousButton(int rowNumber = 1, int columnNumber = 0) =>
            MetaKeyboard.InsertPreviousButton(rowNumber, columnNumber);

        /// <summary>
        /// Сохраняет ID отправленного файла, чтобы можно было не загружать файл повторно.
        /// </summary>
        /// <param name="fileBase">Объект файла, который хранит ID.</param>
        private void FileIdSaving(FileBase fileBase)
        {
            if(File.FileType == FileType.Stream)
            {
                File = new InputOnlineFile(fileBase.FileId);
            }
        }

        /// <summary>
        /// Выполняет отправку переведённого сообщения указанной сессии.
        /// </summary>
        /// <param name="session">Сессия, для которой нужно сделать перевод и отправку сообщения.</param>
        /// <returns>Возвращает Task<Message> с отправкой сообщения.</returns>
        public async Task<Message> SendMessage(Session session)
        {
            Task<Message> sendingTask = null;
            switch (Type)
            {
                //case MessageType.Unknown:
                //    break;
                case MessageType.Text:
                    sendingTask = session.BotClient.SendTextMessageAsync(
                        session.telegramId,
                        Text.ToString(session),
                        parseMode,
                        replyMarkup: MetaKeyboard?.Translate(session));
                    break;
                case MessageType.Photo:
                    sendingTask = session.BotClient.SendPhotoAsync(
                        session.telegramId,
                        File,
                        Text.ToString(session),
                        parseMode,
                        replyMarkup: MetaKeyboard?.Translate(session));
                    FileIdSaving((await sendingTask).Photo[0]);
                    break;
                case MessageType.Audio:
                    sendingTask = session.BotClient.SendAudioAsync(
                        session.telegramId,
                        File,
                        Text.ToString(session),
                        parseMode,
                        replyMarkup: MetaKeyboard?.Translate(session));
                    FileIdSaving((await sendingTask).Audio);
                    break;
                case MessageType.Video:
                    sendingTask = session.BotClient.SendVideoAsync(
                        session.telegramId,
                        File,
                        caption: Text.ToString(session),
                        parseMode: parseMode,
                        replyMarkup: MetaKeyboard?.Translate(session));
                    FileIdSaving((await sendingTask).Video);
                    break;
                case MessageType.Voice:
                    sendingTask = session.BotClient.SendVoiceAsync(
                        session.telegramId,
                        File,
                        Text.ToString(session),
                        parseMode,
                        replyMarkup: MetaKeyboard?.Translate(session));
                    FileIdSaving((await sendingTask).Voice);
                    break;
                case MessageType.Document:
                    sendingTask = session.BotClient.SendDocumentAsync(
                        session.telegramId,
                        File,
                        Text.ToString(session),
                        parseMode,
                        replyMarkup: MetaKeyboard?.Translate(session));
                    FileIdSaving((await sendingTask).Document);
                    break;
                case MessageType.Sticker:
                    sendingTask = session.BotClient.SendStickerAsync(
                        session.telegramId,
                        File,
                        replyMarkup: MetaKeyboard?.Translate(session));
                    FileIdSaving((await sendingTask).Sticker);
                    break;
                //case MessageType.Location:
                //    break;
                //case MessageType.Contact:
                //    break;
                //case MessageType.Venue:
                //    break;
                //case MessageType.Game:
                //    break;
                //case MessageType.VideoNote:
                //    break;
                //case MessageType.Invoice:
                //    break;
                //case MessageType.SuccessfulPayment:
                //    break;
                //case MessageType.WebsiteConnected:
                //    break;
                //case MessageType.ChatMembersAdded:
                //    break;
                //case MessageType.ChatMemberLeft:
                //    break;
                //case MessageType.ChatTitleChanged:
                //    break;
                //case MessageType.ChatPhotoChanged:
                //    break;
                //case MessageType.MessagePinned:
                //    break;
                //case MessageType.ChatPhotoDeleted:
                //    break;
                //case MessageType.GroupCreated:
                //    break;
                //case MessageType.SupergroupCreated:
                //    break;
                //case MessageType.ChannelCreated:
                //    break;
                //case MessageType.MigratedToSupergroup:
                //    break;
                //case MessageType.MigratedFromGroup:
                //    break;
                //case MessageType.Animation:
                //    break;
                //case MessageType.Poll:
                //    break;
                default:
                    throw new NotImplementedException($"Поддержка сообщений типа {Type} не реализована.");
                    //break;
            }

            return await sendingTask;
        }
    }
}
