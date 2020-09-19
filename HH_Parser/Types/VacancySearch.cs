using Newtonsoft.Json;
using System.Collections.Generic;

namespace HH_Parser
{
    public class VacancySearch
    {
        [JsonProperty("found")]
        public int VacanciesCount { get; set; }
        [JsonProperty("per_page")]
        public int ItemsOnPage { get; set; }
        [JsonProperty("pages")]
        public int PagesCount { get; set; }
        [JsonProperty("page")]
        public int CurrenctPage { get; set; }
        [JsonProperty("Items")]
        public List<Vacancy> Vacancies { get; set; }
    }
}
