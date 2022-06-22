using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using HtmlAgilityPack;

namespace PalaiAutoGrabber.Pages
{
    public class LoginPage
    {
        private readonly IPalaiClient _client;
        private string _relativeUrl = "/u/sign_in";
        private HtmlDocument _activePageHtml;

        public AuthToken AuthToken { get; private set; }

        public LoginPage(IPalaiClient palaiClient)
        {
            _client = palaiClient;
        }


        public async Task Navigate()
        {
            Console.WriteLine("Getting Login Page");

            var loginGet = _client.GetHtmlAsync(_relativeUrl);
            _activePageHtml = await loginGet;
        }

        public async Task<DashboardPage> Login(Account account)
        {
            var formContext = FormHelper.FindFormContext(_activePageHtml, _relativeUrl);            
            var token = new AuthToken();
            token.AccountId = account.Id;

            Console.WriteLine("Logging In");
            var response = await _client.PostAsync(formContext.Url, PostableAccount(account), formContext.AuthToken);
            if (response.StatusCode != System.Net.HttpStatusCode.Found)
                throw new Exception("Expected a redirect to the dashboard");
            Console.WriteLine("Redirect");
            var data = await response.Content.ReadAsStringAsync();
            if (!data.Contains("account/dashboard"))
                throw new Exception("Expected a redirect to the dashboard");

            Console.WriteLine("Succeed");
            AuthToken = token;
            return new DashboardPage(token, _client);
        }

        private static IEnumerable<Tuple<string, string>> PostableAccount(Account account)
        {
            yield return Tuple.Create("user[email]", account.UserName);
            yield return Tuple.Create("user[password]", account.Password);
        }
    }
}
