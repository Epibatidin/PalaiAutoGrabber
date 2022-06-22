using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using HtmlAgilityPack;

namespace PalaiAutoGrabber
{
    public interface IPalaiClient
    {
        Task<HtmlDocument> GetHtmlAsync(string url);
        Task<HttpResponseMessage> GetAsync(string url);

        Task<HttpResponseMessage> PostAsync(string url, params IEnumerable<Tuple<string, string>>[] values);

        Task<HtmlDocument> ToHtml(Task<HttpResponseMessage> message);
    }

    public interface IAuthentificatedPalaiClient
    {
        void GrabTheCash();
        float getCashAmountFromDashBoard();
    }
}