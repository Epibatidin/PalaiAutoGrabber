using System;
using System.Linq;
using System.Threading.Tasks;

using HtmlAgilityPack;

namespace PalaiAutoGrabber.Pages
{
    public class DashboardPage
    {
        private readonly AuthToken _token;
        private readonly IPalaiClient _client;
        private HtmlNode _activeContentArea;
        private string _relativeUrl;

        private class RecertificationPageSelector
        {
            private readonly string _url;

            public RecertificationPageSelector(string url)
            {
                _url = url;
            }

            

            public bool Filter(HtmlNode node)
            {
                if (node.NodeType != HtmlNodeType.Element) return false;
                if (node.Name != "a") return false;

                if (!node.HasAttributes) return false;
                var href = node.Attributes["href"];
                if (string.IsNullOrWhiteSpace(href.Value)) return false;

                var isMatch = href.Value.IndexOf(_url, StringComparison.InvariantCultureIgnoreCase) != -1;
                return isMatch;
            }

            public string GetUrl(HtmlNode node)
            {
                return node.Attributes["href"].Value;
            }
        }


        public DashboardPage(AuthToken token, IPalaiClient client)
        {
            _token = token;
            _client = client;
            _relativeUrl = $"/users/{token.AccountId}/account/dashboard";
        }

        public async Task Navigate()
        {
            Console.WriteLine("Getting Dashboard Page");

            var page = await _client.GetHtmlAsync(_relativeUrl);
            _activeContentArea = page.DocumentNode.SelectSingleNode("//article");
            if (_activeContentArea == null)
                throw new Exception("Dashboard Content Area not found !");
        }

        public RecertificationPage RequiresRecertification()
        {
            var selector = new RecertificationPageSelector($"/users/{_token.AccountId}/telephone_number_claim");

            var node = _activeContentArea.Descendants().Where(selector.Filter).FirstOrDefault();
            if (node == null) return null;
            
            var url = selector.GetUrl(node);
            return new RecertificationPage(url, _token, _client);
        }
        public float getCashAmountFromDashBoard()
        {
            int start = 0;

            var text = _activeContentArea.InnerHtml;

            while (true)
            {
                start = text.IndexOf("¶", start + 1);
                if (start == -1)
                    break;

                var end = text.LastIndexOf('>', start);

                var possiblyAFloat = text.Substring(end + 1, start - end - 2).Trim();
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
