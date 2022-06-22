using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PalaiAutoGrabber
{
    public class ResponseHelper
    {
        public async Task<HtmlDocument> ResponseToHtmlAsync(Task<HttpResponseMessage> responseMessage)
        {
            var data = await ResponseToStringAsync(responseMessage);
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(data);
            return htmlDoc;
        }

        public async Task<string> ResponseToStringAsync(Task<HttpResponseMessage> responseMessage)
        {
            var response = await responseMessage;
            var data = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception(data);

            return data;
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
