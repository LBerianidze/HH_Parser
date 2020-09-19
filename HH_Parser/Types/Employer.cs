using Newtonsoft.Json;

namespace HH_Parser
{
    public class Employer
    {
        [JsonProperty("id")]
        public int ID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("url")]
        public string url { get; set; }
        [JsonProperty("alternate_url")]
        public string alternate_url { get; set; }
        [JsonProperty("vacancies_url")]
        public string vacancies_url { get; set; }
        public string WebsiteURL { get; set; }
        public string VacancyCount { get; set; }
    }
}
