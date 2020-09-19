using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using xNet;

namespace ConsoleApp2
{
    internal class Program
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        private static void f()
        {
            using (var request = new HttpRequest())
            {
                request.UserAgent = xNet.Http.ChromeUserAgent();
                request.Cookies = new CookieDictionary();
                request.EnableEncodingContent = true;
                request.AllowAutoRedirect = true;

                var response = request.Post("https://zachestnyibiznes.ru/company/balance?okpo=47620187&inn=7708101030&page=");
                var content = response.ToString();
            }
        }
        private static string GetHttpSourcePage(string URL)
        {
            try
            {
                using (var response = _httpClient.GetAsync(URL).Result)
                {
                    using (var content = response.Content)
                    {
                        return content.ReadAsStringAsync().Result;
                    }
                }
            }
            catch
            {
                return GetHttpSourcePage(URL);
            }
        }

        private const string VacanciesURL = "https://api.hh.ru/vacancies?per_page=100&text=Менеджер&area=1";
        private static void Main(string[] args)
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            var parser = new VacanciesParser(VacanciesURL);
            parser.Parse(DateTime.Now);
            var AllVacancies = parser.Pages.SelectMany(t => t.Vacancies).GroupBy(t => t.ID).Select(t => t.FirstOrDefault()).GroupBy(t => t.Employer.ID).Select(t => t.First()).ToList();
            AllVacancies.RemoveAll(t => t.Employer.ID == 0);

            WebHelper wb = new WebHelper();
            foreach (var item in AllVacancies)
            {
               var source= wb.GetHttpSourcePage("https://api.hh.ru/employers/" + item.Employer.ID);
                Newtonsoft.Json.Linq.JObject obj =  Newtonsoft.Json.Linq.JObject.Parse(source);
                item.Employer.WebsiteURL= obj["site_url"].ToString();
                Console.WriteLine(AllVacancies.IndexOf(item));
            }
            st.Stop();
            Console.WriteLine(st.Elapsed.TotalSeconds);
            Console.ReadLine();
        }

    }
}
