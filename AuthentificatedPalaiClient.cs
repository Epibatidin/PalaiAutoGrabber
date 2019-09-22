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
            if (response.StatusCode != System.Net.HttpStatusCode.Found)
                throw new Exception("Expected a redirect to basic income page");
            var data = Await(response.Content.ReadAsStringAsync());
            if (!data.Contains("/basic_incomes"))
                throw new Exception("Expected a redirect to basic income page");

            Console.WriteLine("Get the Basic Income page again");
            var getForRetrieveMoneyAmount = Await(_client.GetAsync(grabBasicIncomeUrl));
            // we got a redirect and we are not following it 
            if(getForRetrieveMoneyAmount.StatusCode != System.Net.HttpStatusCode.Redirect)
            {
                Console.WriteLine("Expected 302 statuscode to basicIncome page but got : " + 
                    getForRetrieveMoneyAmount.Headers.GetValues("Location"));
            }

            Console.WriteLine("I asume it worked");

            return 0;
        }
    }
}
