using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using PalaiAutoGrabber;

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
            var httpHelper = new HttpHelper();
            foreach (var account in palaiConfig.Accounts)
            {
                try
                {                    
                    var palaiClient = PalaiClient.CreateClient(httpHelper);

                    var loggedIn = palaiClient.Login(account);

                    loggedIn.GrabTheCash();

                    /*
                    // finally do the important thing 
                    var grabBasicIncomeUrl = string.Concat(palaiBaseUrl, "/users/", account.Id, "/basic_incomes");
                    Console.WriteLine($"Using {grabBasicIncomeUrl} to grab the cash");

                    var balance = CollectTheCash(grabBasicIncomeUrl, authCookie);
                    Console.WriteLine("You are rich becasue you own " + balance + " fantasy money");

                    if (palaiConfig.PushBalanceNotification)
                        SendToPushoverApi(grabBasicIncomeUrl,balance);
                        */
                }
                catch(Exception e)
                {
                    justEndWhenDone = false;
                    Console.WriteLine(e.Message);
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
    }
}  