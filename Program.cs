using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace PalaiAutoGrabber
{
    public class Program
    {
        private const string TransfersUrl = "/a/name+someGUID/transactions"; // ToDo change
        private const string PushoverKey = "token=foo&user=bar&message=PalimPalim"; // ToDo change

        private static void Main()
        {
            Console.WriteLine("Yet Another Day To Grab some");

            ConfigurationBuilder configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("appSettings.json", false);

            var config = configBuilder.Build();

            var palaiConfig = config.Get<Configuration>();

            bool justEndWhenDone = palaiConfig.Silent;
            var responseHelper = new ResponseHelper();
            var formHelper = new FormHelper();
            foreach (var account in palaiConfig.Accounts)
            {
                try
                {                    
                    var palaiClient = new PalaiClient(responseHelper, formHelper);

                    var loggedIn = palaiClient.Login(account);

                    var cash = loggedIn.GrabTheCash();
                    Console.WriteLine("Current Balance : " + cash);
                }
                catch(Exception e)
                {
                    justEndWhenDone = false;
                    Console.WriteLine(e.Message);
                    //throw;
                }
            };
            Console.WriteLine("Nothing more todo");
            if(!justEndWhenDone)
            {
                Console.WriteLine("Press the \"Any Key\" - Key to close");
                Console.ReadLine();
            }
        }
               
        private static void SendToPushoverApi(string grabBasicIncomeUrl, string balance)
        {
            var client = new HttpClient();
            var toSend = string.Format($"{PushoverKey} {balance}&title=PalimPalim&url={grabBasicIncomeUrl}");
            var now = DateTime.Now;
            if (now.Hour >= 22 || now.Hour < 7)
                toSend = string.Format("{0}&sound=none", toSend);

            client.PostAsync("https://api.pushover.net/1/messages.json", new StringContent(toSend, Encoding.UTF8, "application/x-www-form-urlencoded")).GetAwaiter().GetResult();
        }
        
        public static T Await<T>(Task<T> task)
        {
            return task.ConfigureAwait(false).GetAwaiter().GetResult();
        }

    }
}  