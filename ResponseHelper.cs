using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using static PalaiAutoGrabber.Program;

namespace PalaiAutoGrabber
{
    public class ResponseHelper
    {
        public const string PalaiBaseUrl = "https://palai.org/de";       
        
        public HtmlDocument ResponseToHtml(Task<HttpResponseMessage> response)
        {
            return ResponseToHtmlAsync(response.ConfigureAwait(false).GetAwaiter().GetResult())
                .ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public HtmlDocument ResponseToHtml(HttpResponseMessage response)
        {
            return Await(ResponseToHtmlAsync(response));
        }

        private async static Task<HtmlDocument> ResponseToHtmlAsync(HttpResponseMessage response)
        {
            var data = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception(data);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(data);
            return htmlDoc;
        }

        /*
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
                    Console.WriteLine("Cookie Found : " + newCookie);
                    return newCookie;
                }
            }

            throw new Exception("Expected to find a Auth Cookie but mh there was nothing");
        }
        */
    }
}
