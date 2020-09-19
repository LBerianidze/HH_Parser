using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ConsoleApp2
{
    public class VacanciesParser
    {
        private WebHelper WebHelper;
        private readonly string ParseLink;
        private string StartParseLink;
        public List<VacancySearch> Pages = new List<VacancySearch>();
        public int AllCount = 0;
        public VacanciesParser(string ParseLink)
        {
            this.ParseLink = ParseLink;
            WebHelper = new WebHelper();
        }
        public VacanciesParser(string ParseLink, string StartLink) : this(ParseLink)
        {
            StartParseLink = StartLink;
        }
        public void Parse(DateTime dt)
        {
            string JsonSource = WebHelper.GetHttpSourcePage(ParseLink);
            VacancySearch FirstPage = JsonConvert.DeserializeObject<VacancySearch>(JsonSource);
            Pages.Add(FirstPage);
            AllCount = FirstPage.VacanciesCount;
            if (FirstPage.VacanciesCount <= 2000)
            {
                for (int i = 1; i < FirstPage.PagesCount; i++)
                {
                    VacancySearch otherpage = JsonConvert.DeserializeObject<VacancySearch>(WebHelper.GetHttpSourcePage(string.Format("{0}&page={1}", ParseLink, i)));
                    Pages.Add(otherpage);
                }
            }
            else
            {
                if (ParseLink.Contains("date_from"))
                {
                    string dateatstart = dt.ToString("yyyy-MM-dd");
                    DateTime datetime = dt;
                    datetime = datetime.Subtract(new TimeSpan(1, 0, 0));
                    while (true)
                    {
                        VacanciesParser parser = new VacanciesParser(string.Format("{0}&date_from={1}&date_to={2}", StartParseLink, datetime.Subtract(new TimeSpan(0, 30, 0)).ToString("yyyy-MM-ddTHH:mm:ss"), datetime.ToString("yyyy-MM-ddTHH:mm:ss")), ParseLink);
                        parser.Parse(datetime);
                        if (parser.Pages.Count == 0)
                            break;
                        Pages.AddRange(parser.Pages);
                        datetime = datetime.Subtract(new TimeSpan(0, 30, 0));
                        if (datetime.ToString("yyyy-MM-dd") != dateatstart)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    DateTime datetime = DateTime.Now;
                    bool TodayParse = true;
                    while (true)
                    {
                        VacanciesParser parser = new VacanciesParser(string.Format("{0}&date_from={1}&date_to={2}", ParseLink, datetime.ToString("yyyy-MM-dd"), datetime.ToString("yyyy-MM-dd")), ParseLink);
                        parser.Parse(new DateTime(datetime.Year, datetime.Month, datetime.Day, datetime.Hour, datetime.Minute, 0));
                        if (parser.Pages[0].VacanciesCount == 0)
                            break;
                        Pages.AddRange(parser.Pages);
                        if (TodayParse)
                        {
                            TodayParse = false;
                            datetime = new DateTime(datetime.Year, datetime.Month, datetime.Day,23,59,55);
                        }
                        Console.WriteLine("Parsed day: " + datetime.ToString("yyyy-MM-dd"));
                        datetime = datetime.Subtract(new TimeSpan(1, 0, 0, 0));
                    }
                }
            }
        }
    }
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
    public class Vacancy
    {
        [JsonProperty("id")]
        public int ID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("employer")]
        public Employer Employer { get; set; }
        [JsonProperty("contacts")]
        public Contact Contact { get; set; }
        [JsonProperty("alternate_url")]
        public string VacancyUrl { get; set; }
        [JsonProperty("salary")]
        public Salary Salary { get; set; }
    }
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
    }
    public class Contact
    {
        public string name { get; set; }
        public string email { get; set; }
        public List<Phone> phones { get; set; }
    }
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
