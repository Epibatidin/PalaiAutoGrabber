using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using HtmlAgilityPack;

namespace PalaiAutoGrabber.Pages
{
    public class RecertificationPage
    {
        private readonly string _url;
        private readonly AuthToken _token;
        private readonly IPalaiClient _client;
        private HtmlDocument _activePageHtml;
        private Uri _redirectedUri;

        private int step = 0;

        public RecertificationPage(string url, AuthToken token, IPalaiClient client)
        {
            _url = url;
            _token = token;
            _client = client;
        }

        public async Task Navigate()
        {
            Console.WriteLine("Getting RecertificationPage Page");
            step = 0;
            var response = await _client.GetAsync(_url);
            _redirectedUri = response.Headers.Location;
            if (_redirectedUri != null)
            {
                Console.WriteLine("Following redirect to ", _redirectedUri);
                var loginGet = _client.GetHtmlAsync(_redirectedUri.PathAndQuery);
                _activePageHtml = await loginGet;
                step = 2;
            }
            else
            {
                _activePageHtml = await _client.ToHtml(Task.FromResult(response));
                step = 1;
            }

        }

        private async Task<Uri> firstStep(Account account)
        {
            var relativeURl = $"/users/{_token.AccountId}/telephone_number_claim";
            var formContext = FormHelper.FindFormContext(_activePageHtml, relativeURl);

            var response = await _client.PostAsync(formContext.Url, formContext.AuthToken, firstStepFormContent(account));
            Console.WriteLine("On the Recertification Page(1)");
            if (response.StatusCode != System.Net.HttpStatusCode.Found)
                throw new Exception("Expected a redirect to the second recertification page");
            Console.WriteLine("Redirect", response.Headers.Location);
            return response.Headers.Location;
        }

        private async Task secondStep(Uri url)
        {
            var nv = System.Web.HttpUtility.ParseQueryString(url.Query);
            _activePageHtml = await _client.GetHtmlAsync(url.PathAndQuery);
        }


        public async Task IssueRecertification(Account account)
        {
            if (step == 0)
                throw new Exception("cant resolve the current step");

            if (step == 1)
            {
                _redirectedUri = await firstStep(account);
                await secondStep(_redirectedUri);
                step = 2;
            }
            if (step == 2)
            {
                var nv = System.Web.HttpUtility.ParseQueryString(_redirectedUri.Query);
                var claimId = nv["telephone_number_claim_id"];
                var formUrl = $"/users/{_token.AccountId}/telephone_number_verification";
                var formContext = FormHelper.FindFormContext(_activePageHtml, formUrl);

                var response = await _client.PostAsync(formContext.Url, formContext.AuthToken, secondStepFormContent(claimId));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("Got a 200 (expected 302) result on ReauthentficationResult");
                    return;
                }

                if (response.StatusCode == HttpStatusCode.Found)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Got a 302 result on ReauthentficationResult.");
                    Console.WriteLine(" that seems fine should have gotten a sms (soon) with code");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    return;
                }
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Something failed on reauthentification attempt got a " + response.StatusCode);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }


        private static IEnumerable<Tuple<string, string>> firstStepFormContent(Account account)
        {
            yield return Tuple.Create("telephone_number_claim[telephone_number_input]", account.Phone);
            yield return Tuple.Create("telephone_number_claim[telephone_number]", "+49" + account.Phone);
        }

        private static IEnumerable<Tuple<string, string>> secondStepFormContent(string claimId)
        {
            yield return Tuple.Create("telephone_number_verification[verification_method]", "sms");
            yield return Tuple.Create("telephone_number_verification[telephone_number_claim_id]", claimId);

        }

    }
}
