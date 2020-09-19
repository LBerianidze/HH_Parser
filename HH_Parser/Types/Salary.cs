namespace HH_Parser
{
    public class Salary
    {
        public string from { get; set; }
        public string to { get; set; }
        public string currency { get; set; }
        public string GetSalary()
        {
            if (from != null && to != null && currency != null)
            {
                return string.Format("{0}-{1} {2}", from, to, currency);
            }
            else
            {
                if (from != null && currency != null)
                {
                    return string.Format("{0}+ {1}", from, currency);
                }
                else if (to != null && currency != null)
                {
                    return string.Format("0-{0} {1}", to, currency);
                }
            }
            return null;
        }
    }
}
