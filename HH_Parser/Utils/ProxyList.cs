using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using xNet;

namespace HH_Parser
{
    public class ProxyList
    {
        List<LProxy> Proxies;
        int CurrentProxy = 0;
        public ProxyList(List<string> proxies)
        {
            Proxies = new List<LProxy>();
            foreach (var item in proxies)
            {
                var sp = item.Split(':');
                LProxy proxy = new LProxy();
                proxy.ProxyName = sp[0];
                proxy.ProxyPort = sp[1];
                proxy.UserName = sp[2];
                proxy.Password = sp[3];
                Proxies.Add(proxy);
            }
        }
        public LProxy Next()
        {
            if (CurrentProxy == Proxies.Count)
                CurrentProxy = 0;
            var proxy= Proxies[CurrentProxy];
            CurrentProxy++;
            return proxy;
        }
        public LProxy Current()
        {
            if(CurrentProxy==Proxies.Count)
            {
                return Proxies[CurrentProxy - 1];
            }
            return Proxies[CurrentProxy];
        }
        public int Count
        {
            get
            {
                return Proxies.Count;
            }
        }

        internal LProxy NextAvailable()
        {
            var proxy =  Proxies.FirstOrDefault(t => t.Used == false);
            proxy.Used = true;
            return proxy;
        }
    }
    public class LProxy
    {
        public DateTime? BlockedSince { get; set; }
        public string ProxyName { get; set; }
        public string ProxyPort { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Proxy
        {
            get
            {
                return string.Format("{0}:{1}:{2}:{3}", ProxyName, ProxyPort, UserName, Password);
            }
        }
        public string Cookies { get; set; }
        public bool Used { get; set; }
        private CookieDictionary CookiesContainer { get; set; } = new CookieDictionary();
        HttpRequest YandexHttpRequestSender = new HttpRequest();
        public LProxy()
        {
            YandexHttpRequestSender.Cookies = new CookieDictionary();
            //YandexHttpRequestSender.Proxy = ProxyClient.Parse(ProxyType.Socks5, string.Format("{0}:{1}:{2}:{3}", ProxyName, ProxyPort, UserName, Password));
        }
        public string YandexParse(string url)
        {
            YandexHttpRequestSender.AddHeader(HttpHeader.AcceptLanguage, "ru,en-us;q=0.7,en;q=0.3");
            YandexHttpRequestSender.AddHeader(HttpHeader.AcceptCharset, "Accept-Charset: utf-8;q=0.7,*;q=0.7");
            YandexHttpRequestSender.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            YandexHttpRequestSender.UserAgent = Http.FirefoxUserAgent();
            YandexHttpRequestSender.Referer = System.Web.HttpUtility.HtmlEncode(url);
            HttpResponse resp = YandexHttpRequestSender.Get(url);
            StreamReader streamReader = new StreamReader(resp.ToMemoryStream(), Encoding.GetEncoding("UTF-8"));
            string PageSource = streamReader.ReadToEnd();
            if (PageSource.Contains("/checkcaptcha"))
            {
                HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
                document.LoadHtml(PageSource);
                string key = document.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div[2]/div[2]/form/input[1]").Attributes[3].Value;
                string Imageurl = document.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div[2]/div[2]/form/img").Attributes[1].Value;
                string retpath = document.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div[2]/div[2]/form/input[2]").Attributes[3].Value;
                string captchakey = "";
                var api = new Anticaptcha.Api.ImageToText
                {
                    ClientKey = "b2ca53ced2c530f5848f71235a77ce48",
                    URL = Imageurl,
                    Phrase = true,
                    Case = true
                };
                if (!api.CreateTask())
                {

                }
                else if (!api.WaitForResult())
                {
                }
                else
                {
                    var solution = api.GetTaskSolution();
                    captchakey = solution.Text;
                }
                YandexHttpRequestSender.AddUrlParam("key", System.Web.HttpUtility.UrlEncode(key.Replace("&amp", "")));
                YandexHttpRequestSender.AddUrlParam("retpath", System.Web.HttpUtility.UrlEncode(retpath.Replace("&amp", "")));
                YandexHttpRequestSender.AddUrlParam("rep", System.Web.HttpUtility.UrlEncode(captchakey));
                resp = YandexHttpRequestSender.Get("https://www.yandex.ru/checkcaptcha");
                streamReader = new StreamReader(resp.ToMemoryStream(), Encoding.GetEncoding("UTF-8"));
                PageSource = streamReader.ReadToEnd();
                CookiesContainer = resp.Cookies;
            }
            return PageSource;
        }
    }
}
