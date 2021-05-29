using System;
using System.Collections.Generic;
using System.Net.Http;
using static PalaiAutoGrabber.Program;

namespace PalaiAutoGrabber
{

    public class AuthToken
    {
        public int AccountId { get; set; }
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
            httpClientHandler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;

            _client = new HttpClient(httpClientHandler);
        }
        
        public IAuthentificatedPalaiClient Login(Account account)
        {
            string relativeUrl = "/u/sign_in";
            var loginUrl = ResponseHelper.PalaiBaseUrl + relativeUrl;
            Console.WriteLine("Getting Login Page");
            
            var loginGet = _client.GetAsync(loginUrl);           
            var htmlDoc = _responseHelper.ResponseToHtml(loginGet);
            var authToken = _formHelper.GetAuthTokenFromForm(htmlDoc, relativeUrl);
            Console.WriteLine("AuthToken found" + authToken);

            var token = new AuthToken();
            token.AccountId = account.Id;
                
            var postbackBody = _formHelper.FillForm(_formHelper.AuthValue(authToken), PostableAccount(account));
            Console.WriteLine("Logging In");
            var response = Await(_client.PostAsync(loginUrl, postbackBody));
            if (response.StatusCode != System.Net.HttpStatusCode.Found)
                throw new Exception("Expected a redirect to the dashboard");
            Console.WriteLine("Redirect");
            var data = Await(response.Content.ReadAsStringAsync());
            if (!data.Contains("account/dashboard"))
                throw new Exception("Expected a redirect to the dashboard");

            Console.WriteLine("Succeed");
            return new AuthentificatedPalaiClient(_responseHelper, _formHelper,_client, token);                       
        }
                
        private static IEnumerable<Tuple<string, string>> PostableAccount(Account account)
        {
            yield return Tuple.Create("user[email]", account.UserName);
            yield return Tuple.Create("user[password]", account.Password);
        }
    }
}
