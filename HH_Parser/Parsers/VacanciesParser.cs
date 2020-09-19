using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HH_Parser
{
    internal class VacanciesParser
    {
        private WebHelper WebHelper;
        private readonly string ParseLink;
        public List<VacancySearch> Pages = new List<VacancySearch>();
        public int AllCount = 0;
        private DataGridViewRow row;
        private bool ParseFinished = false;
        private bool TaskFinished = false;
        public VacanciesParser(string ParseLink, DataGridViewRow row)
        {
            this.ParseLink = ParseLink;
            this.row = row;
            WebHelper = new WebHelper();
        }
        public void StartParse()
        {
            var JsonSource = WebHelper.GetHttpSourcePage(ParseLink);
            var FirstPage = JsonConvert.DeserializeObject<VacancySearch>(JsonSource);
            Pages.Add(FirstPage);
            AllCount = FirstPage.VacanciesCount;
            row.DataGridView.BeginInvoke(new ThreadStart(() => { row.Cells[1].Value = FirstPage.VacanciesCount; }));
            if (FirstPage.VacanciesCount <= 2000)
            {
                for (var i = 1; i < FirstPage.PagesCount; i++)
                {
                    var otherpage = JsonConvert.DeserializeObject<VacancySearch>(WebHelper.GetHttpSourcePage(string.Format("{0}&page={1}", ParseLink, i)));
                    Pages.Add(otherpage);
                }
                TaskFinished = true;
            }
            else
            {
                var datetime = DateTime.Today;
                while (true)
                {
                    var parser = new VacanciesParser(ParseLink, row);
                    parser.ParseDay(new DateTime(datetime.Year, datetime.Month, datetime.Day));
                    if (parser.Pages[0].VacanciesCount == 0)
                    {
                        break;
                    }
                    Pages.AddRange(parser.Pages);
                    datetime = datetime.Subtract(new TimeSpan(1, 0, 0, 0));
                    row.DataGridView.BeginInvoke(new ThreadStart(() => { row.Cells[2].Value = row.Cells[2].Value = Pages.SelectMany(t => t.Vacancies).GroupBy(t => t.ID).Select(t => t.FirstOrDefault()).GroupBy(t => t.Employer.ID).Select(t => t.First()).Count(); row.Cells[3].Value = datetime.ToString("yyyy-MM-dd");  }));
                }
            }
            row.DataGridView.BeginInvoke(new ThreadStart(() => { row.Cells[3].Value = "Готово"; }));
        }
        public void ParseDay(DateTime datetime)
        {
            var link = string.Format("{0}&date_from={1}&date_to={2}", ParseLink, datetime.ToString("yyyy-MM-dd"), datetime.ToString("yyyy-MM-dd"));
            var JsonSource = WebHelper.GetHttpSourcePage(link);
            var Page = JsonConvert.DeserializeObject<VacancySearch>(JsonSource);
            if (Page.VacanciesCount <= 2000)
            {
                Pages.Add(Page);
            }
            else
            {

                for (var i = 0; i < 4; i++)
                {
                    var parser = new VacanciesParser(ParseLink, row);
                    var hour = 6 * (i + 1);
                    var minute = 0;
                    var seconds = 0;
                    if (hour == 24)
                    {
                        hour = 23;
                        minute = 59;
                        seconds = 59;
                    }
                    parser.ParseSixHour(new DateTime(datetime.Year, datetime.Month, datetime.Day, 6 * i, 0, 0), new DateTime(datetime.Year, datetime.Month, datetime.Day, hour, minute, seconds));
                    Pages.AddRange(parser.Pages);

                }

            }
        }
        public void ParseSixHour(DateTime from, DateTime to)
        {
            var link = string.Format("{0}&date_from={1}&date_to={2}", ParseLink, from.ToString("yyyy-MM-ddTHH:mm:ss"), to.ToString("yyyy-MM-ddTHH:mm:ss"));
            var JsonSource = WebHelper.GetHttpSourcePage(link);
            var Page = JsonConvert.DeserializeObject<VacancySearch>(JsonSource);
            if (Page.VacanciesCount <= 2000)
            {
                Pages.Add(Page);
            }
            else
            {
                for (var i = from.Hour; i < to.Hour; i++)
                {
                    var parser = new VacanciesParser(ParseLink, row);
                    int hour = i, minute = 0, seconds = 0;
                    if (hour + 1 == 23)
                    {
                        minute = 59;
                        seconds = 59;
                    }
                    parser.ParseHour(new DateTime(from.Year, from.Month, from.Day, i, 0, 0), new DateTime(from.Year, from.Month, from.Day, hour + 1, minute, seconds));
                    Pages.AddRange(parser.Pages);

                }
            }

        }
        public void ParseHour(DateTime from, DateTime to)
        {
            var link = string.Format("{0}&date_from={1}&date_to={2}", ParseLink, from.ToString("yyyy-MM-ddTHH:mm:ss"), to.ToString("yyyy-MM-ddTHH:mm:ss"));
            var JsonSource = WebHelper.GetHttpSourcePage(link);
            var Page = JsonConvert.DeserializeObject<VacancySearch>(JsonSource);
            if (Page.VacanciesCount <= 2000)
            {
                Pages.Add(Page);
            }
            else
            {
                Pages.Add(Page);
            }
        }
        public void Parse(DateTime dt)
        {
            try
            {
                if (ParseLink.Contains("date_from"))
                {
                    var dateatstart = dt.ToString("yyyy-MM-dd");
                    var datetime = dt;
                    datetime = datetime.Subtract(new TimeSpan(1, 0, 0));
                    while (true)
                    {
                        var parser = new VacanciesParser(string.Format("{0}&date_from={1}&date_to={2}", ParseLink, datetime.Subtract(new TimeSpan(0, 30, 0)).ToString("yyyy-MM-ddTHH:mm:ss"), datetime.ToString("yyyy-MM-ddTHH:mm:ss")), row);
                        parser.Parse(datetime);
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
                    var datetime = DateTime.Now;
                    var TodayParse = true;
                    while (true)
                    {
                        var parser = new VacanciesParser(string.Format("{0}&date_from={1}&date_to={2}", ParseLink, datetime.ToString("yyyy-MM-dd"), datetime.ToString("yyyy-MM-dd")), row);
                        parser.Parse(new DateTime(datetime.Year, datetime.Month, datetime.Day, datetime.Hour, datetime.Minute, 0));
                        if (parser.Pages[0].VacanciesCount == 0)
                        {
                            break;
                        }

                        Pages.AddRange(parser.Pages);
                        if (TodayParse)
                        {
                            TodayParse = false;
                            datetime = new DateTime(datetime.Year, datetime.Month, datetime.Day, 23, 59, 55);
                        }
                        Console.WriteLine("Parsed day: " + datetime.ToString("yyyy-MM-dd"));
                        datetime = datetime.Subtract(new TimeSpan(1, 0, 0, 0));
                        row.DataGridView.BeginInvoke(new ThreadStart(() => { row.Cells[2].Value = Pages.Sum(t => t.VacanciesCount); }));
                    }
                }
            }
            catch (Exception e)
            {
                File.WriteAllLines("logs.txt", new string[] { e.Message });
            }

        }

    }
}
