using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeleSharp.TL;
using TLSharp.Core;
using TLSharp.Core.Utils;

namespace Website.TelegramAgent
{
    public class MyTelegramAgent
    {
        private const int API_ID = 1117995;
        private const string API_HASH = "5bf0f315f9f8d13931ed21b688aaf949";
        private const string PHONE_NUMBER = "380956680136";


        private TelegramClient _client;
        private string _phoneHash;
        private bool _isWorking;
        
 
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

            if (_isWorking)
                throw new Exception("Телеграм агент уже работает");
            
            
            var user = await _client.MakeAuthAsync(PHONE_NUMBER, _phoneHash, code);
            _isWorking = true;
            
            Console.WriteLine("MakeAuthAsync Finish");
        }

        
        public void MakeAuth(string code)
        {
            Console.WriteLine("MakeAuthAsync Start");

            if (_isWorking)
                throw new Exception("Телеграм агент уже работает");
            
            _client.MakeAuthAsync(PHONE_NUMBER, _phoneHash, code).Wait();
            
            _isWorking = true;
            
            Console.WriteLine("MakeAuthAsync Finish");
        }

        public async Task MySendPhoto(Stream stream)
        {
            var fileResult = await _client.UploadFile("myFile.jpg", new StreamReader(stream));
            

            TeleSharp.TL.Contacts.TLFound found = await _client.SearchUserAsync("sea_battle_robot");

            //Что это за говно?
            var accessHash = ((TLUser) found.Users.ToList()[0]).AccessHash;
            
            if (accessHash != null)
            {
                long hash = accessHash.Value;
                int id = ((TLUser) found.Users.ToList()[0]).Id;

                TLInputPeerUser peer = new TLInputPeerUser()
                {
                    UserId = id,
                    AccessHash = hash
                };

//                await _client.SendMessageAsync(peer, "Добридень");

                await _client.SendUploadedPhoto(peer, fileResult,"guid");
                
            }
            else
            {
                throw new Exception("accessHash == null");
            }
        }
        

    }
}