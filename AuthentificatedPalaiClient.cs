using System;
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
        
        public int GrabTheCash()
        {
            var relativeUrl = string.Concat("/users/", _authToken.AccountId, "/basic_incomes");
            var grabBasicIncomeUrl = ResponseHelper.PalaiBaseUrl + relativeUrl; 

            Console.WriteLine("Getting Income Page");
            var initalGet = Await(_client.GetAsync(grabBasicIncomeUrl));
            var htmlDoc = _responseHelper.ResponseToHtml(initalGet);
            var authToken = _formHelper.GetAuthTokenFromForm(htmlDoc, relativeUrl);
           

            var formValues = _formHelper.FillForm(_formHelper.AuthValue(authToken));
            Console.WriteLine("So then grab the cash");
            
            var response = Await(_client.PostAsync(grabBasicIncomeUrl, formValues));

            var balancePostbackResult = _responseHelper.ResponseToHtml(response);
            
            Console.WriteLine(balancePostbackResult);

            var balanceNode = balancePostbackResult.DocumentNode.SelectSingleNode("//td[@class='current-balance']");
            if (balanceNode == null)
                throw new Exception("I guess something went wrong there is no 'current-balance' node");
            
            return int.Parse(balanceNode.InnerText.Trim());
        }
    }
}
