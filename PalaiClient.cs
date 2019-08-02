using System;
using System.Collections.Generic;
using System.Net.Http;
using static PalaiAutoGrabber.Program;

namespace PalaiAutoGrabber
{

    public class AuthToken
    {
        public int AccountId { get; set; }
        public string Cookie { get; set; }
    }

    public class PalaiClient : IPalaiClient
    {          
        private ResponseHelper _responseHelper;
        private FormHelper _formHelper;
        private HttpClient _client;

        public PalaiClient(ResponseHelper responseHelper,
            FormHelper formHelper)
        {
            _responseHelper = responseHelper;
            _formHelper = formHelper;

            HttpClientHandler httpClientHandler = new HttpClientHandler();
            httpClientHandler.AllowAutoRedirect = false;
            _client = new HttpClient(httpClientHandler);
        }
        
        public IAuthentificatedPalaiClient Login(Account account)
        {
            var loginUrl = ResponseHelper.PalaiBaseUrl + "/u/sign_in";
            Console.WriteLine("Getting Login Page");
            _client.DefaultRequestHeaders.Add("Referer", loginUrl);
            _client.DefaultRequestHeaders.Add("Host", "palai.org");
            _client.DefaultRequestHeaders.Add("Origin", "https://palai.org");
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.142 Safari/537.36");

            var loginGet = Await(_client.GetAsync(loginUrl));
           
            var htmlDoc = _responseHelper.ResponseToHtml(loginGet);
            var cookie = _responseHelper.ExtractAuthCookie(loginGet);
            var authToken = _formHelper.GetAuthTokenFromForm(htmlDoc);
            Console.WriteLine("AuthToken found" + authToken);

            var token = new AuthToken();
            token.AccountId = account.Id;
                
            //_client.DefaultRequestHeaders.Add("Cookie", token.Cookie);  
            var postbackBody = _formHelper.FillForm(_formHelper.Utf8(), _formHelper.AuthValue(authToken), PostableAccount(account));
            Console.WriteLine("Logging In");
            var response = Await(_client.PostAsync(loginUrl, postbackBody));
            if (response.StatusCode != System.Net.HttpStatusCode.Found)
                throw new Exception("Expected a redirect to the dashboard");
            Console.WriteLine("Redirect");
            var data = Await(response.Content.ReadAsStringAsync());
            if (!data.Contains("account/dashboard"))
                throw new Exception("Expected a redirect to the dashboard");

            _responseHelper.ExtractAuthCookie(response);

            Console.WriteLine("Succeed");

            return new AuthentificatedPalaiClient(_responseHelper, _formHelper,_client, token);                       
        }
                
        private static IEnumerable<Tuple<string, string>> PostableAccount(Account account)
        {
            yield return Tuple.Create("user[email]", account.UserName);
            yield return Tuple.Create("user[password]", account.Password);
            yield return Tuple.Create("user[remember_me]", "0");
            yield return Tuple.Create("commit", "Einloggen");
        }
    }
}
