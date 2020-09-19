using Gecko;
using Gecko.DOM;
using Gecko.Utils;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataCollector
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Xpcom.Initialize(Application.StartupPath + "\\xulrunner");
            GeckoBrowser.DocumentCompleted += GeckoBrowser_DocumentCompleted;
            Program.BrowserIsReady = true;

        }
        private void GeckoBrowser_DocumentCompleted(object sender, Gecko.Events.GeckoDocumentCompletedEventArgs e)
        {
            //richTextBox1.Text = GeckoBrowser.Document.Body?.OuterHtml;
        }

        string GetState()
        {
            string state = "";
            this.Invoke(new Action(() => state = GeckoBrowser.Document.ReadyState));
            return state;
        }
        public string GetSource()
        {
            try
            {
                string state = "";
                this.Invoke(new Action(() =>
                {
                    File.WriteAllText("Source.txt", GeckoBrowser.Document.Body?.OuterHtml);
                    state = GeckoBrowser.Document.Body?.OuterHtml;
                }));
                return state;
            }
            catch (Exception ex)
            {
                File.WriteAllText("GetSourceError.txt", ex.Message + ex.StackTrace + ex.Source);
                return "";
            }
        }
        async Task WaitWhileCompleted()
        {
            await Task.Run(() =>
            {
                while (GetState().ToLower() != "complete")
                {

                }
                Thread.Sleep(250);
            });

        }
        async Task WaitWhileScriptExecuted()
        {
            await Task.Run(() =>
            {
                while (!scriptexecuting)
                {

                }
                Thread.Sleep(250);
            });

        }
        bool scriptexecuting;
        internal async Task<string> ExecuteJavascript(string script)
        {
            string result="";
            this.BeginInvoke(new Action(() =>
            {
                using (var context = new AutoJSContext(GeckoBrowser.Window.JSContext))
                {
                    if (context.EvaluateScript(@script, (nsISupports)GeckoBrowser.Window.DomWindow, out result))
                    {
                        result = "true";
                    }
                    else
                    {
                        result = "false";
                    }
                }
                scriptexecuting = true;
            }));
            await WaitWhileScriptExecuted();
            scriptexecuting = false;
            return result;
        }


        async Task WaitForPageLoadType(string LoadType)
        {
            await Task.Run(() =>
            {
                while (GetState().ToLower() != "interactive")
                {

                }
                Thread.Sleep(250);
            });
        }
        public string GetImageFromPosition()
        {
            this.Invoke(new ThreadStart(() =>
            {
                GeckoHtmlElement element1 = (GeckoHtmlElement)GeckoBrowser.DomDocument.GetElementsByClassName("image form__captcha")[0];
                var rect = element1.GetBoundingClientRect();
                MessageBox.Show((element1 == null).ToString());
                MessageBox.Show($"{rect.X} {rect.Bottom} {rect.Height} {rect.Left} {rect.Right} {rect.Top}");

                SaveImageElement element = new SaveImageElement();
            Bitmap bmp = new Bitmap(element1.ClientWidth, element1.ClientHeight);
            using (Graphics g = GeckoBrowser.CreateGraphics())
            {
                GeckoBrowser.DrawToBitmap(bmp, new Rectangle(rect.X, rect.Y, rect.Width, rect.Height));
            }
            byte[] ret = null;
            using (var m = new MemoryStream())
            {
                bmp.Save(m, ImageFormat.Png);
                ret = m.ToArray();
            }
            bmp.Save("file.png");
        }));
            //this.Invoke(new ThreadStart(() => { MessageBox.Show(success.ToString()); }));
            return "";
        }
        internal async Task InitializeBrowser(string proxyIP, string proxyPort, string proxyName, string proxyPassword, bool disableimageshowing)
        {
            this.BeginInvoke(new Action(() =>
            {
                nsICookieManager CookieMan;
                CookieMan = Xpcom.GetService<nsICookieManager>("@mozilla.org/cookiemanager;1");
                CookieMan = Xpcom.QueryInterface<nsICookieManager>(CookieMan);
                CookieMan.RemoveAll();
                GeckoPreferences.User["network.proxy.type"] = 1;
                GeckoPreferences.User["network.proxy.http"] = proxyIP;
                GeckoPreferences.User["network.proxy.http_port"] = Convert.ToInt32(proxyPort);
                GeckoPreferences.User["network.proxy.ssl"] = proxyIP;
                GeckoPreferences.User["network.proxy.ssl_port"] = Convert.ToInt32(proxyPort);
                GeckoPreferences.User["general.useragent.override"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.109 Safari/537.36";
                if (!string.IsNullOrEmpty(proxyName))
                {
                    PromptFactory.PromptServiceCreator = () => new PromptProvider(proxyName, proxyPassword);
                }
                if (disableimageshowing)
                    GeckoPreferences.Default["permissions.default.image"] = 2;

                GeckoBrowser.Navigate("http://google.com");
            }));
            await WaitWhileCompleted();
        }
        internal string SolveYandexCaptcha()
        {
            string key;
            string Imageurl="";
            string retpath;
            this.Invoke(new ThreadStart(() =>
            {
                string source = GeckoBrowser.Window.Document.GetElementsByTagName("body")[0].OuterHtml;
                HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
                document.LoadHtml(source);
                key = document.DocumentNode.SelectSingleNode("/body/div[2]/div/div[2]/div[2]/form/input[1]").Attributes[3].Value;
                Imageurl = document.DocumentNode.SelectSingleNode("/body/div[2]/div/div[2]/div[2]/form/img").Attributes[1].Value;
                retpath = document.DocumentNode.SelectSingleNode("/body/div[2]/div/div[2]/div[2]/form/input[2]").Attributes[3].Value;
            }));

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
                this.Invoke(new ThreadStart(() =>
                {
                    var solution = api.GetTaskSolution();
                    GeckoBrowser.Window.Document.GetElementById("rep").SetAttribute("value", solution.Text);
                    using (var context = new AutoJSContext(GeckoBrowser.Window.JSContext))
                    {
                        string result;
                        if (context.EvaluateScript(clicker, (nsISupports)GeckoBrowser.Window.DomWindow, out result))
                        {
                        }
                    }
                }));
            }
            Thread.Sleep(2000);
            return "true";

        }
        public string EnterCaptcha(string Captcha)
        {
            this.Invoke(new ThreadStart(() =>
            {
                GetImageFromPosition();

                GeckoBrowser.Window.Document.GetElementById("rep").SetAttribute("value", Captcha);
                using (var context = new AutoJSContext(GeckoBrowser.Window.JSContext))
                {
                    string result;
                    if (context.EvaluateScript(clicker, (nsISupports)GeckoBrowser.Window.DomWindow, out result))
                    {
                    }

                }
            }));
            return "Entered";
        }
        public async Task<string> GetPageSource(string URL, string LoadStrategy)
        {
            GeckoBrowser.Navigate(URL);
            await WaitForPageLoadType(LoadStrategy);
            try
            {
                string source = "empty";
                this.Invoke(new Action(() =>
                {
                    source = GeckoBrowser.Document.Body.OuterHtml;
                }));
                return source;
            }
            catch (Exception ex)
            {
                return "Error";
            }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //Program.CommandProcessor("{\"RequestType\":0,\"Proxy\":\"186.65.117.133:9462:etCQ67: vFQ17Y\",\"ProxyIP\":\"186.65.117.133\",\"ProxyPort\":\"9462\",\"ProxyName\":\"etCQ67\",\"ProxyPassword\":\"vFQ17Y\",\"UserAgent\":\"\",\"RequestURL\":\"\"}");

        }

        const string clicker =
            @"
              document.evaluate(""/html/body/div[2]/div/div[2]/div[2]/form/button"",document,null,XPathResult.FIRST_ORDERED_NODE_TYPE,null).singleNodeValue.click();
             ";

    }
}
