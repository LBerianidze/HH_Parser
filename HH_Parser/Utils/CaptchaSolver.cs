using System.Threading;

namespace HH_Parser
{
    public class CaptchaSolver
    {
        private readonly OpenQA.Selenium.Chrome.ChromeDriver chrome;
        private bool Solving;
        public CaptchaSolver()
        {
            var options = new OpenQA.Selenium.Chrome.ChromeOptions();
            options.AddExtension("captchasolver.crx");
            this.chrome = new OpenQA.Selenium.Chrome.ChromeDriver(options);
        }
        public void SolveCaptchaForUrl(string URL)
        {
            if (this.Solving)
            {
                return;
            }

            this.Solving = true;
            this.chrome.Url = URL;
            if (this.chrome.PageSource.Contains("g-recaptcha"))
            {
                while (this.chrome.PageSource.Contains("g-recaptcha"))
                {
                    Thread.Sleep(1000);
                }
            }
            this.Solving = false;
        }
    }
}
