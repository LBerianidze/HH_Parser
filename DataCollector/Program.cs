using ParentChildProcessRelation;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataCollector
{
    internal static class Program
    {
        private static Form1 form;
        private static string Guid;
        private static ParentChildProcessRelation.ChildProcess ChildProcess;
        public static async Task<string> CommandProcessor(string Arguments)
        {
            var settings = ParserSettings.Create(Arguments);


            while (form == null)
            {
                Thread.Sleep(250);
            }
            if (settings.RequestType == ParentChildProcessRelation.Type.Initialize)
            {
                await form.InitializeBrowser(settings.ProxyIP, settings.ProxyPort, settings.ProxyName, settings.ProxyPassword,settings.DisableImageDisplay);
                return "Success";
            }
            else if (settings.RequestType == ParentChildProcessRelation.Type.Load)
            {
                string pagesource = await form.GetPageSource(settings.RequestURL,settings.LoadType.ToString());
                return pagesource;
            }
            else if (settings.RequestType==ParentChildProcessRelation.Type.EnterCaptcha)
            {
                string pagesource = form.EnterCaptcha(settings.CaptchaKey);
                return pagesource;
            }
            else if (settings.RequestType == ParentChildProcessRelation.Type.GetSource)
            {
                string pagesource="";
                await Task.Run(()=>{ pagesource=form.GetSource(); });
                return pagesource;
            }
            else if (settings.RequestType == ParentChildProcessRelation.Type.SolveYandexCaptcha)
            {
                return form.SolveYandexCaptcha();
            }
            else if (settings.RequestType==ParentChildProcessRelation.Type.ExecuteScript)
            {
                var result = await form.ExecuteJavascript(settings.Script);
            }
            return "";
        }
        public static bool BrowserIsReady = false;
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length == 0)
            {
                form = new Form1();
                Application.Run(form);
            }
            Guid = args[0];
            ChildProcess = new ParentChildProcessRelation.ChildProcess(Guid, CommandProcessor);
            ChildProcess.Start();
            form = new Form1();
            Application.Run(form);
        }

    }
}
