using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using PalaiAutoGrabber;

namespace PalaiAutoGrabber
{
    public class Program
    {
        private const string TransfersUrl = "/a/name+someGUID/transactions"; // ToDo change
        private const string PushoverKey = "token=foo&user=bar&message=PalimPalim"; // ToDo change

        private const string palaiBaseUrl = "https://palai.org";

        private static void Main()
        {
            Console.WriteLine("Yet Another Day To Grab some Fantasy Money");

            ConfigurationBuilder configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("appSettings.json", false);

            var config = configBuilder.Build();

            var palaiConfig = config.Get<Configuration>();

            bool justEndWhenDone = palaiConfig.Silent;

            foreach (var account in palaiConfig.Accounts)
            {
                try
                {
                    var authCookie = Login(account);

                    // finally do the important thing 
                    var grabBasicIncomeUrl = string.Concat(palaiBaseUrl, "/users/", account.Id, "/basic_incomes");
                    Console.WriteLine($"Using {grabBasicIncomeUrl} to grab the cash");

                    var balance = CollectTheCash(grabBasicIncomeUrl, authCookie);
                    Console.WriteLine("You are rich becasue you own " + balance + " fantasy money");

                    if (palaiConfig.PushBalanceNotification)
                        SendToPushoverApi(grabBasicIncomeUrl,balance);
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

        private static string CollectTheCash(string grabBasicIncomeUrl, string authCookie)
        {
            var transferData = DownloadString(grabBasicIncomeUrl, authCookie);
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(transferData);
            var balanceNode = htmlDoc.DocumentNode.SelectSingleNode("//td[@class='current-balance']");
            if (balanceNode == null)
                throw new Exception("I guess something went wrong there is no 'current-balance' node");

            return balanceNode.InnerText.Trim();
        }

        private static IEnumerable<Tuple<string, string>> PostableAccount(Account account, string authToken)
        {
            yield return Tuple.Create("authenticity_token", authToken);
            yield return Tuple.Create("user[email]", account.UserName);
            yield return Tuple.Create("user[password]", account.Password);
            yield return Tuple.Create("commit", "Anmelden");
        }

        private static string Login(Account account)
        {
            var loginUrl = palaiBaseUrl + "/u/sign_in";

            var content = DownloadString(loginUrl, null);  
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(content);
            var formNode = htmlDoc.DocumentNode.SelectSingleNode("//form");
            if(formNode == null)
                throw new Exception("expected to be on the login page - but there was no postback form");

            var authNode = formNode.SelectSingleNode(".//input[@name='authenticity_token']");
            var authToken = authNode.GetAttributeValue("value", "notset");

            var client = new HttpClient();
            StringBuilder postBackData = new StringBuilder();
            foreach (var kv in PostableAccount(account, authToken))
            {
                postBackData.Append(kv.Item1).Append('=').Append(kv.Item1).Append('&');
            }
            postBackData.Length--;
            
            var toSend = WebUtility.UrlEncode(postBackData.ToString());

            var response = client.PostAsync(loginUrl,
                new StringContent(toSend, Encoding.UTF8, "application/x-www-form-urlencoded")).GetAwaiter().GetResult();

            return ExtractAuthCookie(response);
        }
        
        private static string DownloadString(string address, string authCookie)
        {
            using (var webClient = new HttpClient())
            {
                if (authCookie != null)
                    webClient.DefaultRequestHeaders.Add("Cookie", authCookie);

                var response = webClient.GetAsync(address).GetAwaiter().GetResult();
                var data = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                return data;
            }
        }

        private static string ExtractAuthCookie(HttpResponseMessage response)
        {
            var isSetCookie = response.Headers.TryGetValues("Set-Cookie", out var setCookies);
            if (isSetCookie && setCookies != null)
            {
                foreach (var setCookie in setCookies)
                {
                    if (!setCookie.StartsWith("_palai_community_site_session=", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var endIndex = setCookie.IndexOf(';');
                    var newCookie = setCookie.Substring(0, endIndex);
                    return newCookie;
                }
            }

            throw new Exception("Expected to find a Auth Cookie but mh there was nothing");
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