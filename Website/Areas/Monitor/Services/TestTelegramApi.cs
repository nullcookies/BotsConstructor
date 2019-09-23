using System;
using System.Threading.Tasks;
using TeleSharp.TL;
using TLSharp.Core;

namespace Website.Areas.Monitor.Services
{
    public class TestTelegramApi
    {
        private const int API_ID = 1117995;
        private const string API_HASH = "5bf0f315f9f8d13931ed21b688aaf949";
        private const string PHONE_NUMBER = "380956680136";
        private TelegramClient _client;
        private string _phoneHash;
        private TLUser user;


        public void SendImageToBot(int botId, byte[] image)
        {
            TLAbsChat chat = new TLChat();
            //_client.SendUploadedPhoto(new TLInputPeerChat(), new TLInputFile(), )
        }

        #region Start telegram agent

        public async Task SendCodeAsync()
        {
            Console.WriteLine("SendCodeAsync Start");

            var store = new FileSessionStore();
            _client = new TelegramClient(API_ID, API_HASH, store);
            await _client.ConnectAsync();
            _phoneHash = await _client.SendCodeRequestAsync(PHONE_NUMBER);

            Console.WriteLine("SendCodeAsync Finish");
        }

        public async Task MakeAuthAsync(string code)
        {
            Console.WriteLine("MakeAuthAsync Start");


            user = await _client.MakeAuthAsync(PHONE_NUMBER, _phoneHash, code);
            var result = await _client.GetContactsAsync();


            Console.WriteLine("MakeAuthAsync Finish");
        }

        #endregion
    }
}