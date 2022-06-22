using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using HtmlAgilityPack;

namespace PalaiAutoGrabber
{

    public class AuthToken
    {
        public int AccountId { get; set; }
    }

    public class PalaiClient : IPalaiClient
    {
        private ResponseHelper _responseHelper;
        private HttpClient _client;
        public Uri PalaiBaseUrl;

        public PalaiClient(ResponseHelper responseHelper)
        {
            _responseHelper = responseHelper;
            PalaiBaseUrl = new Uri("https://palai.org/", UriKind.Absolute);
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            httpClientHandler.AllowAutoRedirect = false;
            httpClientHandler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;

            _client = new HttpClient(httpClientHandler);
        }

        private Uri doUrlThing(string url)
        {
            var uri = new Uri(PalaiBaseUrl, url);
            var uriBuilder = new UriBuilder(uri);

            var path = uriBuilder.Path;
            var position = path.IndexOf('/', 1);
            if (position == 3)
                return uri;

            uriBuilder.Path = "/de" + path;
            return uriBuilder.Uri;
        }

        private Uri makeAbsoluteUrl(string url)
        {
            var uri = doUrlThing(url);
            return uri;
        }

        public async Task<HtmlDocument> GetHtmlAsync(string url)
        {
            var loginGet = _client.GetAsync(makeAbsoluteUrl(url));
            var htmlDoc = await _responseHelper.ResponseToHtmlAsync(loginGet);
            return htmlDoc;
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            var loginGet = await _client.GetAsync(makeAbsoluteUrl(url));
            return loginGet;
        }

        private StringContent FillForm(params IEnumerable<Tuple<string, string>>[] values)
        {
            StringBuilder postBackData = new StringBuilder();
            foreach (var setOfValues in values)
            {
                foreach (var kv in setOfValues)
                {
                    postBackData.Append(WebUtility.UrlEncode(kv.Item1)).Append('=').Append(WebUtility.UrlEncode(kv.Item2)).Append('&');
                }
            }
            postBackData.Length--;

            var content = new StringContent(postBackData.ToString(), Encoding.UTF8, "application/x-www-form-urlencoded");
            return content;
        }

        public async Task<HttpResponseMessage> PostAsync(string url, params IEnumerable<Tuple<string, string>>[] values)
        {
            var formPayload = FillForm(values);

            return await _client.PostAsync(makeAbsoluteUrl(url), formPayload);
        }

        public async Task<HtmlDocument> ToHtml(Task<HttpResponseMessage> message)
        {
            return await _responseHelper.ResponseToHtmlAsync(message);
        }
    }
}
