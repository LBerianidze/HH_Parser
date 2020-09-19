using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace HH_Parser
{
    public class ResumesByCompanyNameParser
    {
        private readonly DataGridView _companiesLogs;
        private readonly IWebDriver _chromedriver;
        private readonly CompanySearchItem Company;
        private readonly Metros metros;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly Regex ageregex = new Regex(@".*?(?=\d)(\d*)");
        private readonly Regex workingsummaryregex = new Regex(@"(\d*).*(?=и)и (\d*)|(\d*).(.*)");
        private Regex salaryregex = new Regex(@".*?(?=\d)([\d,]*)");
        private int ItemsCount = 0;
        public MetroFramework.Controls.MetroLabel metroLabel { get; set; }
        public MetroFramework.Controls.MetroProgressBar metroProgressBar { get; set; }
        public ResumesByCompanyNameParser(CompanySearchItem company, IWebDriver driver, DataGridView companieslLogsRichTextBox, CancellationTokenSource tokensource)
        {
            this._chromedriver = driver;
            this._companiesLogs = companieslLogsRichTextBox;
            this.Company = company;
            this.cancellationTokenSource = tokensource;
        }
        public ResumesByCompanyNameParser(CompanySearchItem company, IWebDriver driver, CancellationTokenSource tokensource)
        {
            this._chromedriver = driver;
            this.Company = company;
            this.cancellationTokenSource = tokensource;
        }
        public ResumesByCompanyNameParser(CompanySearchItem company, IWebDriver driver, DataGridView companieslLogsRichTextBox, Metros metros, CancellationTokenSource tokensource)
            : this(company, driver, companieslLogsRichTextBox, tokensource)
        {
            this.metros = metros;
        }
        private List<HtmlNode> GetResumesNodes()
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(this._chromedriver.PageSource);
            var node = htmlDocument.DocumentNode.FindElementByAttributeAndGetNodes("div", "itemtype", "http://schema.org/Person");
            return node != null ? node.ToList() : new List<HtmlNode>();
        }
        public void ParseAlternatives()
        {
            var parsedresumes = new List<Resume>();
            var pageindex = 0;
            var companyname = this.Company.Name.ToLower();
            var count = 0;
            var qs = this._chromedriver.PageSource;
            while (true)
            {
                //Проверяем не отменена юзером ли операция парсинга 
                if (this.cancellationTokenSource.IsCancellationRequested)
                {
                    throw new OperationCanceledException(); //Завершаем парсинг
                }
                this.MovePage($"https://hh.ru/search/resume?text={this.Company.Website}&logic=normal&pos=workplace_organization&exp_period=all_time&clusters=true{this.Company.CompaniesArea}&order_by=publication_time&no_magic=false&items_on_page=100&page={pageindex}");
                var resumesNodes = this.GetResumesNodes();
                if (resumesNodes.Count == 0) //проверяем количество резюме на странице,если 0,завершаем парсинг
                {
                    break;
                }
                foreach (var item in resumesNodes)
                {
                    var link = "https://hh.ru" + item.FindElementByAttributeAndGetAnotherAttributeValue("a", "itemprop", "jobTitle", "href").Split('?')[0];//форматируем ссылку из ID резюме
                    this._chromedriver.Navigate().GoToUrl(link); //переходимо в резюме
                    var document = new HtmlDocument();
                    document.LoadHtml(this._chromedriver.PageSource);
                    if (this._chromedriver.PageSource.Contains("error__captcha")) //проверяем нет ли капчи
                    {
                        Thread.Sleep(1000);//ждем пока решится капча,автоматически или пользователем
                    }
                    var nodes = document.DocumentNode.FindElementByAttributeAndGetNodes("div", "itemprop", "worksFor");//получаем места работы пользователя
                    foreach (var it in nodes)
                    {
                        var node = it.FindElementByAttributeAndGetNode("a", "class", "resume__experience-url");//получаем блок ссылки компании,указанный в месте работы
                        if (node != null)//если такая ссылка имеется
                        {
                            var f = node.InnerText.ToLower();//содержание блока
                            if (f.Contains(this.Company.Website))//если контент блока содержит ссылку компании...
                            {
                                var name = it.FindElementByAttributeAndGetNode("div", "itemprop", "name").InnerText.ToLower();//получаем имя компании
                                if (!String.IsNullOrEmpty(name))//если оно не пустое
                                {
                                    this.Company.AlternativeNames.Add(name.Replace("&amp;", "&"));//Добавляем в список альтернативных имен
                                }
                            }
                        }
                    }
                    count++;
                    if (this.Company.CheckOnWebsiteCount != 0) //проверяем,не проверили ли мы нужное количество резюме
                    {
                        if (count >= this.Company.CheckOnWebsiteCount)//если да
                        {
                            break;//Завершаем парсинг альтернативных имен
                        }
                    }
                }
                if (this.Company.CheckOnWebsiteCount != 0) //та же проверка что и выше,но при переходе на следующую страницу
                {
                    if (count >= this.Company.CheckOnWebsiteCount)
                    {
                        break;
                    }
                }

            }
            this.Company.AlternativeNames = this.Company.AlternativeNames.GroupBy(g => g).Select(g => g.FirstOrDefault()).ToList();//фильтруем одинаковые имен в списке альтернативных имен,что бы не было дублей
        }
        public void Parse()
        {
            var pageindex = 0;
            var companyname = this.Company.Name.ToLower().Replace("&", "%26");
            List<Line> Lines = null;
            if (this.Company.ParseByMetro) //если нужно спарсить компанию по метро
            {
                if (this.Company.Places.Count == 1) //если количество указанных городов равно 1
                {
                    var index = this.metros.MetroList.FindIndex(f => f.ID.ToString() == this.Company.Places[0].ID); //содержит ли список метро указанный город
                    if (index != -1)//если содержит
                    {
                        Lines = this.metros.MetroList[index].Lines; //устанавливаем нужные линии метро
                    }
                }
            }
            if (Lines != null) //если метро есть
            {
                this.MovePage($"https://hh.ru/search/resume?text={companyname}&logic=normal&pos=workplace_organization&exp_period=all_time&clusters=true{this.Company.CompaniesArea}&order_by=publication_time&no_magic=false&items_on_page=100&page=49"); //переходим на последнюю страницу
                var doc = new HtmlDocument();
                doc.LoadHtml(this._chromedriver.PageSource);
                var textcount = doc.DocumentNode.FindElementByAttributeAndGetInnerText("h1", "data-qa", "page-title");//получаем заголовок страницы
                var regex = new Regex("Найдено (.*) резюме");
                var match = regex.Match(textcount);//ищем строку по шаблону
                var count1 = Convert.ToInt32(match.Groups[1].Value.Replace(" ", ""));//получаем количество резюме
                if (count1 > this.Company.MinResumesCount)//если количество резюме больше установленного предела в настройках программы
                {
                    //инициализируем парсинг метро
                    this.ParseByMetros(Lines);
                    //выходим из метода,посколку спарсили по метро
                    return;
                }
            }
            //если метро не было указано
            while (true)
            {
                if (this.cancellationTokenSource.IsCancellationRequested) //проверяем не остановил ли пользователь парсинг
                {
                    throw new OperationCanceledException();//останавливаем парсинг
                }
                this.MovePage($"https://hh.ru/search/resume?text={companyname}&logic=normal&pos=workplace_organization&exp_period=all_time&clusters=true{this.Company.CompaniesArea}&order_by=publication_time&no_magic=false&items_on_page=100&page={pageindex}");//переходимо на следующую страницу поиска
                var resumesNodes = this.GetResumesNodes();//получаем список резюме на странице
                if (resumesNodes.Count == 0)//если равен 0
                {
                    break;//завершаем парсинг
                }
                this.ResumeNodesParser(resumesNodes, companyname);//извлекаем информацию из блоков резюме
                pageindex++;
            }
            this.Company.CompanyResumes = this.Company.CompanyResumes.GroupBy(t => t.Link).Select(f => f.FirstOrDefault()).ToList();//фильтруем ссылки резюме
        }
        public void ParseByLink()
        {
            var pageindex = 0;
            var link = this.Company.Name;
            while (true)
            {
                if (this.cancellationTokenSource.IsCancellationRequested) //проверяем не остановил ли пользователь парсинг
                {
                    throw new OperationCanceledException();//останавливаем парсинг
                }
                this.MovePage(link+$"&items_on_page=100&page={pageindex}");//переходимо на следующую страницу поиска
                var resumesNodes = this.GetResumesNodes();//получаем список резюме на странице
                if (resumesNodes.Count == 0)//если равен 0
                {
                    break;//завершаем парсинг
                }
                this.ResumeNodesParser(resumesNodes, link,false);//извлекаем информацию из блоков резюме
                pageindex++;
            }
            this.Company.CompanyResumes = this.Company.CompanyResumes.GroupBy(t => t.Link).Select(f => f.FirstOrDefault()).ToList();//фильтруем ссылки резюме
        }

        public void ParseByMetros(List<Line> Lines)
        {
            //метод тот же что и Parse,с исключение что,проходим цикл линий метро
            var pageindex = 0;
            var companyname = this.Company.Name.ToLower().Replace("&", "%26");
            foreach (var line in Lines)
            {
                while (true)
                {
                    if (this.cancellationTokenSource.IsCancellationRequested)
                    {
                        throw new OperationCanceledException();
                    }
                    this.MovePage($"https://hh.ru/search/resume?text={companyname}&logic=normal&pos=workplace_organization&exp_period=all_time&clusters=true{this.Company.CompaniesArea}&order_by=publication_time&no_magic=false&items_on_page=100&page={pageindex}&metro={line.ID}");
                    var resumesNodes = this.GetResumesNodes();
                    if (resumesNodes.Count == 0)
                    {
                        pageindex = 0;
                        break;
                    }
                    this.ResumeNodesParser(resumesNodes, companyname);
                    pageindex++;
                }
            }
            this.Company.CompanyResumes = this.Company.CompanyResumes.GroupBy(t => t.Link).Select(f => f.FirstOrDefault()).ToList();//фильтруем ссылки резюме
        }
        public void ResumeNodesParser(List<HtmlNode> resumesNodes, string companyname,bool checkname = true)
        {
            foreach (var item in resumesNodes)
            {
                try
                {
                    var resume = new Resume
                    {
                        ResumeUpdate = DateTime.ParseExact(Form1.StringToDatetime(item.FindElementByAttributeAndGetInnerText
                                       ("span", "class", "resume-search-item__date")), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture),//извлекаем дату обновления резюме
                        OwnerName = item.FindElementByAttributeAndGetInnerText("div", "data-qa", "resume-serp__resume-fullname").Replace("&nbsp;", " ")//извлекаем имя владельца резюме
                    };
                    if(resume.ResumeCreated == DateTime.MinValue)
                    {
                        resume.ResumeCreated = resume.ResumeUpdate;
                    }
                    //разбиваем строку имени на имя и возвраст
                    if (resume.OwnerName.Length > 8) //если имя больше 8
                    {
                        var sp = resume.OwnerName.Split(','); //разбиваем строку
                        if (sp.Length == 2)
                        {
                            resume.OwnerName = sp[0];
                            resume.Age = sp[1];
                            var match = this.ageregex.Match(resume.Age);
                            if (match.Groups.Count >= 2)
                            {
                                resume.Age = match.Groups[1].Value;
                            }
                        }
                        else
                        {
                            resume.OwnerName = sp[0];
                            resume.Age = "0";
                        }
                    }
                    else
                    {
                        resume.Age = resume.OwnerName;
                        resume.OwnerName = "-";
                        var match = this.ageregex.Match(resume.Age);
                        if (match.Groups.Count >= 2)
                        {
                            resume.Age = match.Groups[1].Value;
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(resume.Age))
                            {
                                resume.OwnerName = resume.Age;
                            }
                            resume.Age = "0";
                        }
                    }
                    resume.Link = "https://hh.ru" + item.FindElementByAttributeAndGetAnotherAttributeValue("a", "itemprop", "jobTitle", "href").Split('?')[0];
                    resume.WorkingNow = item.FindElementByAttributeAndGetInnerText("div", "data-qa", "resume-serp_resume-item-content").Replace("&nbsp;", " ").Contains("по настоящее время");
                    resume.Salary = item.FindElementByAttributeAndGetInnerText("div", "class", "resume-search-item__compensation").Replace("&nbsp;", " ");
                    var smatch = this.salaryregex.Match(resume.Salary);
                    this.salaryregex = new Regex(@".*?(?=\d)([\d,]*) (.*)");
                    smatch = this.salaryregex.Match(resume.Salary);
                    if (smatch.Groups.Count == 3)
                    {
                        resume.Salary = smatch.Groups[1].Value;
                        resume.SalaryCurrency = smatch.Groups[2].Value;
                    }
                    resume.Position = item.FindElementByAttributeAndGetInnerText("a", "itemprop", "jobTitle");
                    resume.WorkingSummary = item.FindElementByAttributeAndGetInnerText("div", "data-qa", "resume-serp__resume-excpirience-sum");
                    var wmatch = this.workingsummaryregex.Match(resume.WorkingSummary);
                    if (wmatch.Groups.Count == 5)
                    {
                        if (String.IsNullOrEmpty(wmatch.Groups[4].Value))
                        {
                            resume.WorkingSummary = $"{wmatch.Groups[1].Value},{wmatch.Groups[2].Value}";
                        }
                        else if (wmatch.Groups[4].Value.Contains("лет") || wmatch.Groups[4].Value.Contains("год"))
                        {
                            resume.WorkingSummary = $"{wmatch.Groups[3].Value}";
                        }
                        else if (wmatch.Groups[4].Value.Contains("месяц"))
                        {
                            resume.WorkingSummary = $"0,{wmatch.Groups[3].Value}";
                        }
                    }
                    var region = item.FindElementByAttributeAndInnerTextAndGetNode("div", "data-qa", "resume-serp__resume-item-title", "Регион");
                    if (region != null)
                    {
                        resume.WorkingCity = region.ParentNode.ChildNodes[region.ParentNode.ChildNodes.IndexOf(region) + 1].InnerText;
                    }
                    var count = 0;
                    while (true)
                    {
                        var workingplacenode = item.FindElementByAttributeAndGetNode("div", "data-qa", "resume-serp_resume-item-content", count);
                        if (workingplacenode == null)
                        {
                            break;
                        }
                        if (workingplacenode.ChildNodes[0].Name != "div" && workingplacenode.ChildNodes[0].Name != "span")
                        {
                            count++;
                            continue;
                        }
                        if (workingplacenode.ChildNodes[0].InnerText.Contains("Произошла ошибка."))
                        {
                            var work = new Work
                            {
                                Name = workingplacenode.ChildNodes[1].InnerText.Trim().Replace("&amp;", "&")
                            };
                            var i = 3;
                            if (workingplacenode.ChildNodes.Count == 6)
                            {
                                i = 5;
                            }
                            work.Start = DateTime.ParseExact(Form1.StringToDate(workingplacenode.ChildNodes[i].InnerText.Replace("&nbsp;", " ").Split('—')[0].Trim()), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
                            work.End = !workingplacenode.ChildNodes[i].InnerText.Replace("&nbsp;", " ").Contains("по настоящее время") ? DateTime.ParseExact(Form1.StringToDate(workingplacenode.ChildNodes[i].InnerText.Replace("&nbsp;", " ").Split('—')[1].Trim()), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture) : DateTime.Now;
                            count++;
                            resume.WorkedPlaces.Add(work);
                        }
                        else
                        {
                            var work = new Work { Name = workingplacenode.ChildNodes[0].InnerText.Trim().Replace("&amp;", "&"), Start = DateTime.ParseExact(Form1.StringToDate(workingplacenode.ChildNodes[2].InnerText.Replace("&nbsp;", " ").Split('—')[0].Trim()), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture), End = !workingplacenode.ChildNodes[2].InnerText.Replace("&nbsp;", " ").Contains("по настоящее время") ? DateTime.ParseExact(Form1.StringToDate(workingplacenode.ChildNodes[2].InnerText.Replace("&nbsp;", " ").Split('—')[1].Trim()), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture) : DateTime.Now };
                            count++;
                            resume.WorkedPlaces.Add(work);
                        }
                    }
                    var _dayscount = DateTime.Now.Subtract(DateTime.Now.Subtract(new TimeSpan((this.Company.RestrictYears * 365) + (this.Company.RestrictMonthes * 30), 0, 0, 0))).TotalDays;
                    if (!this.Company.RestrictByDate)
                    {
                        _dayscount = 999999;
                    }
                    if (checkname)
                    {
                        foreach (var it in resume.WorkedPlaces)
                        {
                            DateTime datetimenow;
                            if (it.Name.ToLower().Contains(companyname) && it.Name.Length < companyname.Length + 30)
                            {
                                datetimenow = DateTime.Now;
                                var sub = datetimenow.Subtract(it.End);
                                if (sub.TotalDays <= _dayscount)
                                {
                                    this.Company.CompanyResumes.Add(resume);
                                    resume.Start = it.Start;
                                    resume.End = it.End;
                                    resume.WorkedPlaces.Clear();
                                    resume.WorkedPlaces.Add(it);
                                    this.WriteLog(resume);
                                }
                                break;
                            }
                            var wasfound = false;
                            foreach (var alname in this.Company.AlternativeNames)
                            {
                                if (it.Name.ToLower().Contains(alname) && it.Name.Length < alname.Length + 30)
                                {
                                    datetimenow = DateTime.Now;
                                    var sub = datetimenow.Subtract(it.End);
                                    if (sub.TotalDays <= _dayscount)
                                    {
                                        this.Company.CompanyResumes.Add(resume);
                                        resume.Start = it.Start;
                                        resume.End = it.End;
                                        resume.WorkedPlaces.Clear();
                                        resume.WorkedPlaces.Add(it);
                                        this.WriteLog(resume);
                                        wasfound = true;
                                    }
                                    break;
                                }
                            }
                            if (wasfound)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        this.Company.CompanyResumes.Add(resume);
                        this.WriteLog(resume);
                    }
                }
                catch (Exception ex)
                {

                }
            }

        }
        private void WriteLog(Resume res)
        {
            if (this._companiesLogs != null)
            {
                this._companiesLogs.Invoke(new ThreadStart(() =>
               {
                this.ItemsCount++;
                this._companiesLogs.Rows.Add(this.ItemsCount, res.OwnerName, res.Age, res.WorkingSummary, res.Salary + " " + res.SalaryCurrency, res.Position, res.WorkingCity, res.ResumeUpdate, String.Join("-", res.WorkedPlaces.Select(t => t.Name)), res.WorkedPlaces.First().Start.ToShortDateString(), res.WorkedPlaces.First().End.ToShortDateString(), res.Link);
               }));
            }
            if (this.metroLabel != null)
            {
                this.metroLabel.BeginInvoke(new MethodInvoker(() =>
                {
                    this.metroLabel.Text = (Convert.ToInt32(this.metroLabel.Text) + 1).ToString();
                }));
            }
        }
        private void MovePage(string url)
        {
            try
            {
                this._chromedriver.Url = url;
            }
            catch
            {
                var pageisloaded = false;
                while (!pageisloaded)
                {
                    pageisloaded = this.TryCatch();
                }
            }
        }
        private bool TryCatch()
        {
            try
            {
                IWait<IWebDriver> wait = new WebDriverWait(this._chromedriver, TimeSpan.FromSeconds(10));
                bool Func(IWebDriver driver1)
                {
                    return ((IJavaScriptExecutor)this._chromedriver).ExecuteScript("return document.readyState").Equals("complete");
                }
                wait.Until(Func);
                return true;
            }
            catch
            {
                return this._chromedriver == null ? true : false;
            }
        }
    }
}