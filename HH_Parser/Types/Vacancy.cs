using Newtonsoft.Json;
using System.Linq;

namespace HH_Parser
{
    public class Vacancy
    {
        [JsonProperty("id")]
        public int ID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("address")]
        public Address Address { get; set; }
        [JsonProperty("employer")]
        public Employer Employer { get; set; }
        [JsonProperty("contacts")]
        public Contact Contact { get; set; }
        [JsonProperty("alternate_url")]
        public string VacancyUrl { get; set; }
        [JsonProperty("salary")]
        public Salary Salary { get; set; }
        public string Phones
        {
            get
            {
                if (Contact == null)
                {
                    return "";
                }

                if (Contact.phones == null)
                {
                    return "";
                }

                return string.Join(",", Contact.phones.Select(t => t.GetPhoneNumber()));
            }
        }
    }
}
