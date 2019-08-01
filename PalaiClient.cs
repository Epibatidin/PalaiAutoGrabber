using System;
using System.Collections.Generic;
using System.Net.Http;

namespace PalaiAutoGrabber
{

    public class AuthToken
    {
        public int AccountId { get; set; }
        public string Cookie { get; set; }

        public string AuthFormValue { get; set; }
    }

    public class PalaiClient : IPalaiClient
    {        
        public static IPalaiClient CreateClient(HttpHelper httpHelper)
        {
            return new PalaiClient(httpHelper);
        }
        public HttpHelper HttpHelper { get; }
        public PalaiClient(HttpHelper httpHelper)
        {
            HttpHelper = httpHelper;
        }
        
        private HttpClient GetHttpClient()
        {
            var webClient = new HttpClient();
                  
            return webClient;
        }

        public IAuthentificatedPalaiClient Login(Account account)
        {
            var loginUrl = HttpHelper.PalaiBaseUrl + "/u/sign_in";

            var loginGet = GetHttpClient().GetAsync(loginUrl);

            var htmlDoc = HttpHelper.ResponseToHtml(loginGet);

            var formNode = htmlDoc.DocumentNode.SelectSingleNode("//form");
            if (formNode == null)
                throw new Exception("expected to be on the login page - but there was no postback form");

            var authNode = formNode.SelectSingleNode(".//input[@name='authenticity_token']");
            var authToken = authNode.GetAttributeValue("value", "notset");

            var client = GetHttpClient();

            var token = new AuthToken();            
            token.AuthFormValue = authToken;
            token.AccountId = account.Id;

            var postbackBody = HttpHelper.FillForm(PostableAccount(account), HttpHelper.AuthValue(token));

            var response = client.PostAsync(loginUrl, postbackBody).ConfigureAwait(false).GetAwaiter().GetResult();

            var cookie = HttpHelper.ExtractAuthCookie(response);
            token.Cookie = cookie;

            return new AuthentificatedPalaiClient(HttpHelper, token);                       
        }
                
        private static IEnumerable<Tuple<string, string>> PostableAccount(Account account)
        {
            yield return Tuple.Create("user[email]", account.UserName);
            yield return Tuple.Create("user[password]", account.Password);
            yield return Tuple.Create("commit", "Anmelden");
        }
    }
}
