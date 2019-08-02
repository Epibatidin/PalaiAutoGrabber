using System;
using System.Collections.Generic;
using System.Net.Http;
using static PalaiAutoGrabber.Program;

namespace PalaiAutoGrabber
{
    public class AuthentificatedPalaiClient : IAuthentificatedPalaiClient
    {
        private readonly AuthToken _authToken;
        private readonly HttpClient _client;
        private ResponseHelper _responseHelper;
        private FormHelper _formHelper;

        public AuthentificatedPalaiClient(ResponseHelper responseHelper,
            FormHelper formHelper,
            HttpClient client,
            AuthToken authToken)
        {
            _responseHelper = responseHelper;
            _formHelper = formHelper;
            _authToken = authToken;
            _client = client;
        }
        
        private IEnumerable<Tuple<string, string>> MoreStuff()
        {
            yield return Tuple.Create("commit", "Abholen");
        }
        /*
        private HttpClient GetHttpClient(string url)
        {
           
            client.DefaultRequestHeaders.Add("Referer", url);
            client.DefaultRequestHeaders.Add("Host", "palai.org");
            client.DefaultRequestHeaders.Add("Cookie", _authToken.Cookie);
            //client.DefaultRequestHeaders.Add("Origin", "https://palai.org");
            
            return client;
        }
        */

        public int GrabTheCash()
        {
            var grabBasicIncomeUrl = string.Concat(ResponseHelper.PalaiBaseUrl, "/users/", _authToken.AccountId, "/basic_incomes");

            //var client = GetHttpClient(grabBasicIncomeUrl);
            Console.WriteLine("Getting Income Page");
            var initalGet = Await(_client.GetAsync(grabBasicIncomeUrl));
            var htmlDoc = _responseHelper.ResponseToHtml(initalGet);
            var authToken = _formHelper.GetAuthTokenFromForm(htmlDoc);
            _responseHelper.ExtractAuthCookie(initalGet);

            var formValues = _formHelper.FillForm(_formHelper.AuthValue(authToken));
            Console.WriteLine("So then lets get dangerous");

            _client.DefaultRequestHeaders.Remove("Referer");
            _client.DefaultRequestHeaders.Add("Referer", grabBasicIncomeUrl);

            var response = Await(_client.PostAsync(grabBasicIncomeUrl, formValues));
            var balancePostbackResult = _responseHelper.ResponseToHtml(response);
            var cookie = _responseHelper.ExtractAuthCookie(response);
            Console.WriteLine("Cookie found" + cookie);
            Console.WriteLine(balancePostbackResult);

            var balanceNode = balancePostbackResult.DocumentNode.SelectSingleNode("//td[@class='current-balance']");
            if (balanceNode == null)
                throw new Exception("I guess something went wrong there is no 'current-balance' node");
            
            return int.Parse(balanceNode.InnerText.Trim());
        }
    }
}
