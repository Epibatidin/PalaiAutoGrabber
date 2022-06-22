using System;
using System.Threading.Tasks;

using HtmlAgilityPack;

namespace PalaiAutoGrabber.Pages
{
    public class DashboardPage
    {
        private readonly AuthToken _token;
        private readonly IPalaiClient _client;
        private HtmlDocument _activePageHtml;
        private string _relativeUrl;

        public DashboardPage(AuthToken token, IPalaiClient client)
        {
            _token = token;
            _client = client;
            _relativeUrl = $"/users/{token.AccountId}/account/dashboard";
        }

        public async Task Navigate()
        {
            Console.WriteLine("Getting Dashboard Page");

            var page = _client.GetHtmlAsync(_relativeUrl);
            _activePageHtml = await page;
        }

        public RecertificationPage RequiresRecertification()
        {
            var links = _activePageHtml.DocumentNode.SelectNodes("//a");
            var userId = $"/users/{_token.AccountId}/telephone_number_claim";
            string url = null;
            foreach (var link in links)
            {
                var href = link.Attributes["href"];
                if (href.Value.IndexOf(userId, StringComparison.InvariantCultureIgnoreCase) == -1)
                    continue;

                url = href.Value;
                break;
            }

            if (string.IsNullOrEmpty(url))
                return null;

            return new RecertificationPage(url, _token, _client);
        }
        public float getCashAmountFromDashBoard()
        {
            int start = 0;          

            while (true)
            {
                start = _activePageHtml.ParsedText.IndexOf("¶", start + 1);
                if (start == -1)
                    break;

                var end = _activePageHtml.ParsedText.LastIndexOf('>', start);

                var possiblyAFloat = _activePageHtml.ParsedText.Substring(end + 1, start - end - 2).Trim();
                if (float.TryParse(possiblyAFloat, out float afloat))
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
