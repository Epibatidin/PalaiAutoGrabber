using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using HtmlAgilityPack;

namespace PalaiAutoGrabber.Pages
{
    public class IncomePage
    {
        private readonly AuthToken _authToken;
        private readonly IPalaiClient _client;
        private HtmlDocument _activePageHtml;
        private string _relativeUrl;
        public IncomePage(AuthToken authToken, IPalaiClient client)
        {
            _authToken = authToken;
            _client = client;

            _relativeUrl = $"/users/{_authToken.AccountId}/basic_incomes";
        }

        public async Task Navigate()
        {
            Console.WriteLine("Getting Income Page");
            _activePageHtml = await _client.GetHtmlAsync(_relativeUrl);
        }

        public async Task<bool> GrabTheCash()
        {
            Console.WriteLine("Will Try to Grab the cash soon");
            FormContext formContext = null;
            try
            {
                formContext = FormHelper.FindFormContext(_activePageHtml, _relativeUrl);
            }
            catch(FormNotFoundException e)
            {
                Console.WriteLine(e.Message);                
                return false;
            }
            Console.WriteLine("So then grab the cash");

            var response = await _client.PostAsync(_relativeUrl, formContext.AuthToken);
            if (response.StatusCode != System.Net.HttpStatusCode.Found)
                throw new Exception("Expected a redirect to basic income page");
            var data = await response.Content.ReadAsStringAsync();
            if (!data.Contains("/basic_incomes"))
                throw new Exception("Expected a redirect to basic income page");
            return true;
        }
    }
}
