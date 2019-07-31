using System;
using Telegram.Bot.Types;

namespace LogicalCore
{
    /// <summary>
    /// Класс для удобной проверки, может ли текущий узел обрабатывать определённую стандартную команду.
    /// </summary>
    public static class KeyboardActionsManager
    {
        public static bool CheckNeeding(bool haveButton, bool haveReplyKeyboard, Session session, Message message, string neededKey, Func<bool> func = null)
        {
            if (haveButton && haveReplyKeyboard)
            {
                string text = message.Text;
                if(session.TryRetranslate(text, out string key) && key == neededKey)
                {
                    return func == null || func.Invoke();
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool CheckNeeding(bool haveButton, bool haveInlineKeyboard, Session session, CallbackQuery callbackQuerry, string neededAction, Func<bool> func = null)
        {
            if (haveButton && haveInlineKeyboard)
            {
                string actionName = ButtonIdManager.GetActionNameFromCallbackData(callbackQuerry.Data);
                if (actionName == neededAction)
                {
                    return func == null || func.Invoke();
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
