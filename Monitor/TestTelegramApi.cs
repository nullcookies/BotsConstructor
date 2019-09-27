using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Telegram.Bot;
using TLSharp.Core;

namespace Monitor
{
    public class TestTelegramApi
    {
        private TelegramClient _client;
                               
        //string phoneNumber = "3836";
        //string apiHash = "5bf949";
        private string _phoneHash;
        int apiId = 1117995;

        public async Task SendCodeAsync()
        {
            Console.WriteLine("SendCodeAsync Start");

            var store = new FileSessionStore();
            _client = new TelegramClient(apiId, apiHash, store);
            await _client.ConnectAsync();
            _phoneHash= await _client.SendCodeRequestAsync(phoneNumber);

            Console.WriteLine("SendCodeAsync Finish");

        }

        public async Task MakeAuthAsync(string code)
        {

            Console.WriteLine("MakeAuthAsync Start");


            var user = await _client.MakeAuthAsync(phoneNumber, _phoneHash, code);
            var result = await _client.GetContactsAsync();


            Console.WriteLine("MakeAuthAsync Finish");

        }
    }
}
