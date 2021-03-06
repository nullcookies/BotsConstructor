﻿using System;
using System.Linq;
using System.Threading.Tasks;
using LogicalCore.TreeNodes;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace LogicalCore
{
    public class MetaMultiMessage : IMetaMessage
    {
        private readonly IMetaMessage[] messages;
        public int Count { get; private set; }
        public bool HaveReplyKeyboard { get; private set; }
        public bool HaveInlineKeyboard { get; private set; }
        private int defaultMessageIndex = 0;
        /// <summary>
        /// Индекс сообщения, к которому будут добавляться кнопки.
        /// </summary>
        public int DefaultMessageIndex
        {
            get => defaultMessageIndex;

            set
            {
                if (value < 0 || value >= Count) throw new ArgumentOutOfRangeException(nameof(value));
                defaultMessageIndex = value;
            }
        }
        public IMetaReplyMarkup MetaKeyboard => messages[defaultMessageIndex].MetaKeyboard;
		public MessageType MessageType => messages[defaultMessageIndex].MessageType;
		public ITranslatable Text => messages[defaultMessageIndex].Text;
		public InputOnlineFile File => messages[defaultMessageIndex].File;

		public MetaMultiMessage(int count = 2)
        {
            if (count <= 0) throw new ArgumentException("Мультисообщение должно состоять минимум из одного сообщения!", nameof(count));

            messages = new IMetaMessage[count];

            for(int i = 0; i < count; i++)
            {
                messages[i] = new MetaMessage();
            }

            Count = count;

            CheckKeyboards();
        }

        public MetaMultiMessage(params IMetaMessage[] metaMessages)
        {
            Count = metaMessages.Length;

            if (Count <= 0) throw new ArgumentException("Мультисообщение должно состоять минимум из одного сообщения!", nameof(metaMessages));

            messages = metaMessages;
            
            CheckKeyboards();
        }

        public IMetaMessage this[int index]
        {
            get => messages[index];

            set
            {
                messages[index] = value ?? throw new ArgumentNullException(nameof(value));
                if (!HaveReplyKeyboard && value.HaveReplyKeyboard) HaveReplyKeyboard = true;
                if (!HaveInlineKeyboard && value.HaveInlineKeyboard) HaveInlineKeyboard = true;
            }
        }

        private void CheckKeyboards()
        {
            HaveReplyKeyboard = messages.Any((msg) => msg.HaveReplyKeyboard);
            HaveInlineKeyboard = messages.Any((msg) => msg.HaveInlineKeyboard);
        }

        public void AddNextButton(int rowNumber = 1) =>
            messages[defaultMessageIndex].AddNextButton(rowNumber);

        public void AddNodeButton(ITreeNode node, params Predicate<ISession>[] rules) =>
            messages[defaultMessageIndex].AddNodeButton(node, rules);

        public void AddNodeButton(int rowNumber, ITreeNode node, params Predicate<ISession>[] rules) =>
            messages[defaultMessageIndex].AddNodeButton(rowNumber, node, rules);

        public void InsertBackButton(ITreeNode parent, int rowNumber = 0, int columnNumber = 0) =>
            messages[defaultMessageIndex].InsertBackButton(parent, rowNumber, columnNumber);

        public void InsertPreviousButton(int rowNumber = 1, int columnNumber = 0) =>
            messages[defaultMessageIndex].InsertPreviousButton(rowNumber, columnNumber);

        public async Task<Message> SendMessage(ISession session)
        {
            Task<Message> lastTask = messages[0].SendMessage(session);

            for(int i = 1; i < Count; i++)
            {
                lastTask = await lastTask.ContinueWith((task) =>
                messages[i].SendMessage(session),
                TaskContinuationOptions.NotOnFaulted);
            }

            return await lastTask;
        }
    }
}
