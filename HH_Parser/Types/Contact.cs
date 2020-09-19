using System.Collections.Generic;

namespace HH_Parser
{
    public class Contact
    {
        public string name { get; set; }
        public string email { get; set; }
        public List<Phone> phones { get; set; } = new List<Phone>();
    }
}
