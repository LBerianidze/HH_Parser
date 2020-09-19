using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using xNet;

namespace HH_Parser
{
    public class OgrnParser
    {
        private MultiThreadedList<OgrnCompany> _companies;
        private HttpClient _client = new HttpClient();
        private HttpRequest _request = new HttpRequest();
        private ProxyList _proxies;
        private readonly Random _r = new Random();
        private ProgressBar _row;
        private readonly int _processIndex;
        public OgrnParser(MultiThreadedList<OgrnCompany> companies, ProxyList proxies, ProgressBar datagridview)
        {
            _companies = companies;
            _proxies = proxies;
            _client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
            _request.UserAgent = xNet.Http.ChromeUserAgent();
            _request.Cookies = new CookieDictionary();
            _request.EnableEncodingContent = true;
            _request.AllowAutoRedirect = true;
            _row = datagridview;

        }

        private OpenQA.Selenium.IWebDriver driver;

        private string SGetSource(string url)
        {
            try
            {
                driver.Url = url;
                return driver.PageSource;
            }
            catch
            {
                return SGetSource(url);
            }
        }
        public void Start()
        {
            var nextproxy = _proxies.NextAvailable();
            var opt = new ChromeOptions();
            opt.AddExtension(Application.StartupPath + "\\captchasolver.crx");
            opt.AddArgument($"--proxy-server={nextproxy.ProxyName}:{nextproxy.ProxyPort}");
            var service = ChromeDriverService.CreateDefaultService();
            driver = new ChromeDriver(service, opt);
            try
            {
                Thread.Sleep(2500);
                Task.Run(() =>
                {
                    try
                    {
                        driver.Url = "https://google.com";
                    }
                    catch
                    {

                    }
                });
                Thread.Sleep(2500);
                throw new Exception();
            }
            catch
            {

            }
            AutoIt.AutoItX.WinWait("data:, - Google Chrome", "", 5);
            AutoIt.AutoItX.WinActivate("data:, - Google Chrome");
            AutoIt.AutoItX.Send(nextproxy.UserName+"{TAB}");
            AutoIt.AutoItX.Send(nextproxy.Password+"{Enter}");
            while (true)
            {
                var item = _companies.GetNextElement();
                if (item == null)
                {
                    break;
                }
                if (string.IsNullOrEmpty(item.WebSiteUrl))
                {
                    _row.BeginInvoke(new ThreadStart(() => { _row.Value++; }));
                    continue;
                }
                var searchLink = $"https://www.google.ru/search?num=100&start=0&q={item.WebSiteUrl} ОГРН";
                searchLink = $"https://www.yandex.ru/search?text={item.WebSiteUrl} ОГРН";
                var searchSource = SGetSource(searchLink);
                if (searchSource.Contains("/checkcaptcha"))
                {
                    while (string.IsNullOrEmpty(driver.FindElement(OpenQA.Selenium.By.Id("rep")).GetAttribute("value")))
                    {
                        Thread.Sleep(250);
                    }
                    string val = driver.FindElement(OpenQA.Selenium.By.Id("rep")).GetAttribute("value").Trim();
                    driver.FindElement(OpenQA.Selenium.By.XPath("//*[@id=\"rep\"]")).Clear();
                    driver.FindElement(OpenQA.Selenium.By.XPath("//*[@id=\"rep\"]")).SendKeys(val);
                    Thread.Sleep(1000);
                    driver.FindElement(OpenQA.Selenium.By.XPath("/html/body/div[2]/div/div[2]/div[2]/form/button")).Click();
                    Thread.Sleep(3000);
                }
                var regex = new Regex(@"<em>ОГРН<\/em>\D{1,2}(\d*)");
                var regex1 = new Regex(@"<b>ОГРН<\/b>\D{1,2}(\d*)");
                var matches = regex.Matches(searchSource);
                var matches1 = regex1.Matches(searchSource);
                var all = matches.Cast<Match>().Where(t => t.Groups.Count == 2).GroupBy(t => t.Groups[1].Value).Select(f => f.FirstOrDefault()?.Groups[1].Value).Where(f => f != null && f.Length == 13).ToList();
                all.AddRange(matches1.Cast<Match>().Where(t => t.Groups.Count == 2).GroupBy(t => t.Groups[1].Value).Select(f => f.FirstOrDefault().Groups[1].Value).Where(f => f.Length == 13).ToList());
                if (all.Count != 0)
                {
                    item.Ogrn = all[0];
                    if (all.Count > 1)
                    {
                        item.Status = "Было найдено несколько ОГРН: " + string.Join(",", all);
                    }
                    Task.Run(() =>
                    {
                        var itemSource = GetSource($"https://zachestnyibiznes.ru/company/ul/{item.Ogrn}");
                        var document = new HtmlAgilityPack.HtmlDocument();
                        document.LoadHtml(itemSource);
                        var averageNode = document.DocumentNode.FindElementByTagNameAndInnerText("#text", "\nСреднесписочная численность работников:&nbsp;");
                        if (averageNode != null)
                        {
                            var index = averageNode.ParentNode.ChildNodes.IndexOf(averageNode) + 3;
                            item.PersonalAverage = averageNode.ParentNode.ChildNodes[index].ChildNodes[1].InnerText;
                        }
                        var registrationDate = document.DocumentNode.FindElementByTagNameAndInnerText("#text", "\nДата регистрации:&nbsp;");
                        if (registrationDate != null)
                        {
                            var index = registrationDate.ParentNode.ChildNodes.IndexOf(registrationDate) + 4;
                            item.RegistrationDate = registrationDate.ParentNode.ChildNodes[index].ChildNodes[1].InnerText;
                        }
                        var courtNode = document.DocumentNode.FindElementByTagNameAndInnerText("#text", "Ответчик: ");
                        if (courtNode != null)
                        {
                            item.CasesCount = courtNode.ParentNode.ChildNodes[2].InnerText;
                        }
                        var innNode = document.DocumentNode.FindElementByTagNameAndInnerText("td", "\nИНН&nbsp;\n?\n");
                        if (innNode != null)
                        {
                            item.Inn = innNode.ParentNode.ChildNodes[3].ChildNodes[3].InnerText;
                        }
                        var okpoNode = document.DocumentNode.FindElementByTagNameAndInnerText("td", "\nОКПО&nbsp;\n?\n");
                        if (okpoNode != null)
                        {
                            item.Okpo = okpoNode.ParentNode.ChildNodes[3].ChildNodes[3].InnerText;
                        }
                        if (!string.IsNullOrEmpty(item.Inn) && !string.IsNullOrEmpty(item.Okpo))
                        {
                            itemSource = XNetPost($"https://zachestnyibiznes.ru/company/balance?okpo={item.Okpo}&inn={item.Inn}&page=");
                            document = new HtmlAgilityPack.HtmlDocument();
                            document.LoadHtml(itemSource);
                            var revenueNode = document.DocumentNode.FindElementByTagNameAndInnerText("td", "Выручка");
                            if (revenueNode != null)
                            {
                                item.Revenue = revenueNode.ParentNode.ChildNodes[3].InnerText;
                            }
                        }
                    });
                }
                _row.BeginInvoke(new ThreadStart(() => { _row.Value++; }));

            }
            driver.Dispose();
            driver.Quit();
        }
        private string XNetPost(string url)
        {
            var response = _request.Post(url);
            return response.ToString();
        }
        public string GetSource(string url)
        {
            using (var response = _client.GetAsync(url).Result)
            {
                using (var content = response.Content)
                {
                    return content.ReadAsStringAsync().Result;
                }
            }
        }

        private void SolveYandexCaptcha(LProxy proxy, string key, string cvalue, string retpath, string uri)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create($"https://www.yandex.ru/checkcaptcha?key={System.Web.HttpUtility.UrlEncode(key)}&retpath={System.Web.HttpUtility.UrlEncode(retpath)}&rep={System.Web.HttpUtility.UrlEncode(cvalue)}");
            //WebProxy webproxy = new WebProxy();
            //webproxy.Credentials = new NetworkCredential(proxy.UserName, proxy.Password);
            //webproxy.Address = new Uri(string.Format("http://{0}:{1}", proxy.ProxyName, proxy.ProxyPort));
            httpWebRequest.Method = "GET";
            httpWebRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "ru,en-us;q=0.7,en;q=0.3");
            httpWebRequest.Headers.Add(HttpRequestHeader.AcceptCharset, "Accept-Charset: utf-8;q=0.7,*;q=0.7");
            httpWebRequest.Headers.Add(HttpRequestHeader.CacheControl, "max-age=0");
            //httpWebRequest.Headers.Add(HttpRequestHeader.Cookie, proxy.Cookies);
            httpWebRequest.Headers.Add("upgrade-insecure-requests", "1");
            httpWebRequest.Referer = uri;
            httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36";
            httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            //httpWebRequest.Proxy = webproxy;
            httpWebRequest.Timeout = 5000;
            var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            var cookies = httpWebResponse.GetResponseHeader("Set-Cookie");
            proxy.Cookies = cookies;
            var streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
            object value = streamReader.ReadToEnd();
            streamReader.Close();
            httpWebResponse.Close();
        }

        private string GetSourceWithWebRequest(string url)
        {
            var proxy = _proxies.Next();
            while (true)
            {
                if (!proxy.BlockedSince.HasValue)
                {
                    break;
                }

                if (DateTime.Now.Subtract(proxy.BlockedSince.Value).TotalHours > 1)
                {
                    proxy.BlockedSince = null;
                    break;
                }
                proxy = _proxies.Next();
                break;
            }
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                var webproxy = new WebProxy
                {
                    Credentials = new NetworkCredential(proxy.UserName, proxy.Password),
                    Address = new Uri($"http://{proxy.ProxyName}:{proxy.ProxyPort}")
                };
                httpWebRequest.Method = "GET";
                httpWebRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "ru,en-us;q=0.7,en;q=0.3");
                httpWebRequest.Headers.Add(HttpRequestHeader.AcceptCharset, "Accept-Charset: utf-8;q=0.7,*;q=0.7");
                httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                httpWebRequest.Headers.Add(HttpRequestHeader.CacheControl, "max-age=0");
                httpWebRequest.Headers.Add("upgrade-insecure-requests", "1");
                if (proxy.Cookies != null)
                {
                    httpWebRequest.Headers.Add(HttpRequestHeader.Cookie, proxy.Cookies);
                }

                httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36";
                httpWebRequest.Proxy = webproxy;
                httpWebRequest.Timeout = 5000;
                var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                var cookies = httpWebResponse.GetResponseHeader("Set-Cookie");
                proxy.Cookies = cookies;
                var streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                var value = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebResponse.Close();
                if (value.Contains("/checkcaptcha"))
                {
                    var document = new HtmlAgilityPack.HtmlDocument();
                    document.LoadHtml(value);
                    var key = document.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div[2]/div[2]/form/input[1]").Attributes[3].Value;
                    var imageurl = document.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div[2]/div[2]/form/img").Attributes[1].Value;
                    var retpath = document.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div[2]/div[2]/form/input[2]").Attributes[3].Value;
                    var captchakey = "";
                    var api = new Anticaptcha.Api.ImageToText
                    {
                        ClientKey = "b2ca53ced2c530f5848f71235a77ce48",
                        URL = imageurl
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
                    SolveYandexCaptcha(proxy, key, captchakey, retpath, httpWebResponse.ResponseUri.AbsoluteUri);
                }
                return value;
            }
            catch (Exception)
            {
                proxy.BlockedSince = DateTime.Now;
                return "";
            }
        }

        private string YandexParse(string url)
        {
            var pr = _proxies.Next();
            return pr.YandexParse(url);
            var yandexHttpRequestSender = new HttpRequest();
            yandexHttpRequestSender.AddHeader(HttpHeader.AcceptLanguage, "ru,en-us;q=0.7,en;q=0.3");
            yandexHttpRequestSender.AddHeader(HttpHeader.AcceptCharset, "Accept-Charset: utf-8;q=0.7,*;q=0.7");
            yandexHttpRequestSender.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            yandexHttpRequestSender.AddHeader(HttpHeader.CacheControl, "max-age=0");
            yandexHttpRequestSender.Cookies = _dict;
            yandexHttpRequestSender.UserAgent = Http.FirefoxUserAgent();
            yandexHttpRequestSender.Referer = System.Web.HttpUtility.HtmlEncode(url);
            var resp = yandexHttpRequestSender.Get(url);
            var streamReader = new StreamReader(resp.ToMemoryStream(), Encoding.GetEncoding("UTF-8"));
            var content = streamReader.ReadToEnd();
            if (content.Contains("/checkcaptcha"))
            {
                var document = new HtmlAgilityPack.HtmlDocument();
                document.LoadHtml(content);
                var key = document.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div[2]/div[2]/form/input[1]").Attributes[3].Value;
                var imageurl = document.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div[2]/div[2]/form/img").Attributes[1].Value;
                var retpath = document.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div[2]/div[2]/form/input[2]").Attributes[3].Value;
                var captchakey = "";
                var api = new Anticaptcha.Api.ImageToText
                {
                    ClientKey = "b2ca53ced2c530f5848f71235a77ce48",
                    URL = imageurl,
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
                yandexHttpRequestSender.AddUrlParam("key", System.Web.HttpUtility.UrlEncode(key.Replace("&amp", "")));
                yandexHttpRequestSender.AddUrlParam("retpath", System.Web.HttpUtility.UrlEncode(retpath.Replace("&amp", "")));
                yandexHttpRequestSender.AddUrlParam("rep", System.Web.HttpUtility.UrlEncode(captchakey));
                resp = yandexHttpRequestSender.Get("https://www.yandex.ru/checkcaptcha");
                streamReader = new StreamReader(resp.ToMemoryStream(), Encoding.GetEncoding("UTF-8"));
                content = streamReader.ReadToEnd();
            }
            yandexHttpRequestSender.Close();
            return content;
        }

        private string YandexParseNext(string url)
        {
            //var resp = YandexHttpRequestSender.Get(url);
            //var streamReader = new StreamReader(resp.ToMemoryStream(), Encoding.GetEncoding("UTF-8"));
            //var content = streamReader.ReadToEnd();
            //while (content.Contains("/checkcaptcha"))
            //{
            //    HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            //    document.LoadHtml(content);
            //    string key = document.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div[2]/div[2]/form/input[1]").Attributes[3].Value;
            //    string Imageurl = document.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div[2]/div[2]/form/img").Attributes[1].Value;
            //    string retpath = document.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div[2]/div[2]/form/input[2]").Attributes[3].Value;
            //    string captchakey = "";
            //    var api = new Anticaptcha_example.Api.ImageToText
            //    {
            //        ClientKey = "b2ca53ced2c530f5848f71235a77ce48",
            //        URL = Imageurl,
            //        Phrase = true,
            //        Case = true
            //    };
            //    if (!api.CreateTask())
            //    {

            //    }
            //    else if (!api.WaitForResult())
            //    {
            //    }
            //    else
            //    {
            //        var solution = api.GetTaskSolution();
            //        captchakey = solution.Text;
            //    }
            //    YandexHttpRequestSender.AddUrlParam("key", System.Web.HttpUtility.UrlEncode(key.Replace("&amp", "")));
            //    YandexHttpRequestSender.AddUrlParam("retpath", System.Web.HttpUtility.UrlEncode(retpath.Replace("&amp", "")));
            //    YandexHttpRequestSender.AddUrlParam("rep", System.Web.HttpUtility.UrlEncode(captchakey));
            //    resp = YandexHttpRequestSender.Get("https://www.yandex.ru/checkcaptcha");
            //    streamReader = new StreamReader(resp.ToMemoryStream(), Encoding.GetEncoding("UTF-8"));
            //    content = streamReader.ReadToEnd();
            //    dict = resp.Cookies;
            //}
            //return content;
            return "";
        }

        private readonly CookieDictionary _dict = new CookieDictionary();
    }
    public class Alert : OpenQA.Selenium.IAlert
    {
        public string Text => throw new NotImplementedException();

        public void Dismiss()
        {
            throw new NotImplementedException();
        }

        public void Accept()
        {
            throw new NotImplementedException();
        }

        public void SendKeys(string keysToSend)
        {
            throw new NotImplementedException();
        }

        public void SetAuthenticationCredentials(string userName, string password)
        {
            throw new NotImplementedException();
        }
    }
}
