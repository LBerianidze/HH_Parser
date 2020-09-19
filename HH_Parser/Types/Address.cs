using Newtonsoft.Json;

namespace HH_Parser
{
    public class Address
    {
        [JsonProperty("city")]
        public string City { get; set; }
    }
}
