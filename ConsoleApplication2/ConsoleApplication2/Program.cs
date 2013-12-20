using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using HtmlAgilityPack;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new CookieAwareWebClient();

            client.Encoding = Encoding.UTF8;

            string appId = "4070029";
            string permissions = "groups";
            string redirectUri = "";
            string display = "page";
            string apiVersion = "5.5";

            string authUrl = string.Format("https://oauth.vk.com/authorize?client_id={0}&scope={1}&redirect_uri={2}&display={3}&v={4}&response_type=token", appId, permissions, HttpUtility.UrlEncode(redirectUri), display, HttpUtility.UrlEncode(apiVersion));

            string downloadString = client.DownloadString(authUrl);

            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(downloadString);

            HtmlNode form = htmlDocument.DocumentNode.SelectSingleNode("//form");
            string postUrl = GetHtmlAttributeIgnoreCase(form, "action");

            HtmlNodeCollection inputs = form.SelectNodes("//input");
            NameValueCollection requestParams = new NameValueCollection();

            foreach (var inputNode in inputs)
            {
                var type = GetHtmlAttributeIgnoreCase(inputNode, "type");

                if (type == "submit")
                {
                    continue;
                }

                var name = GetHtmlAttributeIgnoreCase(inputNode, "name");

                if (name == "email")
                {
                    requestParams.Add(name, "+79164556832");
                }
                else if (name == "pass")
                {
                    requestParams.Add(name, "SslDemo:)");
                }
                else
                {
                    requestParams.Add(name, GetHtmlAttributeIgnoreCase(inputNode, "value"));
                }
            }

            requestParams.Add("expire", "0");

            byte[] responsebytes = client.UploadValues(postUrl, "POST", requestParams);
            string responsebody = Encoding.UTF8.GetString(responsebytes);


            Uri responseUri = client.ResponseUri;

            NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(responseUri.ToString().Split('#')[1]);
            string accessToken = nameValueCollection["access_token"];

            NameValueCollection boardParams = new NameValueCollection();
            boardParams.Add("gid", "30621569");
            boardParams.Add("tid", "27884279");
            boardParams.Add("extended", "0");
            boardParams.Add("offset", "0");
            boardParams.Add("count", "100");
            boardParams.Add("access_token", accessToken);


            string getCommentsUrl = string.Format("https://api.vk.com/method/{0}{1}", "board.getComments", ToQueryString(boardParams));
            string boardCommentsJson = client.DownloadString(getCommentsUrl);

            // auth_token........

            Console.WriteLine(responsebody);
        }

        private static string GetHtmlAttributeIgnoreCase(HtmlNode inputNode, string attrName)
        {
            string name = inputNode.Attributes.First(a => a.Name.Equals(attrName, StringComparison.InvariantCultureIgnoreCase)).Value;
            return name;
        }

        private static string ToQueryString(NameValueCollection nameValueCollection)
        {
            List<string> result = new List<string>();

            foreach (string key in nameValueCollection.AllKeys)
            {
                string value = nameValueCollection[key];
                string pair = string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value));
                result.Add(pair);
            }

            return "?" + string.Join("&", result);
        }
    }
}
