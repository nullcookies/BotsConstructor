using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace LogicalCore
{
    /// <summary>
    /// Содержит 2 сообщения, одно с reply-клавиатурой, другое - с inline.
    /// </summary>
    public class MetaDoubleKeyboardedMessage : MetaMultiMessage, IMetaMessage<MetaReplyKeyboardMarkup>, IMetaMessage<MetaInlineKeyboardMarkup>
    {
        public readonly MetaMessage<MetaReplyKeyboardMarkup> replyMessage;
        public readonly MetaMessage<MetaInlineKeyboardMarkup> inlineMessage;
        public MetaReplyKeyboardMarkup ReplyMarkup => replyMessage.MetaKeyboard;
        MetaReplyKeyboardMarkup IMetaMessage<MetaReplyKeyboardMarkup>.MetaKeyboard => ReplyMarkup;
        public MetaInlineKeyboardMarkup InlineMarkup => inlineMessage.MetaKeyboard;
        MetaInlineKeyboardMarkup IMetaMessage<MetaInlineKeyboardMarkup>.MetaKeyboard => InlineMarkup;
        public new bool HaveReplyKeyboard => true;
        public new bool HaveInlineKeyboard => true;
        public readonly bool replyFirst;
        private bool downLocation;
        public bool DownButtonsLocation
        {
            get => downLocation;
            set
            {
                if(value ^ replyFirst) // XOR для определения позиции
                {
                    DefaultMessageIndex = 1;
                }
                else
                {
                    DefaultMessageIndex = 0;
                }

                downLocation = value;
            }
        }

        public MetaDoubleKeyboardedMessage(
            MetaReplyKeyboardMarkup replyKeyboard = null,
            MetaInlineKeyboardMarkup inlineKeyboard = null,
            MetaText metaReplyText = null,
            MetaText metaInlineText = null,
            MessageType messageType = MessageType.Text,
            InputOnlineFile messageFile = null,
            ParseMode parsing = ParseMode.Markdown,
            bool useReplyMsgForFile = false,
            bool useReplyMsgForButtons = true,
            bool replyMsgFirst = true) :
            base(2)
        {
            replyKeyboard = replyKeyboard ?? new MetaReplyKeyboardMarkup();
            inlineKeyboard = inlineKeyboard ?? new MetaInlineKeyboardMarkup();

            replyFirst = replyMsgFirst;

            DownButtonsLocation = useReplyMsgForButtons;

            if(useReplyMsgForFile)
            {
                replyMessage = new MetaMessage<MetaReplyKeyboardMarkup>(metaReplyText, messageType, messageFile, replyKeyboard, parsing);
                inlineMessage = new MetaMessage<MetaInlineKeyboardMarkup>(metaInlineText, MessageType.Text, null, inlineKeyboard, parsing);
            }
            else
            {
                replyMessage = new MetaMessage<MetaReplyKeyboardMarkup>(metaReplyText, MessageType.Text, null, replyKeyboard, parsing);
                inlineMessage = new MetaMessage<MetaInlineKeyboardMarkup>(metaInlineText, messageType, messageFile, inlineKeyboard, parsing);
            }

            if (replyMsgFirst)
            {
                this[0] = replyMessage;
                this[1] = inlineMessage;
            }
            else
            {
                this[0] = inlineMessage;
                this[1] = replyMessage;
            }
        }

        public MetaDoubleKeyboardedMessage(MetaText name, MetaText description = null)
            : this(metaReplyText: name, metaInlineText: description ?? name) { }
    }
}
