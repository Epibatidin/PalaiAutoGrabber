using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace PalaiAutoGrabber
{
    public class FormContext
    {
        public IEnumerable<Tuple<string, string>> AuthToken;

        public HtmlNode Form;
        public string Url;
    }

    public class FormNotFoundException : Exception
    {
        public FormNotFoundException(string message) : base(message)
        {

        }
    }


    public static class FormHelper
    {
        private static string authTokenName = "authenticity_token";
        
        public static HtmlNode FindFormByPostUrl(HtmlDocument htmlDoc, string postTarget)
        {
            var formNodes = htmlDoc.DocumentNode.SelectNodes("//form");

            foreach (var form in formNodes)
            {
                if (form.Attributes["action"].Value.EndsWith(postTarget))
                    return form;
            }
            return null;
        }
                
        public static FormContext FindFormContext(HtmlDocument htmlDoc, string postTarget)
        {
            var formNode = FindFormByPostUrl(htmlDoc, postTarget);
            if (formNode == null)
                throw new FormNotFoundException("expected to find a authToken form - but there was no postback form on " + postTarget);

            var authNode = formNode.SelectSingleNode(".//input[@name='" + authTokenName + "']");
            if(authNode == null)
                throw new Exception("formElement (" + authTokenName + ") that contain authtoken was not found");
            
            var defaultValue = "notset";
            var authToken = authNode.GetAttributeValue("value", defaultValue);
            if (authToken == defaultValue)
                throw new Exception("the authtoken can not be found on input element");

            var context = new FormContext();

            context.Url = formNode.Attributes["action"].Value;
            context.AuthToken = new[] { Tuple.Create(authTokenName, authToken) };
            context.Form = formNode;
            Console.WriteLine("AuthToken found " + authToken);
            return context;
        }
    }
}
