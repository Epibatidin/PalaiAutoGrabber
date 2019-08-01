using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace PalaiAutoGrabber
{
    public class AuthentificatedPalaiClient : IAuthentificatedPalaiClient
    {
        public HttpHelper HttpHelper { get; }
        private readonly AuthToken _authToken;

        public AuthentificatedPalaiClient(HttpHelper httpHelper, AuthToken authToken)
        {
            HttpHelper = httpHelper;
            _authToken = authToken;
        }

        private HttpClient GetHttpClient()
        {
            var webClient = new HttpClient();
            webClient.DefaultRequestHeaders.Add("Cookie", _authToken.Cookie);
            webClient.DefaultRequestHeaders.Add("Host", "palai.org");
            return webClient;
        }


        public int GrabTheCash()
        {
            var grabBasicIncomeUrl = string.Concat(HttpHelper.PalaiBaseUrl, "/users/", _authToken.AccountId, "/basic_incomes");

            var formValues = HttpHelper.FillForm(HttpHelper.AuthValue(_authToken));

            var client = GetHttpClient();
            client.DefaultRequestHeaders.Add("Referer", grabBasicIncomeUrl);

            var response = client.PostAsync(grabBasicIncomeUrl, formValues);
            var htmlDoc = HttpHelper.ResponseToHtml(response);

            var balanceNode = htmlDoc.DocumentNode.SelectSingleNode("//td[@class='current-balance']");
            if (balanceNode == null)
                throw new Exception("I guess something went wrong there is no 'current-balance' node");

            return int.Parse(balanceNode.InnerText.Trim());
        }
    }


}
