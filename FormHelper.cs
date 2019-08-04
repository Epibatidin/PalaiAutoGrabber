using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace PalaiAutoGrabber
{
    public class FormHelper
    {
        private static string authTokenName = "authenticity_token";
        
        private HtmlNode FindFormByPostUrl(HtmlDocument htmlDoc, string postTarget)
        {
            var formNodes = htmlDoc.DocumentNode.SelectNodes("//form");

            foreach (var form in formNodes)
            {
                if (form.Attributes["action"].Value.EndsWith(postTarget))
                    return form;
            }
            return null;
        }

        public string GetAuthTokenFromForm(HtmlDocument htmlDoc, string postTarget)
        {
            var formNode = FindFormByPostUrl(htmlDoc, postTarget);
            if (formNode == null)
                throw new Exception("expected to be on the login page - but there was no postback form");

            var authNode = formNode.SelectSingleNode(".//input[@name='" + authTokenName + "']");
            var authToken = authNode.GetAttributeValue("value", "notset");
            if (authToken.Length != 88)
                throw new Exception("the authtoken is not 88 chars long");

            return authToken;
        }

        public StringContent FillForm(params IEnumerable<Tuple<string, string>>[] values)
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
        
        public IEnumerable<Tuple<string, string>> AuthValue(string authToken)
        {
            yield return Tuple.Create(authTokenName, authToken);
        }

    }
}
