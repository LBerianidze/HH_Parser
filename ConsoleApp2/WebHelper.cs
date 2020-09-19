using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    public class WebHelper
    {
        public WebHelper()
        {
            _httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
        }
        private  HttpClient _httpClient = new HttpClient();
        public string GetHttpSourcePage(string URL)
        {
            try
            {
                using (HttpResponseMessage response = _httpClient.GetAsync(URL).Result)
                {
                    using (HttpContent content = response.Content)
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
    }
}
