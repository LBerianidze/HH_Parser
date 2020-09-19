namespace HH_Parser
{
    public class Phone
    {
        public string city { get; set; }
        public string number { get; set; }
        public string country { get; set; }
        public string LoadedPhone { get; set; }
        public string GetPhoneNumber()
        {
            if (!string.IsNullOrEmpty(LoadedPhone))
            {
                return LoadedPhone;
            }
            return string.Format("+{0}{1}{2}", country, city, number).Replace("-", "");
        }
    }
}
