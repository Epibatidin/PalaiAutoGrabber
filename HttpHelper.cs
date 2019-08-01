using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PalaiAutoGrabber
{
    public class HttpHelper
    {
        public const string PalaiBaseUrl = "https://palai.org/de";

        public StringContent FillForm(params IEnumerable<Tuple<string, string>>[] values)
        {
            StringBuilder postBackData = new StringBuilder();
            foreach (var setOfValues in values)
            {
                foreach (var kv in setOfValues)
                {
                    postBackData.Append(kv.Item1).Append('=').Append(kv.Item1).Append('&');
                }
            }           
            postBackData.Length--;

            var toSend = WebUtility.UrlEncode(postBackData.ToString());

            var content = new StringContent(toSend, Encoding.UTF8, "application/x-www-form-urlencoded");
            return content;
        }

        public static HtmlDocument ResponseToHtml(Task<HttpResponseMessage> response)
        {
            return ResponseToHtmlAsync(response).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private async static Task<HtmlDocument> ResponseToHtmlAsync(Task<HttpResponseMessage> response)
        {
            var cgxh = await response;
            var data = await cgxh.Content.ReadAsStringAsync();

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(data);
            return htmlDoc;
        }


        public static IEnumerable<Tuple<string, string>> AuthValue(AuthToken authToken)
        {
            yield return Tuple.Create("authenticity_token", authToken.Cookie);
        }

        public string ExtractAuthCookie(HttpResponseMessage response)
        {
            var isSetCookie = response.Headers.TryGetValues("Set-Cookie", out var setCookies);
            if (isSetCookie && setCookies != null)
            {
                foreach (var setCookie in setCookies)
                {
                    if (!setCookie.StartsWith("_palai_community_site_session=", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var endIndex = setCookie.IndexOf(';');
                    var newCookie = setCookie.Substring(0, endIndex);
                    return newCookie;
                }
            }

            throw new Exception("Expected to find a Auth Cookie but mh there was nothing");
        }

    }
}
