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
        
        public void GrabTheCash()
        {
            var relativeUrl = string.Concat("/users/", _authToken.AccountId, "/basic_incomes");
            var grabBasicIncomeUrl = ResponseHelper.PalaiBaseUrl + relativeUrl; 

            Console.WriteLine("Getting Income Page");
            var initalGet = _client.GetAsync(grabBasicIncomeUrl);
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

        }


        public float getCashAmountFromDashBoard()
        {
            var relativeUrl = string.Concat("/users/", _authToken.AccountId, "/account/dashboard");
            var dashboardurl = ResponseHelper.PalaiBaseUrl + relativeUrl;

            Console.WriteLine("Getting Dashboard Page");
            var initalGet = _client.GetAsync(dashboardurl);
            var documentContent = _responseHelper.ResponseToString(initalGet);

            int start = 0;

            start = documentContent.IndexOf("/users/"+ _authToken.AccountId + "/telephone_number_claim");
            if(start != -1)
            {
                Console.WriteLine("===============================================");
                Console.WriteLine("The Palai Account outdates soon");
                Console.WriteLine("===============================================");
            }
            start = 0;
            while (true)
            {                
                start = documentContent.IndexOf("¶", start +1);
                if (start == -1) break;

                var end = documentContent.LastIndexOf('>', start);

                var possiblyAFloat = documentContent.Substring(end + 1, start - end - 2).Trim();
                if(float.TryParse(possiblyAFloat, out float afloat))
                {
                    //Console.WriteLine("Found a PalaiValue " + afloat);
                    return afloat;
                }
            }

            Console.WriteLine("No float like value found that could be the balance");
            return float.NaN;
        }
    }
}
