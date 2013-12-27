using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;

namespace ConsoleApplication2
{
    public class VkApiClient
    {
        private readonly string appId;
        private readonly string permissions;
        private readonly string apiVersion;
        private readonly string phoneNumber;
        private readonly string password;
        private const string redirectUri = "";
        private const string display = "page";
        private readonly CookieAwareWebClient webClient;

        private string accessToken;

        public VkApiClient(string appId, string permissions, string apiVersion, string phoneNumber, string password)
        {
            this.appId = appId;
            this.permissions = permissions;
            this.apiVersion = apiVersion;
            this.phoneNumber = phoneNumber;
            this.password = password;

            this.webClient = new CookieAwareWebClient();
            webClient.Encoding = Encoding.UTF8;
        }

        public string ExecuteMethod(string methodName, NameValueCollection parameters)
        {
            if (string.IsNullOrEmpty(this.accessToken))
            {
                this.Authorize();
            }

            parameters.Add("access_token", this.accessToken);
            string url = string.Format("https://api.vk.com/method/{0}{1}", methodName, ToQueryString(parameters));

            return webClient.DownloadString(url);
        }

        private void Authorize()
        {
            string authUrl = string.Format(
                "https://oauth.vk.com/authorize?client_id={0}&scope={1}&redirect_uri={2}&display={3}&v={4}&response_type=token",
                HttpUtility.UrlEncode(appId),
                HttpUtility.UrlEncode(permissions),
                HttpUtility.UrlEncode(redirectUri),
                HttpUtility.UrlEncode(display),
                HttpUtility.UrlEncode(apiVersion));

            string responseContent = webClient.DownloadString(authUrl);

            Tuple<string, NameValueCollection> formData = this.ParseForm(responseContent);
            string postUrl = formData.Item1;
            NameValueCollection requestParams = formData.Item2;

            requestParams["email"] = this.phoneNumber;
            requestParams["pass"] = this.password;
            requestParams.Add("expire", "0");

            byte[] credentialsResponse = webClient.UploadValues(postUrl, "POST", requestParams);
            string credentialsResponseContent = webClient.Encoding.GetString(credentialsResponse);

            if (webClient.ResponseUri.ToString().Contains("security_check"))
            {
                this.DoSecurityCheck(credentialsResponseContent);
            }

            string responseUrl = webClient.ResponseUri.ToString();
            NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(responseUrl.Split('#')[1]);
            this.accessToken = nameValueCollection["access_token"];
        }

        private void DoSecurityCheck(string responseContent)
        {
            Tuple<string, NameValueCollection> formData = this.ParseForm(responseContent);
            string postUrl = formData.Item1;
            NameValueCollection requestParams = formData.Item2;

            Regex cleanRegex = new Regex(@"\D");
            string phoneNumbers = cleanRegex.Replace(this.phoneNumber, "");

            requestParams["code"] = phoneNumbers.Substring(1, 8);

            webClient.UploadValues(postUrl, "POST", requestParams);
        }

        private Tuple<string, NameValueCollection> ParseForm(string downloadString)
        {
            NameValueCollection requestParams = new NameValueCollection();
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(downloadString);

            HtmlNode form = htmlDocument.DocumentNode.SelectSingleNode("//form");
            string postUrl = GetHtmlAttributeIgnoreCase(form, "action");

            HtmlNodeCollection inputs = form.SelectNodes("//input");
            requestParams = new NameValueCollection();

            foreach (var inputNode in inputs)
            {
                var type = GetHtmlAttributeIgnoreCase(inputNode, "type");

                if (type == "submit")
                {
                    continue;
                }

                var name = GetHtmlAttributeIgnoreCase(inputNode, "name");
                requestParams.Add(name, this.GetHtmlAttributeIgnoreCase(inputNode, "value"));
            }

            return new Tuple<string, NameValueCollection>(postUrl, requestParams);
        }

        private string GetHtmlAttributeIgnoreCase(HtmlNode inputNode, string attrName)
        {
            HtmlAttribute attribute = inputNode.Attributes.FirstOrDefault(a => a.Name.Equals(attrName, StringComparison.InvariantCultureIgnoreCase));

            if (attribute == null)
            {
                return string.Empty;
            }

            return attribute.Value;
        }

        private string ToQueryString(NameValueCollection nameValueCollection)
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