using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tulpep.NotificationWindow;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace HH_Parser
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly List<User> _innerUsers = new List<User>();
        private IWebDriver _chrome;
        private string _companiesAreasdata = "";
        private string _companiesEmploymentTypedata = "";
        private string _companiesWorkTime = "";
        private MultiThreadedList<OgrnCompany> _ogrnCompanies;
        private List<string> _parsedCompaniesList = new List<string>();
        private List<Vacancy> _parsedVacancies;
        private bool _userparsingisinprocess;
        private XSSFWorkbook _workbook;
        private readonly List<CompanySearchItem> companyItems = new List<CompanySearchItem>();
        private PopupNotifier notifier;
        private CancellationTokenSource resumebycompanysearhsource;
        private List<CompanySearchItem> companiesResumesTrackerItems = new List<CompanySearchItem>();
        private int Tab5ParseType = 0;
        public Form1()
        {
            this.InitializeComponent();
            try
            {
                if (File.Exists("SavedCompanies.json"))
                {
                    var serializer = new JsonSerializer();
                    JsonReader reader = new JsonTextReader(new StreamReader("SavedCompanies.json", Encoding.UTF8));
                    this.companyItems = serializer.Deserialize<List<CompanySearchItem>>(reader);
                    this.textBox4.Text = this.companyItems.Count.ToString();
                }
            }
            catch (Exception)
            {

            }
            if (this.companyItems == null)
            {
                this.companyItems = new List<CompanySearchItem>();
            }
            httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
        }
        private bool RefreshFilterList { get; set; } = true;
        public int GetPhoneAndMailFromResume(User user)
        {
            this._chrome.Url = user.LastResume.Link;
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(this._chrome.PageSource);
            //GetPhone
            user.SearchName = htmlDocument.DocumentNode.FindElementByAttributeAndGetInnerText("h1", "data-qa", "resume-personal-name").Trim();
            user.Phone = htmlDocument.DocumentNode.FindElementByAttributeAndGetInnerText("span", "itemprop", "telephone");
            if (user.Phone.Contains("..."))
            {
                user.Phone = htmlDocument.DocumentNode.FindElementByAttributeAndGetInnerText("div", "class", "resume__contacts-phone-print-version");
            }
            user.Phone = user.Phone.Split('—')[0].Trim().Replace("&nbsp;", "");
            ////////////////////
            user.Mail = htmlDocument.DocumentNode.FindElementByAttributeAndGetInnerText("a", "itemprop", "email");
            if (htmlDocument.DocumentNode.FindElementByAttributeAndGetNode("span", "itemprop", "telephone") != null)
            {
                htmlDocument.DocumentNode.FindElementByAttributeAndGetNode("span", "itemprop", "telephone").InnerHtml = user.Phone;
            }
            //File.WriteAllText(Application.StartupPath + "\\htmls\\" + user.SearchName + ".html", htmlDocument.DocumentNode.OuterHtml);
            return !String.IsNullOrEmpty(user.Mail) && !String.IsNullOrEmpty(user.Phone)
                ? 1
                : !String.IsNullOrEmpty(user.Mail) ? 2 : !String.IsNullOrEmpty(user.Phone) ? 3 : 0;
        }
        private void AddUsersColorLogAsync(string text, Color color)
        {
            this.BeginInvoke(new MethodInvoker(() =>
            {
                var old = this.LogsRichTextBox.TextLength;
                this.LogsRichTextBox.AppendText(text);
                var newl = this.LogsRichTextBox.TextLength;
                this.LogsRichTextBox.Select(old, newl - old);
                this.LogsRichTextBox.SelectionColor = color;
            }));
        }
        private void AddUsersLogAsync(string text)
        {
            this.BeginInvoke(new MethodInvoker(() => this.LogsRichTextBox.AppendText(text)));
        }
        private void AuthorizeButton_Click(object sender, EventArgs e)
        {
            var options = new ChromeOptions
            {
                //options.AddExtension("Block-image_v1.1.crx");
                //options.AddExtension("captchasolver.crx");
                AcceptInsecureCertificates = true
            };
            var service = ChromeDriverService.CreateDefaultService();
            service.EnableVerboseLogging = false;
            service.SuppressInitialDiagnosticInformation = true;
            this._chrome = new ChromeDriver(service, options);
            try
            {
                this._chrome.Url = "https://hh.ru/account/login?backurl=%2F";
                var element = this._chrome.FindElement(By.Name("username"));
                element.SendKeys(this.textBox1.Text);
                this._chrome.FindElement(By.Name("password")).SendKeys(this.textBox2.Text);
                this._chrome.FindElement(By.XPath("/html/body/div[1]/div/div[2]/div/div/form/div[3]/input")).Click();
                Thread.Sleep(3000);
                this._chrome.FindElement(By.Name("password")).SendKeys(this.textBox2.Text);
                Thread.Sleep(2000);
                this._chrome.FindElement(By.ClassName("HH-Recaptcha-Placeholder g-recaptcha")).Click();
                this._chrome.FindElement(By.XPath("/html/body/div[1]/div/div[2]/div/div/form/div[3]/input")).Click();
            }
            catch
            {
                if (this._chrome.Url.Contains("hh.ru/account/login"))
                {
                    this.notifier = new PopupNotifier
                    {
                        Image = Properties.Resources.Info_Button_256,
                        TitleText = "HH Parser - Ошибка",
                        TitleFont = new Font("Segui UI", 16, FontStyle.Regular, GraphicsUnit.Pixel),
                        TitleColor = Color.Black,
                        AnimationInterval = 1,
                        AnimationDuration = 1000,
                        Delay = 20000,
                        ContentFont = new Font("Segui UI", 14, FontStyle.Regular, GraphicsUnit.Pixel),
                        ContentText = "Возникла ошибка во время авторизации!",
                        ShowCloseButton = true,
                        ImageSize = new Size(35, 35),
                        ImagePadding = new Padding(3, 3, 0, 0),
                        Size = new Size(400, 90),
                        HeaderColor = Color.SlateGray
                    };
                    this.notifier.Popup();
                }
            }
            this.CompaniesFirstParsing = true;
        }
        private void CitiesTB_DoubleClick(object sender, EventArgs e)
        {
            var window = new SelectCitiesWindow();
            var companiesCities = window.ShowDialog(this);
            this._companiesAreasdata = "";
            if (companiesCities != null)
            {
                this.CitiesTB.Text = String.Join(",", companiesCities.Select(t => t.Name));
                foreach (var item in companiesCities)
                {
                    this._companiesAreasdata += $"area={item.ID}&";
                }
                if (!String.IsNullOrEmpty(this._companiesAreasdata))
                {
                    this._companiesAreasdata = this._companiesAreasdata.Substring(0, this._companiesAreasdata.Length - 1);
                }
            }
        }
        private void ClearFilter_Click(object sender, EventArgs e)
        {
            this._parsedCompaniesList.Clear();
            this.label4.Text = "Файл не загружен";
        }
        private void EmploymentType_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            this._companiesEmploymentTypedata = "";
            var indexes = this.EmploymentType.CheckedIndices.Cast<int>().ToList();
            if (e.NewValue == CheckState.Checked)
            {
                indexes.Add(e.Index);
            }
            else if (e.NewValue == CheckState.Unchecked)
            {
                indexes.Remove(e.Index);
            }
            if (indexes.Contains(0))
            {
                this._companiesEmploymentTypedata += "employment=full&";
            }
            if (indexes.Contains(1))
            {
                this._companiesEmploymentTypedata += "employment=part&";
            }
            if (indexes.Contains(2))
            {
                this._companiesEmploymentTypedata += "employment=project&";
            }
            if (indexes.Contains(3))
            {
                this._companiesEmploymentTypedata += "employment=volunteer&";
            }
            if (indexes.Contains(4))
            {
                this._companiesEmploymentTypedata += "employment=probation&";
            }
            if (!String.IsNullOrEmpty(this._companiesEmploymentTypedata))
            {
                this._companiesEmploymentTypedata = this._companiesEmploymentTypedata.Substring(0, this._companiesEmploymentTypedata.Length - 1);
            }
        }
        private bool FindLastResumeByMailorPhone(User user, byte type)
        {
            this._chrome.Url = type == 0
                ? $"https://hh.ru/search/resume?text={user.Mail}&logic=normal&pos=full_text&exp_period=all_time&clusters=true&order_by=publication_time&no_magic=false"
                : $"https://hh.ru/search/resume?text={user.Phone}&logic=normal&pos=full_text&exp_period=all_time&clusters=true&order_by=publication_time&no_magic=false";
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(this._chrome.PageSource);
            var resumes = htmlDocument.DocumentNode.FindElementByAttributeAndGetNodes("div", "itemtype", "http://schema.org/Person");
            if (resumes.Count != 0)
            {
                user.LastResume = new Resume
                {
                    Link = "https://hh.ru" + resumes[0].FindElementByAttributeAndGetNode("a", "itemprop", "jobTitle").Attributes[3].Value.Split('?')[0]
                };
                user.LastResume.Mail = user.Mail;
                user.LastResume.Phone = user.Phone;
                try
                {
                    user.LastResume.ResumeUpdate = DateTime.ParseExact(StringToDatetime(resumes[0].FindElementByAttributeAndGetInnerText("span", "class", "output__tab m-output__date")), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
                }
                catch
                {
                    user.LastResume.ResumeUpdate = DateTime.ParseExact(StringToDatetime(resumes[0].FindElementByAttributeAndGetInnerText("span", "class", "resume-search-item__date")), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
                }
                if (user.FirstlyParsedDate == DateTime.MinValue)
                {
                    user.FirstlyParsedDate = user.LastResume.ResumeUpdate;
                }
                user.LastResume.WorkingNow = resumes[0].FindElementByAttributeAndGetInnerText("div", "data-qa", "resume-serp_resume-item-content").Replace("&nbsp;", " ").Contains("по настоящее время");
                user.ResumesCount = Convert.ToInt32(htmlDocument.DocumentNode.FindElementByAttributeAndGetInnerText("h1", "data-qa", "page-title").Split(' ')[1].Replace(" ", ""));
                if (type == 0)
                {
                    this.AddUsersLogAsync($"Получили резюме пользователя используя почтовый адрес {user.Mail}{Environment.NewLine}");
                }
                else
                {
                    this.AddUsersLogAsync($"Получили резюме пользователя используя номер телефона {user.Phone}{Environment.NewLine}");
                }
                this.GetPhoneAndMailFromResume(user);
                return true;
            }
            return false;
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                var serializer = new JsonSerializer();
                var sw = new StreamWriter("SavedCompanies.json", false, Encoding.UTF8);
                serializer.Serialize(sw, this.companyItems, typeof(List<CompanySearchItem>));
                sw.Close();
                sw.Dispose();
                this._chrome?.Close();
                this._chrome?.Quit();
                this.notifier?.Hide();
                Environment.Exit(0);
            }
            catch (Exception)
            {
            }
        }

        private bool CheckIfSeleniumIsAlive()
        {
            try
            {
                this._chrome.Manage();
                this._chrome.Navigate();
                var windows = this._chrome.WindowHandles;
                return true;
            }
            catch
            {
                return false;
            }
        }
        private void IncrementUsersProgress()
        {
            this.BeginInvoke(new MethodInvoker(() => this.UsersProgress.Value++));
        }
        private void LoadCompaniesForOGRN_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                FileName = "Excel File",
                Filter = "xlsx|*.xlsx"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                this._ogrnCompanies = new MultiThreadedList<OgrnCompany>();
                this._workbook = new XSSFWorkbook(new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                var sheet = this._workbook.GetSheetAt(0);
                var count = sheet.LastRowNum;
                for (var i = 1; i <= count; i++)
                {
                    var row = sheet.GetRow(i);
                    var oGrnCompany = new OgrnCompany();
                    if (row != null)
                    {
                        oGrnCompany.WebSiteUrl = row.GetCell(3)?.StringCellValue?.Split('/')?.Last();
                    }
                    this._ogrnCompanies.Add(oGrnCompany);
                }
                this.label5.Text = $"Загружено: компаний - {this._ogrnCompanies.Count}";
            }
        }
        private void LoadFilterFile_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                FileName = "Excel File",
                Filter = "xlsx|*.xlsx",
                Multiselect = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                this._parsedCompaniesList = new List<string>();
                foreach (var item in ofd.FileNames)
                {
                    var workbook = new XSSFWorkbook(new FileStream(item, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                    var sheet = workbook.GetSheetAt(0);
                    var count = sheet.LastRowNum;
                    for (var i = 1; i <= count; i++)
                    {
                        var row = sheet.GetRow(i);
                        if (row != null)
                        {
                            this._parsedCompaniesList.Add(row.GetCell(0)?.StringCellValue?.Split('/')?.Last());
                        }
                    }
                }
                this.label4.Text = $"Загружено: файлов - {ofd.FileNames.Length} , компаний - {this._parsedCompaniesList.Count}";
            }
        }
        private void LoadListButton_Click(object sender, EventArgs e)
        {
            try
            {
                var ofd = new OpenFileDialog
                {
                    Filter = "xlsx|*.xlsx"
                };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    this._innerUsers.Clear();
                    var workbook = new XSSFWorkbook(new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                    var sheet = workbook.GetSheetAt(0);
                    var count = sheet.LastRowNum;
                    for (var i = 1; i <= count; i++)
                    {
                        var row = sheet.GetRow(i);
                        if (row != null)
                        {
                            var user = new User
                            {
                                LoadedName = row.GetCell(1)?.StringCellValue,
                                ID = i
                            };
                            if (!String.IsNullOrEmpty(row.GetCell(0)?.StringCellValue))
                            {
                                user.LastResume = new Resume
                                {
                                    Link = row.GetCell(0)?.StringCellValue
                                };
                            }
                            user.Phone = row.GetCell(2)?.GetCellValue();
                            user.Mail = row.GetCell(3)?.StringCellValue;
                            user.Office = row.GetCell(4)?.StringCellValue;
                            user.Department = row.GetCell(5)?.StringCellValue;
                            user.Position = row.GetCell(6)?.StringCellValue;
                            this._innerUsers.Add(user);
                        }
                    }
                    this.label3.Text = $"Загружено {this._innerUsers.Count} резюме";
                }
            }
            catch
            {
                MessageBox.Show("Загружен неверный файл");
            }
        }
        private void LoadParsedFile_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "xlsx|*.xlsx"
            };
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            this._innerUsers.Clear();
            var workbook = new XSSFWorkbook(new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            var sheet = workbook.GetSheetAt(0);
            var count = sheet.LastRowNum;
            for (var i = 1; i <= count; i++)
            {
                var row = sheet.GetRow(i);
                if (row == null)
                {
                    continue;
                }
                var user = new User
                {
                    LoadedName = row.GetCell(0)?.StringCellValue,
                    SearchName = row.GetCell(1)?.StringCellValue
                };
                user.Phone = row.GetCell(2)?.GetCellValue();
                user.Mail = row.GetCell(3)?.StringCellValue;
                user.ResumesCount = (int)row.GetCell(4)?.NumericCellValue;
                if (row.GetCell(5) != null)
                {
                    user.LastResume = new Resume
                    {
                        Link = row.GetCell(5)?.StringCellValue
                    };
                }
                user.ResumeFound = row.GetCell(6)?.StringCellValue;
                if (row.GetCell(7)?.CellType == CellType.Numeric)
                {
                    var dateCellValue = row.GetCell(7)?.DateCellValue;
                    if (dateCellValue != null)
                    {
                        user.FirstlyParsedDate = row.GetCell(7).DateCellValue;
                    }
                }
                if (user.LastResume != null && row.GetCell(8)?.CellType == CellType.Numeric)
                {
                    var dateCellValue = row.GetCell(8)?.DateCellValue;
                    if (dateCellValue != null)
                    {
                        user.LastResume.ResumeUpdate = (DateTime)dateCellValue;
                    }
                }
                user.Office = row.GetCell(9)?.StringCellValue;
                user.Department = row.GetCell(10)?.StringCellValue;
                user.Position = row.GetCell(11)?.StringCellValue;
                user.OldPhone = row.GetCell(12)?.GetCellValue();
                user.OldMail = row.GetCell(13)?.StringCellValue;
                this._innerUsers.Add(user);
            }
            this.label3.Text = $"Загружено {this._innerUsers.Count} резюме";
        }
        private void OpenReportGenerator_Click(object sender, EventArgs e)
        {
            if (this._innerUsers != null)
            {
                var form = new ReportGeneratorForm(this._innerUsers);
                form.ShowDialog(this);
            }
        }
        private async void ParseCompanies_Click(object sender, EventArgs e)
        {
            this.resumebycompanysearhsource = new CancellationTokenSource();
            if (this._chrome == null)
            {
                MessageBox.Show("Необходимо авторизироваться", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (this.companyItems.Count == 0)
            {
                MessageBox.Show("Нет добавленных компаний", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (Directory.Exists(Application.StartupPath + "\\htmls"))
            {
                Directory.Delete(Application.StartupPath + "\\htmls");
            }
            Directory.CreateDirectory(Application.StartupPath + "\\htmls");
            this.resumes_grid.Rows.Clear();
            var showdialog = MessageBox.Show("Показать окно редактирования компаний для изменения альтернативных имен после парсинга по сайту?", "Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            this._chrome.Url = "https://hh.ru/search/resume?text=%D0%9B%D0%B0%D0%BD%D0%B8%D1%82&logic=normal&pos=workplace_organization&exp_period=all_time&area=1&relocation=living_or_relocation&salary_from=&salary_to=&currency_code=RUR&education=none&age_from=&age_to=&gender=unknown&order_by=publication_time&search_period=0&items_on_page=100";
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(this._chrome.PageSource);
                var xpath1 = doc.DocumentNode.FindElementByTagNameAndInnerText("span", "Вид результатов")?.XPath; ;
                this._chrome.FindElement(By.XPath(xpath1)).Click();
                Thread.Sleep(1000);
                doc = new HtmlDocument();
                doc.LoadHtml(this._chrome.PageSource);
                var xpath = doc.DocumentNode.FindElementByAttributeAndGetXpath("input", "data-qa", "resume-serp__view-companies");
                var attr = this._chrome.FindElement(By.XPath(xpath)).GetAttribute("checked");
                if (attr == "false" || attr == null)
                {
                    this._chrome.FindElement(By.XPath(xpath.Replace("/input[1]", ""))).Click();
                    Thread.Sleep(500);
                }
                xpath = doc.DocumentNode.FindElementByAttributeAndGetXpath("input", "value", "area");
                attr = this._chrome.FindElement(By.XPath(xpath)).GetAttribute("checked");
                if (attr == "false" || attr == null)
                {
                    this._chrome.FindElement(By.XPath(xpath.Replace("/input[1]", ""))).Click();
                    Thread.Sleep(500);
                }
                this._chrome.FindElement(By.XPath(doc.DocumentNode.FindElementByAttributeAndGetXpath("input", "data-qa", "resume-serp__view-save"))).Click();
            }
            catch (Exception)
            {
                var result = MessageBox.Show("Возникла ошибка в ходе установки параметров.Возможно это связано с ошибкой в ходе авторизации.\r" +
                      "При продолжение парсинга,результат будет неполным.\r" +
                      "Продолжить?", "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                {
                    MessageBox.Show("Отменить парсинг?", "Информация", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    return;
                }
            }
            try
            {
                foreach (var item in this.companyItems)
                {
                    if (item.CheckOnWebsite && !String.IsNullOrEmpty(item.Website))
                    {
                        var parser = new ResumesByCompanyNameParser(item, this._chrome, this.resumes_grid, this.resumebycompanysearhsource);
                        await Task.Run(() => parser.ParseAlternatives());
                    }
                }
                if (showdialog == DialogResult.Yes)
                {
                    var editform = new CompaniesEditForm(this.companyItems);
                    editform.ShowDialog(this);
                }
                Thread.Sleep(1000);
                var metros = new Metros();
                foreach (var item in this.companyItems)
                {
                    item.CompanyResumes = new List<Resume>();
                    this.resumes_grid.Rows.Add("Next", "Начало парсинга компании: " + item.Name);
                    var parser = new ResumesByCompanyNameParser(item, this._chrome, this.resumes_grid, metros, this.resumebycompanysearhsource);
                    await Task.Run(() => parser.Parse());
                }
                if (this.notifier == null)
                {
                    this.notifier = new PopupNotifier
                    {
                        Image = Properties.Resources.Info_Button_256,
                        TitleText = "HH Parser",
                        TitleFont = new Font("Segui UI", 16, FontStyle.Regular, GraphicsUnit.Pixel),
                        TitleColor = Color.Black,
                        AnimationInterval = 1,
                        AnimationDuration = 1000,
                        Delay = 20000,
                        ContentFont = new Font("Segui UI", 14, FontStyle.Regular, GraphicsUnit.Pixel),
                        ContentText = "Парсинг резюме завершен!",
                        ShowCloseButton = true,
                        ImageSize = new Size(35, 35),
                        ImagePadding = new Padding(3, 3, 0, 0),
                        Size = new Size(400, 90),
                        HeaderColor = Color.SlateGray
                    };
                    this.notifier.Popup();
                }
                else
                {
                    this.notifier.Hide();
                    this.notifier.Popup();
                }
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Парсинг отменен", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("Возникла ошибка во время парсинга", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }

        }
        private async void ParseOGRN_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.textBox3.Text))
            {
                MessageBox.Show("Прокси не указан");
                return;
            }
            this.OGRNParseProgress.Maximum = this._ogrnCompanies.Count;
            this.OGRNStatusLabel.Text = "Парсинг запущен...";
            var proxies = new ProxyList(new List<string>() { this.textBox3.Text });
            var parsers = new List<OgrnParser>();
            var parser = new OgrnParser(this._ogrnCompanies, proxies, this.OGRNParseProgress);
            await Task.Run(() => parser.Start());
            this.OGRNStatusLabel.Text = "Парсинг завершен";
        }
        private async void ParseVacancies_Click(object sender, EventArgs e)
        {
            try
            {
                this.CompaniesThreadGrid.Rows.Clear();
                var vacancyname = this.VacancyName.Text;
                this.ThirdStatusLabel.Text = "Парсим список компаний";
                var link = $"https://api.hh.ru/vacancies?text={vacancyname}&per_page=100";
                if (!String.IsNullOrEmpty(this._companiesAreasdata))
                {
                    link += "&" + this._companiesAreasdata;
                }
                if (!String.IsNullOrEmpty(this._companiesEmploymentTypedata))
                {
                    link += "&" + this._companiesEmploymentTypedata;
                }
                if (!String.IsNullOrEmpty(this._companiesWorkTime))
                {
                    link += "&" + this._companiesWorkTime;
                }
                var row2 = new DataGridViewRow();
                row2.CreateCells(this.CompaniesThreadGrid, 0, 0, 0, "В процессе...");
                this.CompaniesThreadGrid.Rows.Add(row2);
                var parser = new VacanciesParser(link, row2);
                await Task.Run(() =>
                {
                    parser.StartParse();
                    this._parsedVacancies = parser.Pages.SelectMany(t => t.Vacancies).GroupBy(t => t.ID).Select(t => t.FirstOrDefault()).GroupBy(t => t.Employer.ID).Select(t => t.First()).ToList();
                    this._parsedVacancies.RemoveAll(t => t.Employer.ID == 0);
                });
                this.CompaniesThreadGrid.Rows.Clear();
                var threads = 15;
                var per = this._parsedVacancies.Count / threads;
                var last = this._parsedVacancies.Count - (per * threads) + per;
                var tasks = new List<Task>();
                for (var i = 0; i < threads; i++)
                {
                    var j = i;
                    if (i == threads - 1)
                    {
                        var row1 = new DataGridViewRow();
                        row1.CreateCells(this.CompaniesThreadGrid, j, last, 0, "В процессе...");
                        this.CompaniesThreadGrid.Rows.Add(row1);
                        tasks.Add(Task.Run(() => this.Parse(this._parsedVacancies.Skip(j * per).Take(last).ToList(), row1)));
                        continue;
                    }
                    var row = new DataGridViewRow();
                    row.CreateCells(this.CompaniesThreadGrid, j, per, 0, "В процессе...");
                    this.CompaniesThreadGrid.Rows.Add(row);
                    tasks.Add(Task.Run(() => this.Parse(this._parsedVacancies.Skip(j * per).Take(per).ToList(), row)));
                }
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                File.WriteAllLines("logs1.txt", new string[] { ex.Message });
            }
            if (this.notifier == null)
            {
                this.notifier = new PopupNotifier
                {
                    Image = Properties.Resources.Info_Button_256,
                    TitleText = "HH Parser",
                    TitleFont = new Font("Segui UI", 16, FontStyle.Regular, GraphicsUnit.Pixel),
                    TitleColor = Color.Black,
                    AnimationInterval = 1,
                    AnimationDuration = 1000,
                    Delay = 20000,
                    ContentFont = new Font("Segui UI", 14, FontStyle.Regular, GraphicsUnit.Pixel),
                    ContentText = "Парсинг компаний завершен!",
                    ShowCloseButton = true,
                    ImageSize = new Size(35, 35),
                    ImagePadding = new Padding(3, 3, 0, 0),
                    Size = new Size(400, 90),
                    HeaderColor = Color.SlateGray
                };
                this.notifier.Popup();
            }
            else
            {
                this.notifier.Hide();
                this.notifier.Popup();
            }
        }
        public void Parse(List<Vacancy> vacanies, DataGridViewRow row)
        {
            var wb = new WebHelper();
            foreach (var item in vacanies)
            {
                var source = wb.GetHttpSourcePage("https://api.hh.ru/employers/" + item.Employer.ID);
                var count = wb.GetHttpSourcePage("https://api.hh.ru/vacancies?employer_id=" + item.Employer.ID);
                item.Employer.WebsiteURL = JObject.Parse(source)["site_url"].ToString();
                item.Employer.VacancyCount = JObject.Parse(count)["found"].ToString();
                row.DataGridView.BeginInvoke(new ThreadStart(() => { row.Cells[2].Value = (int)row.Cells[2].Value + 1; }));
            }
            row.DataGridView.BeginInvoke(new ThreadStart(() => { row.Cells[3].Value = "Готово"; }));
        }
        private void RefreshDoubles_CheckedChanged(object sender, EventArgs e)
        {
            this.RefreshFilterList = this.RefreshDoubles.Checked;
        }
        private void ResetUsersProgressAndSetMaximum(int max)
        {
            this.BeginInvoke(new MethodInvoker(() =>
            {
                this.UsersProgress.Value = 0;
                this.UsersProgress.Maximum = max;
            }));
        }
        private void SaveCompanies_Click(object sender, EventArgs e)
        {
            if (this._parsedVacancies == null)
            {
                return;
            }
            var sfd = new SaveFileDialog
            {
                Filter = "xlsx|*.xlsx",
                FileName = "Результат парсинга компаний"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var workbook = new XSSFWorkbook();
                var sheet = workbook.CreateSheet();
                for (var i = 0; i < this._parsedVacancies.Count + 1; i++)
                {
                    var row = sheet.CreateRow(i);
                    for (var z = 0; z < 10; z++)
                    {
                        row.CreateCell(z);
                    }
                }
                sheet.GetRow(0).GetCell(0).SetCellValue("Ссылка на HH");
                sheet.GetRow(0).GetCell(1).SetCellValue("Название компании");
                sheet.GetRow(0).GetCell(2).SetCellValue("Вакансия");
                sheet.GetRow(0).GetCell(3).SetCellValue("Сайт");
                sheet.GetRow(0).GetCell(4).SetCellValue("Количество вакансий");
                sheet.GetRow(0).GetCell(5).SetCellValue("Контактное лицо");
                sheet.GetRow(0).GetCell(6).SetCellValue("Номер телефона");
                sheet.GetRow(0).GetCell(7).SetCellValue("Почта");
                sheet.GetRow(0).GetCell(8).SetCellValue("Зарплата");
                sheet.GetRow(0).GetCell(9).SetCellValue("Город");
                if (this.checkBox3.Checked)
                {
                    this._parsedVacancies = this._parsedVacancies.OrderByDescending(t => t.Contact?.name).ToList();
                }
                for (var i = 0; i < this._parsedVacancies.Count; i++)
                {
                    sheet.GetRow(i + 1).GetCell(0).SetCellValue(this._parsedVacancies[i].Employer.alternate_url);
                    sheet.GetRow(i + 1).GetCell(1).SetCellValue(this._parsedVacancies[i].Employer.Name);
                    sheet.GetRow(i + 1).GetCell(2).SetCellValue(this._parsedVacancies[i].VacancyUrl);
                    sheet.GetRow(i + 1).GetCell(3).SetCellValue(this._parsedVacancies[i].Employer.WebsiteURL);
                    sheet.GetRow(i + 1).GetCell(4).SetCellValue(this._parsedVacancies[i].Employer.VacancyCount);
                    sheet.GetRow(i + 1).GetCell(5).SetCellValue(this._parsedVacancies[i].Contact?.name);
                    sheet.GetRow(i + 1).GetCell(6).SetCellValue(this._parsedVacancies[i].Phones);
                    sheet.GetRow(i + 1).GetCell(7).SetCellValue(this._parsedVacancies[i].Contact?.email);
                    sheet.GetRow(i + 1).GetCell(8).SetCellValue(this._parsedVacancies[i].Salary?.GetSalary());
                    sheet.GetRow(i + 1).GetCell(9).SetCellValue(this._parsedVacancies[i].Address?.City);
                }
                for (var i = 0; i < 10; i++)
                {
                    sheet.AutoSizeColumn(i);
                }
                var fs = new FileStream(sfd.FileName, FileMode.Create);
                workbook.Write(fs);
            }
        }
        private void SaveUsers_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                Filter = "xlsx|*.xlsx",
                FileName = "Result"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var workbook = new XSSFWorkbook();
                var sheet = workbook.CreateSheet();
                for (var i = 0; i < this._innerUsers.Count + 1; i++)
                {
                    var row = sheet.CreateRow(i);
                    for (var z = 0; z <= 16; z++)
                    {
                        row.CreateCell(z);
                    }
                }
                sheet.GetRow(0).GetCell(0).SetCellValue("Имя");
                sheet.GetRow(0).GetCell(1).SetCellValue("Имя(Поиск)");
                sheet.GetRow(0).GetCell(2).SetCellValue("Телефон");
                sheet.GetRow(0).GetCell(3).SetCellValue("Почта");
                sheet.GetRow(0).GetCell(4).SetCellValue("Кол-во резюме");
                sheet.GetRow(0).GetCell(5).SetCellValue("Последнее резюме");
                sheet.GetRow(0).GetCell(6).SetCellValue("Резюме найдено");
                sheet.GetRow(0).GetCell(7).SetCellValue("Дата создания резюме");
                sheet.GetRow(0).GetCell(8).SetCellValue("Дата обновления резюме");
                sheet.GetRow(0).GetCell(9).SetCellValue("Отдел");
                sheet.GetRow(0).GetCell(10).SetCellValue("Отделение");
                sheet.GetRow(0).GetCell(11).SetCellValue("Должность");
                sheet.GetRow(0).GetCell(12).SetCellValue("Старый телефон");
                sheet.GetRow(0).GetCell(13).SetCellValue("Старая почта");
                sheet.GetRow(0).GetCell(14).SetCellValue("Найдено по");
                sheet.GetRow(0).GetCell(15).SetCellValue("Резюме");
                for (var i = 0; i < this._innerUsers.Count; i++)
                {
                    sheet.GetRow(i + 1).GetCell(0).SetCellValue(this._innerUsers[i].LoadedName);
                    sheet.GetRow(i + 1).GetCell(1).SetCellValue(this._innerUsers[i].SearchName);
                    sheet.GetRow(i + 1).GetCell(2).SetCellValue(this._innerUsers[i].Phone);
                    sheet.GetRow(i + 1).GetCell(3).SetCellValue(this._innerUsers[i].Mail);
                    sheet.GetRow(i + 1).GetCell(4).SetCellValue(this._innerUsers[i].ResumesCount);
                    sheet.GetRow(i + 1).GetCell(5).SetCellValue(this._innerUsers[i].LastResume?.Link);
                    sheet.GetRow(i + 1).GetCell(6).SetCellValue(this._innerUsers[i].ResumeFound);
                    if (this._innerUsers[i].FirstlyParsedDate != DateTime.MinValue)
                    {
                        var cell = sheet.GetRow(i + 1).GetCell(7);
                        var format = workbook.CreateDataFormat();
                        var cellStyle = workbook.CreateCellStyle();
                        cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd HH:mm:ss");
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(this._innerUsers[i].FirstlyParsedDate);
                    }
                    else
                    {
                        sheet.GetRow(i + 1).GetCell(7).SetCellValue("-");
                    }
                    if (this._innerUsers[i].LastResume?.ResumeUpdate != null && this._innerUsers[i].LastResume?.ResumeUpdate != DateTime.MinValue)
                    {
                        var cell = sheet.GetRow(i + 1).GetCell(8);
                        var format = workbook.CreateDataFormat();
                        var cellStyle = workbook.CreateCellStyle();
                        cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd HH:mm:ss");
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(this._innerUsers[i].LastResume.ResumeUpdate);
                    }
                    else
                    {
                        sheet.GetRow(i + 1).GetCell(8).SetCellValue("-");
                    }
                    sheet.GetRow(i + 1).GetCell(9).SetCellValue(this._innerUsers[i].Office);
                    sheet.GetRow(i + 1).GetCell(10).SetCellValue(this._innerUsers[i].Department);
                    sheet.GetRow(i + 1).GetCell(11).SetCellValue(this._innerUsers[i].Position);
                    sheet.GetRow(i + 1).GetCell(12).SetCellValue(this._innerUsers[i].OldPhone);
                    sheet.GetRow(i + 1).GetCell(13).SetCellValue(this._innerUsers[i].OldMail);
                    sheet.GetRow(i + 1).GetCell(14).SetCellValue(this._innerUsers[i].ParsedBy == 0 ? "-" : this._innerUsers[i].ParsedBy == 1 ? "Почта" : this._innerUsers[i].ParsedBy == 2 ? "Телефон" : "Ссылка");
                    var cell1 = sheet.GetRow(i + 1).GetCell(15);
                    cell1.Hyperlink = new XSSFHyperlink(HyperlinkType.File)
                    {
                        Address = "resumes/" + i + ".html",
                        Label = "Открыть"
                    };
                    cell1.SetCellValue("Открыть");
                }
                for (var i = 0; i <= 13; i++)
                {
                    sheet.AutoSizeColumn(i);
                }
                var fs = new FileStream(sfd.FileName, FileMode.Create);
                workbook.Write(fs);
                if (Directory.Exists(Application.StartupPath + "\\htmls"))
                {
                    var dirpath = Path.GetDirectoryName(sfd.FileName);
                    if (Directory.Exists(dirpath + "\\resumes"))
                    {
                        var result = MessageBox.Show("Каталог \"resumes\" уже существует,желаете заменить?", "Replace", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            Directory.Move(Application.StartupPath + "\\htmls", Path.GetDirectoryName(sfd.FileName) + "\\resumes");
                        }
                    }
                    else
                    {
                        Directory.Move(Application.StartupPath + "\\htmls", Path.GetDirectoryName(sfd.FileName) + "\\resumes");
                    }
                }
            }
        }
        private void SetUsersProgressStatusLabelText(string text)
        {
            this.BeginInvoke(new MethodInvoker(() => { this.StatusLabel.Text = text; }));
        }
        private async void StartParsingButton_Click(object sender, EventArgs e)
        {
            if (this._userparsingisinprocess)
            {
                return;
            }
            if (this._chrome == null)
            {
                MessageBox.Show("Необходимо авторизироваться");
                return;
            }
            this._userparsingisinprocess = true;
            this.LogsRichTextBox.Clear();
            this.StatusLabel.Text = "Получаем последнее резюме пользователей";
            this.UsersProgress.Value = 0;
            this.UsersProgress.Maximum = this._innerUsers.Count;
            await Task.Run(() =>
            {
                foreach (var user in this._innerUsers)
                {
                    var r = user.LastResume;
                    if (!String.IsNullOrEmpty(user.Mail)) //Если есть почта
                    {
                        var resumeFound = this.FindLastResumeByMailorPhone(user, 0); //ищем по почте
                        user.ParsedBy = 1;
                        if (!resumeFound && !String.IsNullOrEmpty(user.Phone))
                        {
                            resumeFound = this.FindLastResumeByMailorPhone(user, 1);
                            if (!resumeFound && !String.IsNullOrEmpty(user.LastResume?.Link))
                            {
                                user.ParsedBy = 1;
                                var result = this.GetPhoneAndMailFromResume(user);
                                if (result == 1 || result == 2)
                                {
                                    this.FindLastResumeByMailorPhone(user, 0);
                                }
                                else if (result == 3)
                                {
                                    this.FindLastResumeByMailorPhone(user, 1);
                                    user.ParsedBy = 2;
                                }
                                else
                                {
                                    user.ParsedBy = 0;
                                    this.AddUsersColorLogAsync($"Не удалось найти пользователя с телефонным номером {(String.IsNullOrEmpty(user.Phone) ? "..." : user.Phone)} или почтой {(String.IsNullOrEmpty(user.Mail) ? "..." : user.Mail)}{Environment.NewLine}", Color.Red);
                                }
                            }
                            else
                            {
                                user.ParsedBy = 0;

                                this.AddUsersColorLogAsync($"Не удалось найти пользователя с телефонным номером {(String.IsNullOrEmpty(user.Phone) ? "..." : user.Phone)} или почтой {(String.IsNullOrEmpty(user.Mail) ? "..." : user.Mail)}{Environment.NewLine}", Color.Red);
                            }
                        } //если не найдено ищем по телефону
                        else if (!resumeFound && !String.IsNullOrEmpty(user.LastResume?.Link))
                        {
                            var result = this.GetPhoneAndMailFromResume(user);
                            if (result == 1 || result == 2)
                            {
                                this.FindLastResumeByMailorPhone(user, 0);
                            }
                            else if (result == 3)
                            {
                                user.ParsedBy = 2;

                                this.FindLastResumeByMailorPhone(user, 1);
                            }
                            else
                            {
                                user.ParsedBy = 0;
                                this.AddUsersColorLogAsync($"Не удалось найти пользователя с телефонным номером {(String.IsNullOrEmpty(user.Phone) ? "..." : user.Phone)} или почтой {(String.IsNullOrEmpty(user.Mail) ? "..." : user.Mail)}{Environment.NewLine}", Color.Red);
                            }
                        } //если не найдено ищем по ссылке на резюме
                        else if (!resumeFound)
                        {
                            user.ParsedBy = 0;
                            this.AddUsersColorLogAsync($"Не удалось найти пользователя с телефонным номером {(String.IsNullOrEmpty(user.Phone) ? "..." : user.Phone)} или почтой {(String.IsNullOrEmpty(user.Mail) ? "..." : user.Mail)}{Environment.NewLine}", Color.Red);
                        }
                    }
                    else if (!String.IsNullOrEmpty(user.Phone))//Если есть телефон
                    {
                        var found = this.FindLastResumeByMailorPhone(user, 1);
                        user.ParsedBy = 2;
                        if (!found && !String.IsNullOrEmpty(user.LastResume?.Link))
                        {
                            var result = this.GetPhoneAndMailFromResume(user);
                            if (result == 1 || result == 2)
                            {
                                this.FindLastResumeByMailorPhone(user, 0);
                            }
                            else if (result == 3)
                            {
                                user.ParsedBy = 2;

                                this.FindLastResumeByMailorPhone(user, 1);
                            }
                            else
                            {
                                user.ParsedBy = 0;
                                this.AddUsersColorLogAsync($"Не удалось найти пользователя с телефонным номером {(String.IsNullOrEmpty(user.Phone) ? "..." : user.Phone)} или почтой {(String.IsNullOrEmpty(user.Mail) ? "..." : user.Mail)}{Environment.NewLine}", Color.Red);
                            }
                        }
                        else if (!found)
                        {
                            user.ParsedBy = 0;
                            this.AddUsersColorLogAsync($"Не удалось найти пользователя с телефонным номером {(String.IsNullOrEmpty(user.Phone) ? "..." : user.Phone)} или почтой {(String.IsNullOrEmpty(user.Mail) ? "..." : user.Mail)}{Environment.NewLine}", Color.Red);
                        }
                    }
                    else if (user.LastResume != null) //если есть ссылка на резюме
                    {
                        this.AddUsersColorLogAsync($"Попытка получить телефон и почту пользователя по ссылке {user.LastResume.Link} на резюме{Environment.NewLine}", Color.Green);
                        var result = this.GetPhoneAndMailFromResume(user);
                        user.ParsedBy = 3;
                        if (result == 1 || result == 2)
                        {
                            this.FindLastResumeByMailorPhone(user, 0);
                        }
                        else if (result == 3)
                        {
                            this.FindLastResumeByMailorPhone(user, 1);
                        }
                        else
                        {
                            user.ParsedBy = 0;
                            this.AddUsersColorLogAsync($"Не удалось найти резюме {user.LastResume.Link}{Environment.NewLine}", Color.Red);
                        }
                    }
                    user.ResumeFound = r == user.LastResume ? "-" : "+";
                    this.IncrementUsersProgress();
                    Application.DoEvents();
                }
                this.SetUsersProgressStatusLabelText("Парсинг завершен");
                this.ResetUsersProgressAndSetMaximum(0);
            });
            this._userparsingisinprocess = false;
            if (this.notifier == null)
            {
                this.notifier = new PopupNotifier
                {
                    Image = Properties.Resources.Info_Button_256,
                    TitleText = "HH Parser",
                    TitleFont = new Font("Segui UI", 16, FontStyle.Regular, GraphicsUnit.Pixel),
                    TitleColor = Color.Black,
                    AnimationInterval = 1,
                    AnimationDuration = 1000,
                    Delay = 20000,
                    ContentFont = new Font("Segui UI", 14, FontStyle.Regular, GraphicsUnit.Pixel),
                    ContentText = "Парсинг пользователей завершен!",
                    ShowCloseButton = true,
                    ImageSize = new Size(35, 35),
                    ImagePadding = new Padding(3, 3, 0, 0),
                    Size = new Size(400, 90),
                    HeaderColor = Color.SlateGray
                };
                this.notifier.Popup();
            }
            else
            {
                this.notifier.Hide();
                this.notifier.Popup();
            }
        }
        public static string StringToDate(string source)
        {
            var sp = source.Split(' ');
            var month = "01";
            switch (sp[0].ToLower())
            {
                case "январь":
                    {
                        month = "01";
                        break;
                    }
                case "февраль":
                    {
                        month = "02";
                        break;
                    }
                case "март":
                    {
                        month = "03";
                        break;
                    }
                case "апрель":
                    {
                        month = "04";
                        break;
                    }
                case "май":
                    {
                        month = "05";
                        break;
                    }
                case "июнь":
                    {
                        month = "06";
                        break;
                    }
                case "июль":
                    {
                        month = "07";
                        break;
                    }
                case "август":
                    {
                        month = "08";
                        break;
                    }
                case "сентябрь":
                    {
                        month = "09";
                        break;
                    }
                case "октябрь":
                    {
                        month = "10";
                        break;
                    }
                case "ноябрь":
                    {
                        month = "11";
                        break;
                    }
                case "декабрь":
                    {
                        month = "12";
                        break;
                    }
            }
            return $"01.{month}.{sp[1]} 00:00";
        }
        public static string StringToDatetime(string source)
        {
            try
            {
                var sp = source.Split(' ');
                var day = sp[2];
                if (day.Length == 1)
                {
                    day = '0' + day;
                }
                var month = "";
                switch (sp[3].Replace(",", ""))
                {
                    case "января":
                        {
                            month = "01";
                            break;
                        }
                    case "февраля":
                        {
                            month = "02";
                            break;
                        }
                    case "марта":
                        {
                            month = "03";
                            break;
                        }
                    case "апреля":
                        {
                            month = "04";
                            break;
                        }
                    case "мая":
                        {
                            month = "05";
                            break;
                        }
                    case "июня":
                        {
                            month = "06";
                            break;
                        }
                    case "июля":
                        {
                            month = "07";
                            break;
                        }
                    case "августа":
                        {
                            month = "08";
                            break;
                        }
                    case "сентября":
                        {
                            month = "09";
                            break;
                        }
                    case "октября":
                        {
                            month = "10";
                            break;
                        }
                    case "ноября":
                        {
                            month = "11";
                            break;
                        }
                    case "декабря":
                        {
                            month = "12";
                            break;
                        }
                }
                var time = sp[Int32.TryParse(sp[4].Replace(",", ""), out var result) ? 5 : 4];
                return Int32.TryParse(sp[4].Replace(",", ""), out result)
                    ? $"{day}.{month}.{sp[4].Replace(",", "")} {time}"
                    : $"{day}.{month}.2019 {time}";
            }
            catch
            {
                return DateTime.Today.ToString();
            }
        }
        private void WorkTable_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            this._companiesWorkTime = "";
            var indexes = this.WorkTable.CheckedIndices.Cast<int>().ToList();
            if (e.NewValue == CheckState.Checked)
            {
                indexes.Add(e.Index);
            }
            else if (e.NewValue == CheckState.Unchecked)
            {
                indexes.Remove(e.Index);
            }
            if (indexes.Contains(0))
            {
                this._companiesWorkTime += "schedule=fullDay&";
            }
            if (indexes.Contains(1))
            {
                this._companiesWorkTime += "schedule=shift&";
            }
            if (indexes.Contains(2))
            {
                this._companiesWorkTime += "schedule=flexible&";
            }
            if (indexes.Contains(3))
            {
                this._companiesWorkTime += "schedule=remote&";
            }
            if (indexes.Contains(4))
            {
                this._companiesWorkTime += "schedule=flyInFlyOut&";
            }
            if (!String.IsNullOrEmpty(this._companiesWorkTime))
            {
                this._companiesWorkTime = this._companiesWorkTime.Substring(0, this._companiesWorkTime.Length - 1);
            }
        }
        private void SaveOGRN_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                Filter = "xlsx|*.xlsx",
                FileName = "Результат парсинга компаний"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var sheet = this._workbook.GetSheetAt(0);
                var count = sheet.LastRowNum;
                for (var i = 0; i <= count; i++)
                {
                    var row = sheet.GetRow(i);
                    row.CreateCell(10);
                    row.CreateCell(11);
                    row.CreateCell(12);
                    row.CreateCell(13);
                    row.CreateCell(14);
                    row.CreateCell(15);
                }
                var headerrow = sheet.GetRow(0);
                headerrow.GetCell(10).SetCellValue("ОГРН");
                headerrow.GetCell(11).SetCellValue("Сред.Численность персонала");
                headerrow.GetCell(12).SetCellValue("Дата регистрации");
                headerrow.GetCell(13).SetCellValue("Выручка");
                headerrow.GetCell(14).SetCellValue("Кол-во судебных дел");
                headerrow.GetCell(15).SetCellValue("Статус");
                for (var i = 1; i <= count; i++)
                {
                    var row = sheet.GetRow(i);
                    row.GetCell(10).SetCellValue(this._ogrnCompanies[i - 1].Ogrn);
                    row.GetCell(11).SetCellValue(this._ogrnCompanies[i - 1].PersonalAverage);
                    row.GetCell(12).SetCellValue(this._ogrnCompanies[i - 1].RegistrationDate);
                    row.GetCell(13).SetCellValue(this._ogrnCompanies[i - 1].Revenue);
                    row.GetCell(14).SetCellValue(this._ogrnCompanies[i - 1].CasesCount);
                    row.GetCell(15).SetCellValue(this._ogrnCompanies[i - 1].Status);
                }
                for (var i = 0; i <= 15; i++)
                {
                    sheet.AutoSizeColumn(i);
                }
                var fs = new FileStream(sfd.FileName, FileMode.Create);
                this._workbook.Write(fs);
            }
        }
        private void SetStyleToAll(IRow cell, ICellStyle style, int count)
        {
            for (var i = 0; i <= count; i++)
            {
                cell.GetCell(i).CellStyle = style;
            }
        }
        private void SaveResumeParsingByCompanyResults_Click(object sender, EventArgs e)
        {
            var sfd = new FolderBrowserDialog()
            {
            };
            if (sfd.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            foreach (var company in this.companyItems)
            {
                var workbook = new XSSFWorkbook();
                var sheet1 = workbook.CreateSheet("Обложка");
                var sheet2 = workbook.CreateSheet("Данные для просмотра и печати");
                var sheet3 = workbook.CreateSheet("Данные");
                var sheet4 = workbook.CreateSheet("Статистика");
                var today = DateTime.Today;

                var NotWorkingUsers = company.CompanyResumes.Where(t => !t.WorkingNow).ToList();
                var working = company.CompanyResumes.Where(t => t.WorkedPlaces[0].End.Date == today.Date).ToList();
                var TwoWeeks = working.Where(t => today.Subtract(t.ResumeUpdate).TotalDays <= 14).ToList();
                var TwoWeekToThreeMonth = working.Where(t => today.Subtract(t.ResumeUpdate).TotalDays > 14 && today.Subtract(t.ResumeUpdate).TotalDays <= 90).ToList();
                var ThreeMonthPlus = working.Where(t => today.Subtract(t.ResumeUpdate).TotalDays > 90).ToList();
                {
                    for (var i = 0; i < 25; i++)
                    {
                        var row = sheet1.CreateRow(i);
                        for (var z = 0; z <= 5; z++)
                        {
                            row.CreateCell(z);
                        }
                    }
                    var cellstyle = workbook.CreateCellStyle();
                    var font = workbook.CreateFont();
                    font.FontHeight = 11;
                    font.IsBold = true;
                    cellstyle.SetFont(font);
                    sheet1.GetRow(0).GetCell(1).CellStyle = cellstyle;
                    sheet1.GetRow(10).GetCell(1).CellStyle = cellstyle;
                    sheet1.GetRow(16).GetCell(1).CellStyle = cellstyle;
                    sheet1.GetRow(0).GetCell(1).SetCellValue("На что следует обратисть внимание:");
                    sheet1.GetRow(1).GetCell(1).SetCellValue("Смотрите вкладку Данные для просмотра и печати");
                    sheet1.GetRow(2).GetCell(1).SetCellValue("Обратите внимание на города и названия компаний, в случае, если в отчет попали города и названия не имеющие к вам ");
                    sheet1.GetRow(3).GetCell(1).SetCellValue("отношения - исключите их через фитьтр.");
                    sheet1.GetRow(4).GetCell(1).SetCellValue("Если вы видите, что ФИО сотрудника скрыты, значит, скорее всего, это резюме скрыто и от поиска Вашей компанией");
                    sheet1.GetRow(5).GetCell(1).SetCellValue("Обратите наибольшее  внимание на сотрудников, недавно обновивших резюме и работающих в компании!");
                    sheet1.GetRow(6).GetCell(1).SetCellValue("Если сотрудник давно обновил резюме - вероятно сейчас он активно не ищет работу, но его могут попытаться перемонить.");
                    sheet1.GetRow(7).GetCell(1).SetCellValue("Сотрудники, которые ранее работали в Вашей компании и ищут работу - это интересная информация, а иногда и отличные ");
                    sheet1.GetRow(8).GetCell(1).SetCellValue("кандидаты, способные быстро войти в курс дела.");
                    sheet1.GetRow(10).GetCell(1).SetCellValue("Как использовать настоящий отчет:");
                    sheet1.GetRow(11).GetCell(1).SetCellValue("Важно понять -");
                    sheet1.GetRow(12).GetCell(1).SetCellValue("1. Подтвердить что резюме размещены для смены работы.");
                    sheet1.GetRow(13).GetCell(1).SetCellValue("2. Понять причины такого желания");
                    sheet1.GetRow(14).GetCell(1).SetCellValue("3. Оценить риски и принять решение направленное на удержание этого сотрудника или снижение ущерба его ухода.");
                    sheet1.GetRow(16).GetCell(1).SetCellValue("Возможно ли, что отчет содержит лишние данные?");
                    sheet1.GetRow(17).GetCell(1).SetCellValue("Возможно, это экспресс отчет и его возможности ограничены.");
                    sheet1.GetRow(18).GetCell(1).SetCellValue("Так же, по нашему опыту, поиск HRSave находит на 20-50% больше резюме, благодаря используемым алгоритмам поиска.");
                }
                {
                    for (var i = 0; i < company.CompanyResumes.Count + 5; i++)
                    {
                        var row = sheet2.CreateRow(i);
                        for (var z = 0; z <= 12; z++)
                        {
                            row.CreateCell(z);
                        }
                    }
                    sheet2.GetRow(0).GetCell(0).SetCellValue("Имя");
                    sheet2.GetRow(0).GetCell(1).SetCellValue("Лет");
                    sheet2.GetRow(0).GetCell(2).SetCellValue("Стаж");
                    sheet2.GetRow(0).GetCell(3).SetCellValue("Город");
                    sheet2.GetRow(0).GetCell(4).SetCellValue("Должность");
                    sheet2.GetRow(0).GetCell(5).SetCellValue("Зарплата");
                    sheet2.GetRow(0).GetCell(6).SetCellValue("Валюта");
                    sheet2.GetRow(0).GetCell(7).SetCellValue("Обновления резюме");
                    sheet2.GetRow(0).GetCell(8).SetCellValue("Работает сейчас");
                    sheet2.GetRow(0).GetCell(9).SetCellValue("Дата начала работы в компании");
                    sheet2.GetRow(0).GetCell(10).SetCellValue("Работал до");
                    sheet2.GetRow(0).GetCell(11).SetCellValue("Ссылка");
                    sheet2.GetRow(0).GetCell(12).SetCellValue("Место работы в резюме");


                    var cellstyle = workbook.CreateCellStyle();
                    cellstyle.FillForegroundColor = IndexedColors.DarkRed.Index;
                    cellstyle.FillPattern = FillPattern.SolidForeground;
                    var font = workbook.CreateFont();
                    font.Color = IndexedColors.Yellow.Index;
                    font.FontHeight = 15;
                    cellstyle.SetFont(font);
                    this.SetStyleToAll(sheet2.GetRow(1), cellstyle, 12);
                    sheet2.GetRow(1).GetCell(0).SetCellValue($"Сотрудник работает в компании, обновил резюме менее 2 недель назад - {TwoWeeks.Count} резюме");
                    var rowindex = 2;
                    var underlinecellstyle = workbook.CreateCellStyle();
                    var underlinefont = workbook.CreateFont();
                    underlinefont.Color = IndexedColors.LightBlue.Index;
                    underlinefont.Underline = FontUnderlineType.Single;
                    underlinecellstyle.SetFont(underlinefont);
                    for (var i = 0; i < TwoWeeks.Count; i++)
                    {
                        sheet2.GetRow(rowindex).GetCell(0).SetCellValue(TwoWeeks[i].OwnerName);
                        sheet2.GetRow(rowindex).GetCell(1).SetCellValue(TwoWeeks[i].Age);
                        sheet2.GetRow(rowindex).GetCell(2).SetCellType(CellType.Numeric);
                        sheet2.GetRow(rowindex).GetCell(2).SetCellValue(TwoWeeks[i].WorkingSummary);
                        sheet2.GetRow(rowindex).GetCell(3).SetCellValue(TwoWeeks[i].WorkingCity);
                        sheet2.GetRow(rowindex).GetCell(4).SetCellValue(TwoWeeks[i].Position);
                        sheet2.GetRow(rowindex).GetCell(5).SetCellType(CellType.Numeric);
                        sheet2.GetRow(rowindex).GetCell(5).SetCellValue(TwoWeeks[i].Salary);
                        sheet2.GetRow(rowindex).GetCell(6).SetCellValue(TwoWeeks[i].SalaryCurrency);
                        if (TwoWeeks[i].ResumeUpdate != DateTime.MinValue)
                        {
                            var cell = sheet2.GetRow(rowindex).GetCell(7);
                            var format = workbook.CreateDataFormat();
                            var cellStyle = workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(TwoWeeks[i].ResumeUpdate);
                        }
                        else
                        {
                            sheet2.GetRow(rowindex).GetCell(7).SetCellValue("-");
                        }
                        sheet2.GetRow(rowindex).GetCell(8).SetCellValue(true);
                        if (TwoWeeks[i].Start != DateTime.MinValue)
                        {
                            var cell = sheet2.GetRow(rowindex).GetCell(9);
                            var format = workbook.CreateDataFormat();
                            var cellStyle = workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(TwoWeeks[i].Start);
                        }
                        else
                        {
                            sheet2.GetRow(rowindex).GetCell(9).SetCellValue("-");
                        }
                        if (TwoWeeks[i].End != DateTime.MinValue)
                        {
                            var cell = sheet2.GetRow(rowindex).GetCell(10);
                            var format = workbook.CreateDataFormat();
                            var cellStyle = workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(TwoWeeks[i].End);
                        }
                        else
                        {
                            sheet2.GetRow(rowindex).GetCell(10).SetCellValue("-");
                        }
                        var targetLink = new XSSFHyperlink(HyperlinkType.Url)
                        {
                            Address = TwoWeeks[i].Link
                        };
                        sheet2.GetRow(rowindex).GetCell(11).Hyperlink = targetLink;
                        sheet2.GetRow(rowindex).GetCell(11).SetCellValue("Ссылка");
                        sheet2.GetRow(rowindex).GetCell(11).CellStyle = underlinecellstyle;
                        sheet2.GetRow(rowindex).GetCell(11).SetAsActiveCell();
                        sheet2.GetRow(rowindex).GetCell(12).SetCellValue(TwoWeeks[i].WorkedPlaces[0].Name);
                        rowindex++;
                    }
                    this.SetStyleToAll(sheet2.GetRow(rowindex), cellstyle, 12);
                    sheet2.GetRow(rowindex++).GetCell(0).SetCellValue($"Сотрудник работает в компании, обновил резюме от 2 недель до 3 месяцев - {TwoWeekToThreeMonth.Count} резюме");
                    for (var i = 0; i < TwoWeekToThreeMonth.Count; i++)
                    {
                        sheet2.GetRow(rowindex).GetCell(0).SetCellValue(TwoWeekToThreeMonth[i].OwnerName);
                        sheet2.GetRow(rowindex).GetCell(1).SetCellValue(TwoWeekToThreeMonth[i].Age);
                        sheet2.GetRow(rowindex).GetCell(2).SetCellType(CellType.Numeric);
                        sheet2.GetRow(rowindex).GetCell(2).SetCellValue(TwoWeekToThreeMonth[i].WorkingSummary);
                        sheet2.GetRow(rowindex).GetCell(3).SetCellValue(TwoWeekToThreeMonth[i].WorkingCity);
                        sheet2.GetRow(rowindex).GetCell(4).SetCellValue(TwoWeekToThreeMonth[i].Position);
                        sheet2.GetRow(rowindex).GetCell(5).SetCellType(CellType.Numeric);
                        sheet2.GetRow(rowindex).GetCell(5).SetCellValue(TwoWeekToThreeMonth[i].Salary);
                        sheet2.GetRow(rowindex).GetCell(6).SetCellValue(TwoWeekToThreeMonth[i].SalaryCurrency);
                        if (TwoWeekToThreeMonth[i].ResumeUpdate != DateTime.MinValue)
                        {
                            var cell = sheet2.GetRow(rowindex).GetCell(7);
                            var format = workbook.CreateDataFormat();
                            var cellStyle = workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(TwoWeekToThreeMonth[i].ResumeUpdate);
                        }
                        else
                        {
                            sheet2.GetRow(rowindex).GetCell(7).SetCellValue("-");
                        }
                        sheet2.GetRow(rowindex).GetCell(8).SetCellValue(true);
                        if (TwoWeekToThreeMonth[i].Start != DateTime.MinValue)
                        {
                            var cell = sheet2.GetRow(rowindex).GetCell(9);
                            var format = workbook.CreateDataFormat();
                            var cellStyle = workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(TwoWeekToThreeMonth[i].Start);
                        }
                        else
                        {
                            sheet2.GetRow(rowindex).GetCell(9).SetCellValue("-");
                        }

                        if (TwoWeekToThreeMonth[i].End != DateTime.MinValue)
                        {
                            var cell = sheet2.GetRow(rowindex).GetCell(10);
                            var format = workbook.CreateDataFormat();
                            var cellStyle = workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(TwoWeekToThreeMonth[i].End);
                        }
                        else
                        {
                            sheet2.GetRow(rowindex).GetCell(10).SetCellValue("-");
                        }
                        var targetLink = new XSSFHyperlink(HyperlinkType.Url)
                        {
                            Address = TwoWeekToThreeMonth[i].Link
                        };
                        sheet2.GetRow(rowindex).GetCell(11).Hyperlink = targetLink;
                        sheet2.GetRow(rowindex).GetCell(11).SetCellValue("Ссылка");
                        sheet2.GetRow(rowindex).GetCell(11).CellStyle = underlinecellstyle;
                        sheet2.GetRow(rowindex).GetCell(11).SetAsActiveCell();
                        sheet2.GetRow(rowindex).GetCell(12).SetCellValue(TwoWeekToThreeMonth[i].WorkedPlaces[0].Name);
                        rowindex++;
                    }
                    this.SetStyleToAll(sheet2.GetRow(rowindex), cellstyle, 12);
                    sheet2.GetRow(rowindex++).GetCell(0).SetCellValue($"Указано что сотрудник работает в компании, обновил резюме более 3 месяцев назад - {ThreeMonthPlus.Count} резюме");
                    for (var i = 0; i < ThreeMonthPlus.Count; i++)
                    {
                        sheet2.GetRow(rowindex).GetCell(0).SetCellValue(ThreeMonthPlus[i].OwnerName);
                        sheet2.GetRow(rowindex).GetCell(1).SetCellValue(ThreeMonthPlus[i].Age);
                        sheet2.GetRow(rowindex).GetCell(2).SetCellType(CellType.Numeric);
                        sheet2.GetRow(rowindex).GetCell(2).SetCellValue(ThreeMonthPlus[i].WorkingSummary);
                        sheet2.GetRow(rowindex).GetCell(3).SetCellValue(ThreeMonthPlus[i].WorkingCity);
                        sheet2.GetRow(rowindex).GetCell(4).SetCellValue(ThreeMonthPlus[i].Position);
                        sheet2.GetRow(rowindex).GetCell(5).SetCellType(CellType.Numeric);
                        sheet2.GetRow(rowindex).GetCell(5).SetCellValue(ThreeMonthPlus[i].Salary);
                        sheet2.GetRow(rowindex).GetCell(6).SetCellValue(ThreeMonthPlus[i].SalaryCurrency);
                        if (ThreeMonthPlus[i].ResumeUpdate != DateTime.MinValue)
                        {
                            var cell = sheet2.GetRow(rowindex).GetCell(7);
                            var format = workbook.CreateDataFormat();
                            var cellStyle = workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(ThreeMonthPlus[i].ResumeUpdate);
                        }
                        else
                        {
                            sheet2.GetRow(rowindex).GetCell(7).SetCellValue("-");
                        }
                        sheet2.GetRow(rowindex).GetCell(8).SetCellValue(true);
                        if (ThreeMonthPlus[i].Start != DateTime.MinValue)
                        {
                            var cell = sheet2.GetRow(rowindex).GetCell(9);
                            var format = workbook.CreateDataFormat();
                            var cellStyle = workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(ThreeMonthPlus[i].Start);
                        }
                        else
                        {
                            sheet2.GetRow(rowindex).GetCell(9).SetCellValue("-");
                        }

                        if (ThreeMonthPlus[i].End != DateTime.MinValue)
                        {
                            var cell = sheet2.GetRow(rowindex).GetCell(10);
                            var format = workbook.CreateDataFormat();
                            var cellStyle = workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(ThreeMonthPlus[i].End);
                        }
                        else
                        {
                            sheet2.GetRow(rowindex).GetCell(10).SetCellValue("-");
                        }
                        var targetLink = new XSSFHyperlink(HyperlinkType.Url)
                        {
                            Address = ThreeMonthPlus[i].Link
                        };
                        sheet2.GetRow(rowindex).GetCell(11).Hyperlink = targetLink;
                        sheet2.GetRow(rowindex).GetCell(11).SetCellValue("Ссылка");
                        sheet2.GetRow(rowindex).GetCell(11).CellStyle = underlinecellstyle;
                        sheet2.GetRow(rowindex).GetCell(11).SetAsActiveCell();
                        sheet2.GetRow(rowindex).GetCell(12).SetCellValue(ThreeMonthPlus[i].WorkedPlaces[0].Name);
                        rowindex++;
                    }
                    this.SetStyleToAll(sheet2.GetRow(rowindex), cellstyle, 12);
                    sheet2.GetRow(rowindex++).GetCell(0).SetCellValue($"Сотрудник работал ранее в компании, сейчас ищет работу - {NotWorkingUsers.Count} резюме, упорядочены по дате обновления резюме");
                    for (var i = 0; i < NotWorkingUsers.Count; i++)
                    {
                        sheet2.GetRow(rowindex).GetCell(0).SetCellValue(NotWorkingUsers[i].OwnerName);
                        sheet2.GetRow(rowindex).GetCell(1).SetCellValue(NotWorkingUsers[i].Age);
                        sheet2.GetRow(rowindex).GetCell(2).SetCellType(CellType.Numeric);
                        sheet2.GetRow(rowindex).GetCell(2).SetCellValue(NotWorkingUsers[i].WorkingSummary);
                        sheet2.GetRow(rowindex).GetCell(3).SetCellValue(NotWorkingUsers[i].WorkingCity);
                        sheet2.GetRow(rowindex).GetCell(4).SetCellValue(NotWorkingUsers[i].Position);
                        sheet2.GetRow(rowindex).GetCell(5).SetCellType(CellType.Numeric);
                        sheet2.GetRow(rowindex).GetCell(5).SetCellValue(NotWorkingUsers[i].Salary);
                        sheet2.GetRow(rowindex).GetCell(6).SetCellValue(NotWorkingUsers[i].SalaryCurrency);
                        if (NotWorkingUsers[i].ResumeUpdate != DateTime.MinValue)
                        {
                            var cell = sheet2.GetRow(rowindex).GetCell(7);
                            var format = workbook.CreateDataFormat();
                            var cellStyle = workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(NotWorkingUsers[i].ResumeUpdate);
                        }
                        else
                        {
                            sheet2.GetRow(rowindex).GetCell(7).SetCellValue("-");
                        }
                        sheet2.GetRow(rowindex).GetCell(8).SetCellValue(false);
                        if (NotWorkingUsers[i].Start != DateTime.MinValue)
                        {
                            var cell = sheet2.GetRow(rowindex).GetCell(9);
                            var format = workbook.CreateDataFormat();
                            var cellStyle = workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(NotWorkingUsers[i].Start);
                        }
                        else
                        {
                            sheet2.GetRow(rowindex).GetCell(9).SetCellValue("-");
                        }

                        if (NotWorkingUsers[i].End != DateTime.MinValue)
                        {
                            var cell = sheet2.GetRow(rowindex).GetCell(10);
                            var format = workbook.CreateDataFormat();
                            var cellStyle = workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(NotWorkingUsers[i].End);
                        }
                        else
                        {
                            sheet2.GetRow(rowindex).GetCell(10).SetCellValue("-");
                        }
                        var targetLink = new XSSFHyperlink(HyperlinkType.Url)
                        {
                            Address = NotWorkingUsers[i].Link
                        };
                        sheet2.GetRow(rowindex).GetCell(11).Hyperlink = targetLink;
                        sheet2.GetRow(rowindex).GetCell(11).SetCellValue("Ссылка");
                        sheet2.GetRow(rowindex).GetCell(11).CellStyle = underlinecellstyle;
                        sheet2.GetRow(rowindex).GetCell(11).SetAsActiveCell();
                        sheet2.GetRow(rowindex).GetCell(12).SetCellValue(NotWorkingUsers[i].WorkedPlaces[0].Name);
                        rowindex++;
                    }
                    sheet2.SetColumnWidth(0, 5384);  //A
                    sheet2.SetColumnWidth(1, 769);   //B
                    sheet2.SetColumnWidth(2, 5);     //C
                    sheet2.SetColumnWidth(3, 3382);  //D
                    sheet2.SetColumnWidth(4, 8691);  //E
                    sheet2.SetColumnWidth(5, 1615);  //F
                    sheet2.SetColumnWidth(6, 350);   //G
                    sheet2.SetColumnWidth(7, 2700);  //H
                    sheet2.SetColumnWidth(8, 5);     //I
                    sheet2.SetColumnWidth(9, 5);  //J
                    sheet2.SetColumnWidth(10, 2613); //K
                    sheet2.SetColumnWidth(11, 1921);//L
                    sheet2.SetColumnWidth(12, 12535);//M
                }
                {
                    for (var i = 0; i < company.CompanyResumes.Count + 5; i++)
                    {
                        var row = sheet3.CreateRow(i);
                        for (var z = 0; z <= 12; z++)
                        {
                            row.CreateCell(z);
                        }
                    }
                    sheet3.GetRow(0).GetCell(0).SetCellValue("Имя");
                    sheet3.GetRow(0).GetCell(1).SetCellValue("Возраст");
                    sheet3.GetRow(0).GetCell(2).SetCellValue("Стаж");
                    sheet3.GetRow(0).GetCell(3).SetCellValue("Город");
                    sheet3.GetRow(0).GetCell(4).SetCellValue("Должность");
                    sheet3.GetRow(0).GetCell(5).SetCellValue("Зарплата");
                    sheet3.GetRow(0).GetCell(6).SetCellValue("Валюта");
                    sheet3.GetRow(0).GetCell(7).SetCellValue("Дата обновления резюме");
                    sheet3.GetRow(0).GetCell(8).SetCellValue("Работает сейчас");
                    sheet3.GetRow(0).GetCell(9).SetCellValue("Дата начала работы в компании");
                    sheet3.GetRow(0).GetCell(10).SetCellValue("Дата окончания работы в компании");
                    sheet3.GetRow(0).GetCell(11).SetCellValue("Ссылка");
                    sheet3.GetRow(0).GetCell(12).SetCellValue("Место работы в резюме");


                    var cellstyle = workbook.CreateCellStyle();
                    cellstyle.FillForegroundColor = IndexedColors.DarkRed.Index;
                    cellstyle.FillPattern = FillPattern.SolidForeground;
                    var font = workbook.CreateFont();
                    font.Color = IndexedColors.Yellow.Index;
                    font.FontHeight = 15;
                    cellstyle.SetFont(font);
                    this.SetStyleToAll(sheet3.GetRow(1), cellstyle, 12);
                    sheet3.GetRow(1).GetCell(0).SetCellValue($"0 - 2 недели...{TwoWeeks.Count} резюме");
                    var rowindex = 2;

                    for (var i = 0; i < TwoWeeks.Count; i++)
                    {
                        sheet3.GetRow(rowindex).GetCell(0).SetCellValue(TwoWeeks[i].OwnerName);
                        sheet3.GetRow(rowindex).GetCell(1).SetCellValue(TwoWeeks[i].Age);
                        sheet3.GetRow(rowindex).GetCell(2).SetCellType(CellType.Numeric);
                        sheet3.GetRow(rowindex).GetCell(2).SetCellValue(TwoWeeks[i].WorkingSummary);
                        sheet3.GetRow(rowindex).GetCell(3).SetCellValue(TwoWeeks[i].WorkingCity);
                        sheet3.GetRow(rowindex).GetCell(4).SetCellValue(TwoWeeks[i].Position);
                        sheet3.GetRow(rowindex).GetCell(5).SetCellType(CellType.Numeric);
                        sheet3.GetRow(rowindex).GetCell(5).SetCellValue(TwoWeeks[i].Salary);
                        sheet3.GetRow(rowindex).GetCell(6).SetCellValue(TwoWeeks[i].SalaryCurrency);
                        if (TwoWeeks[i].ResumeUpdate != DateTime.MinValue)
                        {
                            var cell = sheet3.GetRow(rowindex).GetCell(7);
                            var format = workbook.CreateDataFormat();
                            var cellStyle = workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(TwoWeeks[i].ResumeUpdate);
                        }
                        else
                        {
                            sheet3.GetRow(rowindex).GetCell(7).SetCellValue("-");
                        }
                        sheet3.GetRow(rowindex).GetCell(8).SetCellValue(true);
                        if (TwoWeeks[i].Start != DateTime.MinValue)
                        {
                            var cell = sheet3.GetRow(rowindex).GetCell(9);
                            var format = workbook.CreateDataFormat();
                            var cellStyle = workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(TwoWeeks[i].Start);
                        }
                        else
                        {
                            sheet3.GetRow(rowindex).GetCell(9).SetCellValue("-");
                        }

                        if (TwoWeeks[i].End != DateTime.MinValue)
                        {
                            var cell = sheet3.GetRow(rowindex).GetCell(10);
                            var format = workbook.CreateDataFormat();
                            var cellStyle = workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(TwoWeeks[i].End);
                        }
                        else
                        {
                            sheet3.GetRow(rowindex).GetCell(10).SetCellValue("-");
                        }
                        sheet3.GetRow(rowindex).GetCell(11).SetCellValue(TwoWeeks[i].Link);
                        sheet3.GetRow(rowindex).GetCell(12).SetCellValue(TwoWeeks[i].WorkedPlaces[0].Name);
                        rowindex++;
                    }
                    this.SetStyleToAll(sheet3.GetRow(rowindex), cellstyle, 12);
                    sheet3.GetRow(rowindex++).GetCell(0).SetCellValue($"2 недели - 3 месяца...{TwoWeekToThreeMonth.Count} резюме");

                    for (var i = 0; i < TwoWeekToThreeMonth.Count; i++)
                    {
                        sheet3.GetRow(rowindex).GetCell(0).SetCellValue(TwoWeekToThreeMonth[i].OwnerName);
                        sheet3.GetRow(rowindex).GetCell(1).SetCellValue(TwoWeekToThreeMonth[i].Age);
                        sheet3.GetRow(rowindex).GetCell(2).SetCellType(CellType.Numeric);
                        sheet3.GetRow(rowindex).GetCell(2).SetCellValue(TwoWeekToThreeMonth[i].WorkingSummary);
                        sheet3.GetRow(rowindex).GetCell(3).SetCellValue(TwoWeekToThreeMonth[i].WorkingCity);
                        sheet3.GetRow(rowindex).GetCell(4).SetCellValue(TwoWeekToThreeMonth[i].Position);
                        sheet3.GetRow(rowindex).GetCell(5).SetCellType(CellType.Numeric);
                        sheet3.GetRow(rowindex).GetCell(5).SetCellValue(TwoWeekToThreeMonth[i].Salary);
                        sheet3.GetRow(rowindex).GetCell(6).SetCellValue(TwoWeekToThreeMonth[i].SalaryCurrency);
                        if (TwoWeekToThreeMonth[i].ResumeUpdate != DateTime.MinValue)
                        {
                            var cell = sheet3.GetRow(rowindex).GetCell(7);
                            var format = workbook.CreateDataFormat();
                            var cellStyle = workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(TwoWeekToThreeMonth[i].ResumeUpdate);
                        }
                        else
                        {
                            sheet3.GetRow(rowindex).GetCell(7).SetCellValue("-");
                        }
                        sheet3.GetRow(rowindex).GetCell(8).SetCellValue(true);
                        if (TwoWeekToThreeMonth[i].Start != DateTime.MinValue)
                        {
                            var cell = sheet3.GetRow(rowindex).GetCell(9);
                            var format = workbook.CreateDataFormat();
                            var cellStyle = workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(TwoWeekToThreeMonth[i].Start);
                        }
                        else
                        {
                            sheet3.GetRow(rowindex).GetCell(9).SetCellValue("-");
                        }

                        if (TwoWeekToThreeMonth[i].End != DateTime.MinValue)
                        {
                            var cell = sheet3.GetRow(rowindex).GetCell(10);
                            var format = workbook.CreateDataFormat();
                            var cellStyle = workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(TwoWeekToThreeMonth[i].End);
                        }
                        else
                        {
                            sheet3.GetRow(rowindex).GetCell(10).SetCellValue("-");
                        }
                        sheet3.GetRow(rowindex).GetCell(11).SetCellValue(TwoWeekToThreeMonth[i].Link);
                        sheet3.GetRow(rowindex).GetCell(12).SetCellValue(TwoWeekToThreeMonth[i].WorkedPlaces[0].Name);
                        rowindex++;
                    }
                    this.SetStyleToAll(sheet3.GetRow(rowindex), cellstyle, 12);
                    sheet3.GetRow(rowindex++).GetCell(0).SetCellValue($"3 месяца - N...{ThreeMonthPlus.Count} резюме");
                    for (var i = 0; i < ThreeMonthPlus.Count; i++)
                    {
                        sheet3.GetRow(rowindex).GetCell(0).SetCellValue(ThreeMonthPlus[i].OwnerName);
                        sheet3.GetRow(rowindex).GetCell(1).SetCellValue(ThreeMonthPlus[i].Age);
                        sheet3.GetRow(rowindex).GetCell(2).SetCellType(CellType.Numeric);
                        sheet3.GetRow(rowindex).GetCell(2).SetCellValue(ThreeMonthPlus[i].WorkingSummary);
                        sheet3.GetRow(rowindex).GetCell(3).SetCellValue(ThreeMonthPlus[i].WorkingCity);
                        sheet3.GetRow(rowindex).GetCell(4).SetCellValue(ThreeMonthPlus[i].Position);
                        sheet3.GetRow(rowindex).GetCell(5).SetCellType(CellType.Numeric);
                        sheet3.GetRow(rowindex).GetCell(5).SetCellValue(ThreeMonthPlus[i].Salary);
                        sheet3.GetRow(rowindex).GetCell(6).SetCellValue(ThreeMonthPlus[i].SalaryCurrency);
                        if (ThreeMonthPlus[i].ResumeUpdate != DateTime.MinValue)
                        {
                            var cell = sheet3.GetRow(rowindex).GetCell(7);
                            var format = workbook.CreateDataFormat();
                            var cellStyle = workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(ThreeMonthPlus[i].ResumeUpdate);
                        }
                        else
                        {
                            sheet3.GetRow(rowindex).GetCell(7).SetCellValue("-");
                        }
                        sheet3.GetRow(rowindex).GetCell(8).SetCellValue(true);
                        if (ThreeMonthPlus[i].Start != DateTime.MinValue)
                        {
                            var cell = sheet3.GetRow(rowindex).GetCell(9);
                            var format = workbook.CreateDataFormat();
                            var cellStyle = workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(ThreeMonthPlus[i].Start);
                        }
                        else
                        {
                            sheet3.GetRow(rowindex).GetCell(9).SetCellValue("-");
                        }

                        if (ThreeMonthPlus[i].End != DateTime.MinValue)
                        {
                            var cell = sheet3.GetRow(rowindex).GetCell(10);
                            var format = workbook.CreateDataFormat();
                            var cellStyle = workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(ThreeMonthPlus[i].End);
                        }
                        else
                        {
                            sheet3.GetRow(rowindex).GetCell(10).SetCellValue("-");
                        }
                        sheet3.GetRow(rowindex).GetCell(11).SetCellValue(ThreeMonthPlus[i].Link);
                        sheet3.GetRow(rowindex).GetCell(12).SetCellValue(ThreeMonthPlus[i].WorkedPlaces[0].Name);
                        rowindex++;
                    }

                    this.SetStyleToAll(sheet3.GetRow(rowindex), cellstyle, 12);
                    sheet3.GetRow(rowindex++).GetCell(0).SetCellValue($"Не работают...{NotWorkingUsers.Count} резюме");
                    for (var i = 0; i < NotWorkingUsers.Count; i++)
                    {
                        sheet3.GetRow(rowindex).GetCell(0).SetCellValue(NotWorkingUsers[i].OwnerName);
                        sheet3.GetRow(rowindex).GetCell(1).SetCellValue(NotWorkingUsers[i].Age);
                        sheet3.GetRow(rowindex).GetCell(2).SetCellType(CellType.Numeric);
                        sheet3.GetRow(rowindex).GetCell(2).SetCellValue(NotWorkingUsers[i].WorkingSummary);
                        sheet3.GetRow(rowindex).GetCell(3).SetCellValue(NotWorkingUsers[i].WorkingCity);
                        sheet3.GetRow(rowindex).GetCell(4).SetCellValue(NotWorkingUsers[i].Position);
                        sheet3.GetRow(rowindex).GetCell(5).SetCellType(CellType.Numeric);
                        sheet3.GetRow(rowindex).GetCell(5).SetCellValue(NotWorkingUsers[i].Salary);
                        sheet3.GetRow(rowindex).GetCell(6).SetCellValue(NotWorkingUsers[i].SalaryCurrency);
                        if (NotWorkingUsers[i].ResumeUpdate != DateTime.MinValue)
                        {
                            var cell = sheet3.GetRow(rowindex).GetCell(7);
                            var format = workbook.CreateDataFormat();
                            var cellStyle = workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(NotWorkingUsers[i].ResumeUpdate);
                        }
                        else
                        {
                            sheet3.GetRow(rowindex).GetCell(7).SetCellValue("-");
                        }
                        sheet3.GetRow(rowindex).GetCell(8).SetCellValue(false);
                        if (NotWorkingUsers[i].Start != DateTime.MinValue)
                        {
                            var cell = sheet3.GetRow(rowindex).GetCell(9);
                            var format = workbook.CreateDataFormat();
                            var cellStyle = workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(NotWorkingUsers[i].Start);
                        }
                        else
                        {
                            sheet3.GetRow(rowindex).GetCell(9).SetCellValue("-");
                        }

                        if (NotWorkingUsers[i].End != DateTime.MinValue)
                        {
                            var cell = sheet3.GetRow(rowindex).GetCell(10);
                            var format = workbook.CreateDataFormat();
                            var cellStyle = workbook.CreateCellStyle();
                            cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(NotWorkingUsers[i].End);
                        }
                        else
                        {
                            sheet3.GetRow(rowindex).GetCell(10).SetCellValue("-");
                        }
                        sheet3.GetRow(rowindex).GetCell(11).SetCellValue(NotWorkingUsers[i].Link);
                        sheet3.GetRow(rowindex).GetCell(12).SetCellValue(NotWorkingUsers[i].WorkedPlaces[0].Name);
                        rowindex++;
                    }

                    for (var i = 0; i <= 12; i++)
                    {
                        sheet3.AutoSizeColumn(i);
                    }
                }
                {
                    for (var i = 0; i < 100; i++)
                    {
                        var row = sheet4.CreateRow(i);
                        for (var z = 0; z <= 2; z++)
                        {
                            var cell = row.CreateCell(z);
                        }
                    }
                    var rowindex = 0;
                    var cellstyle = workbook.CreateCellStyle();
                    cellstyle.FillForegroundColor = IndexedColors.Aqua.Index;
                    cellstyle.FillPattern = FillPattern.SolidForeground;
                    var font = workbook.CreateFont();
                    font.Color = IndexedColors.DarkBlue.Index;
                    font.FontHeight = 13;
                    cellstyle.SetFont(font);

                    var headercellstyle = workbook.CreateCellStyle();
                    var headercellfontstyle = workbook.CreateFont();
                    headercellstyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thick;
                    headercellstyle.BottomBorderColor = IndexedColors.RoyalBlue.Index;
                    headercellfontstyle.FontHeight = 15;
                    headercellfontstyle.IsBold = true;
                    headercellfontstyle.Color = IndexedColors.RoyalBlue.Index;
                    headercellstyle.SetFont(headercellfontstyle);
                    this.SetStyleToAll(sheet4.GetRow(0), headercellstyle, 2);
                    sheet4.GetRow(rowindex).GetCell(0).SetCellValue("Отчет от " + DateTime.Now.ToShortDateString());
                    sheet4.GetRow(rowindex).GetCell(1).SetCellValue("Всего резюме");
                    sheet4.GetRow(rowindex).GetCell(2).SetCellValue(company.CompanyResumes.Count.ToString());
                    rowindex++;
                    var Secondheadercellstyle = workbook.CreateCellStyle();
                    var Secondheadercellfontstyle = workbook.CreateFont();
                    Secondheadercellstyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thick;
                    Secondheadercellstyle.BottomBorderColor = IndexedColors.DarkBlue.Index;
                    Secondheadercellfontstyle.FontHeight = 13;
                    Secondheadercellfontstyle.IsBold = true;
                    Secondheadercellfontstyle.Color = IndexedColors.DarkBlue.Index;
                    Secondheadercellstyle.SetFont(Secondheadercellfontstyle);
                    this.SetStyleToAll(sheet4.GetRow(rowindex), Secondheadercellstyle, 1);
                    sheet4.GetRow(rowindex).GetCell(0).SetCellValue("Всего работают");
                    sheet4.GetRow(rowindex).GetCell(1).SetCellValue(working.Count);
                    rowindex++;
                    var Thirdheadercellstyle = workbook.CreateCellStyle();
                    var Thirdheadercellfontstyle = workbook.CreateFont();
                    Thirdheadercellstyle.FillForegroundColor = IndexedColors.Yellow.Index;
                    Thirdheadercellstyle.FillPattern = FillPattern.SolidForeground;
                    Thirdheadercellfontstyle.FontHeight = 11;
                    Thirdheadercellstyle.SetFont(Thirdheadercellfontstyle);
                    this.SetStyleToAll(sheet4.GetRow(rowindex), Thirdheadercellstyle, 1);
                    sheet4.GetRow(rowindex).GetCell(0).SetCellValue("Дата обновление резюме до двух недель");
                    sheet4.GetRow(rowindex).GetCell(1).SetCellValue(TwoWeeks.Count);
                    rowindex++;
                    sheet4.GetRow(rowindex).GetCell(0).SetCellValue("Резюме без указанных имен");
                    sheet4.GetRow(rowindex).GetCell(1).SetCellValue(TwoWeeks.Count(t => String.IsNullOrEmpty(t.OwnerName) || t.OwnerName == "-"));
                    rowindex++;
                    sheet4.GetRow(rowindex).GetCell(0).SetCellValue("Максимальная зарплата");
                    sheet4.GetRow(rowindex).GetCell(1).SetCellValue(TwoWeeks.Count == 0 ? 0 : TwoWeeks.Max(t => t.Salary == "" ? 0 : Convert.ToInt32(t.Salary.Replace(" ", ""))));
                    rowindex++;
                    sheet4.GetRow(rowindex).GetCell(0).SetCellValue("Максимальный стаж");
                    sheet4.GetRow(rowindex).GetCell(1).SetCellValue(TwoWeeks.Count == 0 ? 0 : TwoWeeks.Max(t => t.WorkingSummary == "" || t.WorkingSummary == null ? 0 : Convert.ToDouble(t.WorkingSummary.Replace(" ", ""))));
                    rowindex++;
                    sheet4.GetRow(rowindex).GetCell(0).SetCellValue("Максимальный возраст");
                    sheet4.GetRow(rowindex).GetCell(1).SetCellValue(TwoWeeks.Count == 0 ? 0 : TwoWeeks.Max(t => t.Age == "" || t.Age == null ? 0 : Convert.ToDouble(t.Age.Replace(" ", ""))));
                    rowindex++;
                    this.SetStyleToAll(sheet4.GetRow(rowindex), Thirdheadercellstyle, 1);
                    sheet4.GetRow(rowindex).GetCell(0).SetCellValue("Дата обновления резюме от двух недель до трех месяцев");
                    sheet4.GetRow(rowindex).GetCell(1).SetCellValue(TwoWeekToThreeMonth.Count);
                    rowindex++;
                    sheet4.GetRow(rowindex).GetCell(0).SetCellValue("Резюме без указанных имен");
                    sheet4.GetRow(rowindex).GetCell(1).SetCellValue(TwoWeekToThreeMonth.Count(t => String.IsNullOrEmpty(t.OwnerName) || t.OwnerName == "-"));
                    rowindex++;
                    sheet4.GetRow(rowindex).GetCell(0).SetCellValue("Максимальная зарплата");
                    sheet4.GetRow(rowindex).GetCell(1).SetCellValue(TwoWeekToThreeMonth.Count == 0 ? 0 : TwoWeekToThreeMonth.Max(t => t.Salary == "" ? 0 : Convert.ToInt32(t.Salary.Replace(" ", ""))));
                    rowindex++;
                    sheet4.GetRow(rowindex).GetCell(0).SetCellValue("Максимальный стаж");
                    sheet4.GetRow(rowindex).GetCell(1).SetCellValue(TwoWeekToThreeMonth.Count == 0 ? 0 : TwoWeekToThreeMonth.Max(t => t.WorkingSummary == "" || t.WorkingSummary == null ? 0 : Convert.ToDouble(t.WorkingSummary.Replace(" ", ""))));
                    rowindex++;
                    sheet4.GetRow(rowindex).GetCell(0).SetCellValue("Максимальный возраст");
                    sheet4.GetRow(rowindex).GetCell(1).SetCellValue(TwoWeekToThreeMonth.Count == 0 ? 0 : TwoWeekToThreeMonth.Max(t => t.Age == "" || t.Age == null ? 0 : Convert.ToDouble(t.Age.Replace(" ", ""))));
                    rowindex++;
                    this.SetStyleToAll(sheet4.GetRow(rowindex), Thirdheadercellstyle, 1);
                    sheet4.GetRow(rowindex).GetCell(0).SetCellValue("Дата обновления резюме от трех месяцев");
                    sheet4.GetRow(rowindex).GetCell(1).SetCellValue(ThreeMonthPlus.Count);
                    rowindex++;
                    sheet4.GetRow(rowindex).GetCell(0).SetCellValue("Резюме без указанных имен");
                    sheet4.GetRow(rowindex).GetCell(1).SetCellValue(ThreeMonthPlus.Count(t => String.IsNullOrEmpty(t.OwnerName) || t.OwnerName == "-"));
                    rowindex++;
                    sheet4.GetRow(rowindex).GetCell(0).SetCellValue("Максимальная зарплата");
                    sheet4.GetRow(rowindex).GetCell(1).SetCellValue(ThreeMonthPlus.Count == 0 ? 0 : ThreeMonthPlus.Max(t => t.Salary == "" ? 0 : Convert.ToInt32(t.Salary.Replace(" ", ""))));
                    rowindex++;
                    sheet4.GetRow(rowindex).GetCell(0).SetCellValue("Максимальный стаж");
                    sheet4.GetRow(rowindex).GetCell(1).SetCellValue(ThreeMonthPlus.Count == 0 ? 0 : ThreeMonthPlus.Max(t => t.WorkingSummary == "" || t.WorkingSummary == null ? 0 : Convert.ToDouble(t.WorkingSummary.Replace(" ", ""))));
                    rowindex++;
                    sheet4.GetRow(rowindex).GetCell(0).SetCellValue("Максимальный возраст");
                    sheet4.GetRow(rowindex).GetCell(1).SetCellValue(ThreeMonthPlus.Count == 0 ? 0 : ThreeMonthPlus.Max(t => t.Age == "" || t.Age == null ? 0 : Convert.ToDouble(t.Age.Replace(" ", ""))));
                    rowindex++;
                    this.SetStyleToAll(sheet4.GetRow(rowindex), Secondheadercellstyle, 1);
                    sheet4.GetRow(rowindex).GetCell(0).SetCellValue("Не работают");
                    sheet4.GetRow(rowindex).GetCell(1).SetCellValue(NotWorkingUsers.Count);
                    rowindex++;
                    sheet4.GetRow(rowindex).GetCell(0).SetCellValue("Резюме без указанных имен");
                    sheet4.GetRow(rowindex).GetCell(1).SetCellValue(NotWorkingUsers.Count(t => String.IsNullOrEmpty(t.OwnerName) || t.OwnerName == "-"));
                    rowindex++;
                    sheet4.GetRow(rowindex).GetCell(0).SetCellValue("Максимальная зарплата");
                    sheet4.GetRow(rowindex).GetCell(1).SetCellValue(NotWorkingUsers.Count == 0 ? 0 : NotWorkingUsers.Max(t => t.Salary == "" ? 0 : Convert.ToInt32(t.Salary.Replace(" ", ""))));
                    rowindex++;
                    sheet4.GetRow(rowindex).GetCell(0).SetCellValue("Максимальный стаж");
                    sheet4.GetRow(rowindex).GetCell(1).SetCellValue(NotWorkingUsers.Count == 0 ? 0 : NotWorkingUsers.Max(t => t.WorkingSummary == "" || t.WorkingSummary == null ? 0 : Convert.ToDouble(t.WorkingSummary.Replace(" ", ""))));
                    rowindex++;
                    sheet4.GetRow(rowindex).GetCell(0).SetCellValue("Максимальный возраст");
                    sheet4.GetRow(rowindex).GetCell(1).SetCellValue(NotWorkingUsers.Count == 0 ? 0 : NotWorkingUsers.Max(t => t.Age == "" || t.Age == null ? 0 : Convert.ToDouble(t.Age.Replace(" ", ""))));
                    rowindex++;
                    //sheet2.GetRow(rowindex++).GetCell(1).SetCellValue(TwoWeeks.Count.ToString());
                    //sheet2.GetRow(rowindex).GetCell(0).SetCellValue("");
                    //SetStyleToAll(sheet2.GetRow(rowindex), cellstyle, 1);
                    //sheet2.GetRow(rowindex++).GetCell(1).SetCellValue("");
                    //sheet2.GetRow(rowindex).GetCell(0).SetCellValue("Резюме с датой обновления 2 недели - 3 месяца");
                    //sheet2.GetRow(rowindex++).GetCell(1).SetCellValue(TwoWeekToThreeMonth.Count.ToString());
                    //
                    //sheet2.GetRow(rowindex).GetCell(0).SetCellValue("Резюме с датой обновления больше чем 3 месяца");
                    //sheet2.GetRow(rowindex++).GetCell(1).SetCellValue(ThreeMonthPlus.Count.ToString());
                    //
                    //sheet2.GetRow(rowindex).GetCell(0).SetCellValue("Не работают");
                    //sheet2.GetRow(rowindex++).GetCell(1).SetCellValue(NotWorkingUsers.Count.ToString());
                    //
                    //sheet2.GetRow(rowindex).GetCell(0).SetCellValue("Максимальная зарплата");
                    //sheet2.GetRow(rowindex++).GetCell(1).SetCellValue(company.CompanyResumes.Max(t => t.Salary == "" ? 0 : Convert.ToInt32(t.Salary.Replace(" ", ""))));

                    for (var i = 0; i <= 2; i++)
                    {
                        sheet4.AutoSizeColumn(i);
                    }

                }

                var fs = new FileStream(sfd.SelectedPath + "\\" + company.Name.Replace(":", "").Replace("?", "").Replace("/", "")
                    .Replace("\\", "").Replace("\"", "").Replace("*", "").Replace("<", "")
                    .Replace(">", "").Replace("|", "") + ".xlsx", FileMode.Create);
                workbook.Write(fs);
            }
        }
        private void Form1_Shown(object sender, EventArgs e)
        {
#if !DEBUG
            var authcheck = new CheckAuth();
            var result = authcheck.ShowDialog(this);
            if (result != 1)
            {
                Application.Exit();
            }
#endif
            this.textBox1.Text = "o.ivanova.gb@mail.ru";
            this.textBox2.Text = "z31567890";
        }
        private void EditCompanies_Click(object sender, EventArgs e)
        {
            var editform = new CompaniesEditForm(this.companyItems);
            editform.ShowDialog(this);
            this.textBox4.Text = this.companyItems.Count.ToString();
        }
        private void StopParsing_Click(object sender, EventArgs e)
        {
            this.resumebycompanysearhsource.Cancel();
        }
        private void ClearResumeTable_Click(object sender, EventArgs e)
        {
            this.resumes_grid.Rows.Clear();
        }
        private void EditCompaniesList_Click(object sender, EventArgs e)
        {
            if (this.Tab5ParseType == 0)
            {
                var editform = new CompaniesEditForm(this.companiesResumesTrackerItems);
                editform.ShowDialog(this);
                return;
            }
            new LinksEditForm(this.companiesResumesTrackerItems).ShowDialog(this);
        }

        private bool CompaniesFirstParsing = true;
        private async void StartCompaniesParsing_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(Application.StartupPath + "\\htmls"))
            {
                Directory.Delete(Application.StartupPath + "\\htmls", true);
            }
            Directory.CreateDirectory(Application.StartupPath + "\\htmls");
            this.resumebycompanysearhsource = new CancellationTokenSource();
            this.parsedCompaniesProgress.Maximum = this.companiesResumesTrackerItems.Count;
            if (!this.CheckIfSeleniumIsAlive())
            {
                MessageBox.Show("Необходимо авторизироваться", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (this.companiesResumesTrackerItems.Count == 0)
            {
                MessageBox.Show("Нет добавленных компаний или ссылок", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var showdialog = DialogResult.No;
            if (this.Tab5ParseType == 0 && this.companiesResumesTrackerItems.Any(t => t.CheckOnWebsite))
            {
                showdialog = MessageBox.Show("Показать окно редактирования компаний для правки альтернативных имен после парсинга по сайту?", "Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }
            this._chrome.Url = "https://hh.ru/search/resume?text=билайн&logic=normal&pos=workplace_organization&exp_period=all_time&area=1&relocation=living_or_relocation&salary_from=&salary_to=&currency_code=RUR&education=none&age_from=&age_to=&gender=unknown&order_by=publication_time&search_period=0&items_on_page=100";
            if (this.CompaniesFirstParsing)
            {
                try
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(this._chrome.PageSource);
                    var xpath1 = doc.DocumentNode.FindElementByTagNameAndInnerText("span", "Вид результатов")?.XPath; ;
                    this._chrome.FindElement(By.XPath(xpath1)).Click();
                    Thread.Sleep(1000);
                    doc = new HtmlDocument();
                    doc.LoadHtml(this._chrome.PageSource);
                    var xpath = doc.DocumentNode.FindElementByAttributeAndGetXpath("input", "data-qa", "resume-serp__view-companies");
                    var attr = this._chrome.FindElement(By.XPath(xpath)).GetAttribute("checked");
                    if (attr == "false" || attr == null)
                    {
                        this._chrome.FindElement(By.XPath(xpath.Replace("/input[1]", ""))).Click();
                        Thread.Sleep(500);
                    }
                    xpath = doc.DocumentNode.FindElementByAttributeAndGetXpath("input", "value", "area");
                    attr = this._chrome.FindElement(By.XPath(xpath)).GetAttribute("checked");
                    if (attr == "false" || attr == null)
                    {
                        this._chrome.FindElement(By.XPath(xpath.Replace("/input[1]", ""))).Click();
                        Thread.Sleep(500);
                    }
                    this._chrome.FindElement(By.XPath(doc.DocumentNode.FindElementByAttributeAndGetXpath("input", "data-qa", "resume-serp__view-save"))).Click();
                    this.CompaniesFirstParsing = false;
                }
                catch (Exception)
                {
                    //var result = MessageBox.Show("Возникла ошибка в ходе установки параметров.Возможно это связано с ошибкой в ходе авторизации.\r" +
                    //      "При продолжение парсинга,результат будет неполным.\r" +
                    //      "Продолжить?", "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    //if (result == DialogResult.No)
                    //{
                    //    MessageBox.Show("Отменить парсинг?", "Информация", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    //    return;
                    //}
                }
            }
            try
            {
                if (this.Tab5ParseType == 0)
                {
                    foreach (var item in this.companiesResumesTrackerItems)
                    {
                        this.BeginInvoke(new MethodInvoker(() => this.currentParsingCompany.Text = item.Name));
                        if (item.CheckOnWebsite && !String.IsNullOrEmpty(item.Website))
                        {
                            var parser = new ResumesByCompanyNameParser(item, this._chrome, this.resumebycompanysearhsource)
                            {
                                metroLabel = this.currentParsingCompany,
                                metroProgressBar = this.parsedCompaniesProgress
                            }; //TODO нужно заменить таблицу
                            await Task.Run(() => parser.ParseAlternatives());
                        }
                        this.BeginInvoke(new MethodInvoker(() => this.parsedCompaniesProgress.Value++));

                    }
                    if (showdialog == DialogResult.Yes)
                    {
                        var editform = new CompaniesEditForm(this.companiesResumesTrackerItems);
                        editform.ShowDialog(this);
                    }
                    this.parsedCompaniesProgress.Value = 0;
                    Thread.Sleep(1000);
                    var metros = new Metros();
                    foreach (var item in this.companiesResumesTrackerItems)
                    {
                        var oldresumes = item.CompanyResumes?.Select(t => t).ToList();
                        item.CompanyResumes = new List<Resume>();
                        this.currentParsingCompany.Text = item.Name;
                        var parser = new ResumesByCompanyNameParser(item, this._chrome, null, metros, this.resumebycompanysearhsource)
                        {
                            metroLabel = this.companyResumeParsedCount
                        }; //TODO нужно заменить таблицу
                        await Task.Run(() => parser.Parse());
                        this.parsedCompaniesProgress.Value++;
                        this.companyResumeParsedCount.Text = "0";
                        var summaryresumes = new List<Resume>();
                        if (oldresumes != null && oldresumes.Count != 0)
                        {
                            for (var i = 0; i < item.CompanyResumes?.Count; i++)
                            {
                                var index = oldresumes.FindIndex(t => t.Link == item.CompanyResumes[i].Link);
                                if (index != -1) //was
                                {
                                    var it = item.CompanyResumes[i];
                                    it.ResumeCreated = oldresumes[index].ResumeCreated;
                                    summaryresumes.Add(it);
                                }
                                else //is new
                                {
                                    summaryresumes.Add(item.CompanyResumes[i]);
                                    summaryresumes.Last().IsNew = true;
                                    this._chrome.Url = item.CompanyResumes[i].Link;
                                    var htmlDocument = new HtmlDocument();
                                    htmlDocument.LoadHtml(this._chrome.PageSource);
                                    var SearchName = htmlDocument.DocumentNode.FindElementByAttributeAndGetInnerText("h1", "data-qa", "resume-personal-name").Trim();

                                    var Phone = htmlDocument.DocumentNode.FindElementByAttributeAndGetInnerText("span", "itemprop", "telephone");
                                    if (Phone.Contains("..."))
                                    {
                                        Phone = htmlDocument.DocumentNode.FindElementByAttributeAndGetInnerText("div", "class", "resume__contacts-phone-print-version");
                                    }
                                    Phone = Phone.Split('—')[0].Trim().Replace("&nbsp;", "");
                                    if (htmlDocument.DocumentNode.FindElementByAttributeAndGetNode("span", "itemprop", "telephone") != null)
                                    {
                                        htmlDocument.DocumentNode.FindElementByAttributeAndGetNode("span", "itemprop", "telephone").InnerHtml = Phone;
                                    }
                                    File.WriteAllText(Application.StartupPath + "\\htmls\\" + SearchName + ".html", htmlDocument.DocumentNode.OuterHtml);

                                }
                            }
                            item.CompanyResumes = summaryresumes;
                        }
                    }
                }
                else
                {
                    this.parsedCompaniesProgress.Value = 0;
                    Thread.Sleep(1000);
                    foreach (var item in this.companiesResumesTrackerItems)
                    {
                        var oldresumes = item.CompanyResumes?.Select(t => t).ToList();
                        item.CompanyResumes = new List<Resume>();
                        this.currentParsingCompany.Text = item.Name;
                        var parser = new ResumesByCompanyNameParser(item, this._chrome, null, null, this.resumebycompanysearhsource)
                        {
                            metroLabel = this.companyResumeParsedCount
                        };
                        await Task.Run(() => parser.ParseByLink());
                        this.parsedCompaniesProgress.Value++;
                        this.companyResumeParsedCount.Text = "0";
                        var summaryresumes = new List<Resume>();
                        if (oldresumes != null && oldresumes.Count != 0)
                        {
                            for (var i = 0; i < item.CompanyResumes.Count; i++)
                            {
                                var index = oldresumes.FindIndex(t => t.Link == item.CompanyResumes[i].Link);
                                if (index != -1) //was
                                {
                                    var it = item.CompanyResumes[i];
                                    it.ResumeCreated = oldresumes[index].ResumeCreated;
                                    summaryresumes.Add(it);
                                }
                                else //is new
                                {
                                    summaryresumes.Add(item.CompanyResumes[i]);
                                    summaryresumes.Last().IsNew = true;
                                    this._chrome.Url = item.CompanyResumes[i].Link;
                                    var htmlDocument = new HtmlDocument();
                                    htmlDocument.LoadHtml(this._chrome.PageSource);
                                    var SearchName = htmlDocument.DocumentNode.FindElementByAttributeAndGetInnerText("h1", "data-qa", "resume-personal-name").Trim();

                                    var Phone = htmlDocument.DocumentNode.FindElementByAttributeAndGetInnerText("span", "itemprop", "telephone");
                                    if (Phone.Contains("..."))
                                    {
                                        Phone = htmlDocument.DocumentNode.FindElementByAttributeAndGetInnerText("div", "class", "resume__contacts-phone-print-version");
                                    }
                                    Phone = Phone.Split('—')[0].Trim().Replace("&nbsp;", "");
                                    if (htmlDocument.DocumentNode.FindElementByAttributeAndGetNode("span", "itemprop", "telephone") != null)
                                    {
                                        htmlDocument.DocumentNode.FindElementByAttributeAndGetNode("span", "itemprop", "telephone").InnerHtml = Phone;
                                    }
                                    File.WriteAllText(Application.StartupPath + "\\htmls\\" + SearchName + ".html", htmlDocument.DocumentNode.OuterHtml);

                                }
                            }
                            item.CompanyResumes = summaryresumes;
                        }
                    }
                }
                this.parsedCompaniesProgress.Value = 0;
                this.currentParsingCompany.Text = "-------";
                if (this.notifier == null)
                {
                    this.notifier = new PopupNotifier
                    {
                        Image = Properties.Resources.Info_Button_256,
                        TitleText = "HH Parser",
                        TitleFont = new Font("Segui UI", 16, FontStyle.Regular, GraphicsUnit.Pixel),
                        TitleColor = Color.Black,
                        AnimationInterval = 1,
                        AnimationDuration = 1000,
                        Delay = 20000,
                        ContentFont = new Font("Segui UI", 14, FontStyle.Regular, GraphicsUnit.Pixel),
                        ContentText = "Парсинг резюме завершен!",
                        ShowCloseButton = true,
                        ImageSize = new Size(35, 35),
                        ImagePadding = new Padding(3, 3, 0, 0),
                        Size = new Size(400, 90),
                        HeaderColor = Color.SlateGray
                    };
                    this.notifier.Popup();
                }
                else
                {
                    this.notifier.Hide();
                    this.notifier.Popup();
                }
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Парсинг отменен", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("Возникла ошибка во время парсинга", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private List<Resume>[] SortResumes(List<Resume> resumes)
        {
            var today = DateTime.Today;
            var NotWorkingUsers = resumes.Where(t => !t.WorkingNow).ToList();
            var working = resumes.Where(t => t.WorkedPlaces.Count != 0 && t.WorkedPlaces[0].End.Date == today.Date).ToList();
            var TwoWeeks = working.Where(t => today.Subtract(t.ResumeUpdate).TotalDays <= 14).ToList();
            var TwoWeekToThreeMonth = working.Where(t => today.Subtract(t.ResumeUpdate).TotalDays > 14 && today.Subtract(t.ResumeUpdate).TotalDays <= 90).ToList();
            var ThreeMonthPlus = working.Where(t => today.Subtract(t.ResumeUpdate).TotalDays > 90).ToList();
            return new List<Resume>[] { NotWorkingUsers, TwoWeeks, TwoWeekToThreeMonth, ThreeMonthPlus };
        }
        private int WriteCompaniesInSheet(ISheet sheet2, int rowindex, List<Resume> groupedResumes, ICellStyle underlinecellstyle)
        {
            var newResmes = groupedResumes.Where(t => t.IsNew).ToList();
            groupedResumes = groupedResumes.Except(newResmes).ToList();
            groupedResumes.Sort((a, b) => b.ResumeUpdate.CompareTo(a.ResumeUpdate));
            newResmes.Sort((a, b) => b.ResumeUpdate.CompareTo(a.ResumeUpdate));
            groupedResumes.InsertRange(0, newResmes);
            for (var i = 0; i < groupedResumes.Count; i++)
            {
                sheet2.GetRow(rowindex).GetCell(0).SetCellValue(groupedResumes[i].OwnerName);
                sheet2.GetRow(rowindex).GetCell(1).SetCellValue(groupedResumes[i].Age);
                sheet2.GetRow(rowindex).GetCell(2).SetCellType(CellType.Numeric);
                sheet2.GetRow(rowindex).GetCell(2).SetCellValue(groupedResumes[i].WorkingSummary);
                sheet2.GetRow(rowindex).GetCell(3).SetCellValue(groupedResumes[i].WorkingCity);
                sheet2.GetRow(rowindex).GetCell(4).SetCellValue(groupedResumes[i].Position);
                sheet2.GetRow(rowindex).GetCell(5).SetCellType(CellType.Numeric);
                sheet2.GetRow(rowindex).GetCell(5).SetCellValue(groupedResumes[i].Salary);
                sheet2.GetRow(rowindex).GetCell(6).SetCellValue(groupedResumes[i].SalaryCurrency);
                if (groupedResumes[i].ResumeCreated != DateTime.MinValue)
                {
                    var cell = sheet2.GetRow(rowindex).GetCell(7);
                    var format = sheet2.Workbook.CreateDataFormat();
                    var cellStyle = sheet2.Workbook.CreateCellStyle();
                    cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(groupedResumes[i].ResumeCreated);
                }
                else
                {
                    sheet2.GetRow(rowindex).GetCell(7).SetCellValue("-");
                }
                if (groupedResumes[i].ResumeUpdate != DateTime.MinValue)
                {
                    var cell = sheet2.GetRow(rowindex).GetCell(8);
                    //var format = sheet2.Workbook.CreateDataFormat();
                    //var cellStyle = sheet2.Workbook.CreateCellStyle();
                    //cellStyle.DataFormat = format.GetFormat("dd MMM yyyy");
                    //cell.CellStyle = cellStyle;
                    cell.SetCellValue(this.DateToString(groupedResumes[i].ResumeUpdate));
                }
                else
                {
                    sheet2.GetRow(rowindex).GetCell(8).SetCellValue("-");
                }
                sheet2.GetRow(rowindex).GetCell(9).SetCellValue(true);
                if (groupedResumes[i].Start != DateTime.MinValue)
                {
                    var cell = sheet2.GetRow(rowindex).GetCell(10);
                    var format = sheet2.Workbook.CreateDataFormat();
                    var cellStyle = sheet2.Workbook.CreateCellStyle();
                    cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(groupedResumes[i].Start);
                }
                else
                {
                    sheet2.GetRow(rowindex).GetCell(10).SetCellValue("-");
                }

                if (groupedResumes[i].End != DateTime.MinValue)
                {
                    var cell = sheet2.GetRow(rowindex).GetCell(11);
                    var format = sheet2.Workbook.CreateDataFormat();
                    var cellStyle = sheet2.Workbook.CreateCellStyle();
                    cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(groupedResumes[i].End);
                }
                else
                {
                    sheet2.GetRow(rowindex).GetCell(11).SetCellValue("-");
                }
                var targetLink = new XSSFHyperlink(HyperlinkType.Url)
                {
                    Address = groupedResumes[i].Link
                };
                sheet2.GetRow(rowindex).GetCell(12).Hyperlink = targetLink;
                sheet2.GetRow(rowindex).GetCell(12).SetCellValue("Ссылка");
                sheet2.GetRow(rowindex).GetCell(12).CellStyle = underlinecellstyle;
                sheet2.GetRow(rowindex).GetCell(12).SetAsActiveCell();
                sheet2.GetRow(rowindex).GetCell(13).SetCellValue(groupedResumes[i].WorkedPlaces[0].Name);
                sheet2.GetRow(rowindex).GetCell(14).SetCellValue(groupedResumes[i].IsNew);

                rowindex++;
            }
            return rowindex;

        }
        private int WriteLinkResumesInSheet(ISheet sheet2, int rowindex, List<Resume> groupedResumes, ICellStyle underlinecellstyle)
        {
            var newResmes = groupedResumes.Where(t => t.IsNew).ToList();
            groupedResumes = groupedResumes.Except(newResmes).ToList();
            groupedResumes.Sort((a, b) => b.ResumeUpdate.CompareTo(a.ResumeUpdate));
            newResmes.Sort((a, b) => b.ResumeUpdate.CompareTo(a.ResumeUpdate));
            groupedResumes.InsertRange(0, newResmes);
            for (var i = 0; i < groupedResumes.Count; i++)
            {
                sheet2.GetRow(rowindex).GetCell(0).SetCellValue(groupedResumes[i].OwnerName);
                sheet2.GetRow(rowindex).GetCell(1).SetCellValue(groupedResumes[i].Age);
                sheet2.GetRow(rowindex).GetCell(2).SetCellType(CellType.Numeric);
                sheet2.GetRow(rowindex).GetCell(2).SetCellValue(groupedResumes[i].WorkingSummary);
                sheet2.GetRow(rowindex).GetCell(3).SetCellValue(groupedResumes[i].WorkingCity);
                sheet2.GetRow(rowindex).GetCell(4).SetCellValue(groupedResumes[i].Position);
                sheet2.GetRow(rowindex).GetCell(5).SetCellType(CellType.Numeric);
                sheet2.GetRow(rowindex).GetCell(5).SetCellValue(groupedResumes[i].Salary);
                sheet2.GetRow(rowindex).GetCell(6).SetCellValue(groupedResumes[i].SalaryCurrency);
                if (groupedResumes[i].ResumeCreated != DateTime.MinValue)
                {
                    var cell = sheet2.GetRow(rowindex).GetCell(7);
                    var format = sheet2.Workbook.CreateDataFormat();
                    var cellStyle = sheet2.Workbook.CreateCellStyle();
                    cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(groupedResumes[i].ResumeCreated);
                }
                else
                {
                    sheet2.GetRow(rowindex).GetCell(7).SetCellValue("-");
                }
                if (groupedResumes[i].ResumeUpdate != DateTime.MinValue)
                {
                    var cell = sheet2.GetRow(rowindex).GetCell(8);
                    //var format = sheet2.Workbook.CreateDataFormat();
                    //var cellStyle = sheet2.Workbook.CreateCellStyle();
                    //cellStyle.DataFormat = format.GetFormat("yyyy.MM.dd");
                    //cell.CellStyle = cellStyle;
                    cell.SetCellValue(this.DateToString(groupedResumes[i].ResumeUpdate));
                }
                else
                {
                    sheet2.GetRow(rowindex).GetCell(8).SetCellValue("-");
                }
                sheet2.GetRow(rowindex).GetCell(9).SetCellValue(true);
                var work = String.Join("\n", groupedResumes[i].WorkedPlaces.Take(3).Select(t => t.Name + " - " + t.Start.ToString("yyyy.MM.dd") + " - " + t.End.ToString("yyyy.MM.dd")));
                sheet2.GetRow(rowindex).GetCell(10).SetCellValue(work);
                var targetLink = new XSSFHyperlink(HyperlinkType.Url)
                {
                    Address = groupedResumes[i].Link
                };
                sheet2.GetRow(rowindex).GetCell(11).Hyperlink = targetLink;
                sheet2.GetRow(rowindex).GetCell(11).SetCellValue("Ссылка");
                sheet2.GetRow(rowindex).GetCell(11).CellStyle = underlinecellstyle;
                sheet2.GetRow(rowindex).GetCell(11).SetAsActiveCell();
                sheet2.GetRow(rowindex).GetCell(12).SetCellValue(groupedResumes[i].IsNew);
                rowindex++;
            }
            return rowindex;

        }
        private void SaveTrackerItems_Click(object sender, EventArgs e)
        {
            var sfd = new FolderBrowserDialog();
            if (sfd.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            var workbook = new XSSFWorkbook();
            var sheet1 = workbook.CreateSheet("Обложка");
            #region WriteFirstList
            {
                for (var i = 0; i < 25; i++)
                {
                    var row = sheet1.CreateRow(i);
                    for (var z = 0; z <= 5; z++)
                    {
                        row.CreateCell(z);
                    }
                }
                var cellstyle = workbook.CreateCellStyle();
                var font = workbook.CreateFont();
                font.FontHeight = 11;
                font.IsBold = true;
                cellstyle.SetFont(font);
                sheet1.GetRow(0).GetCell(1).CellStyle = cellstyle;
                sheet1.GetRow(10).GetCell(1).CellStyle = cellstyle;
                sheet1.GetRow(16).GetCell(1).CellStyle = cellstyle;
                sheet1.GetRow(0).GetCell(1).SetCellValue("На что следует обратить внимание:");
                sheet1.GetRow(1).GetCell(1).SetCellValue("Смотрите вкладку Данные для просмотра и печати");
                sheet1.GetRow(2).GetCell(1).SetCellValue("Обратите внимание на города и названия компаний, в случае, если в отчет попали города и названия не имеющие к вам ");
                sheet1.GetRow(3).GetCell(1).SetCellValue("отношения - исключите их через фитьтр.");
                sheet1.GetRow(4).GetCell(1).SetCellValue("Если вы видите, что ФИО сотрудника скрыты, значит, скорее всего, это резюме скрыто и от поиска Вашей компанией");
                sheet1.GetRow(5).GetCell(1).SetCellValue("Обратите наибольшее  внимание на сотрудников, недавно обновивших резюме и работающих в компании!");
                sheet1.GetRow(6).GetCell(1).SetCellValue("Если сотрудник давно обновил резюме - вероятно сейчас он активно не ищет работу, но его могут попытаться перемонить.");
                sheet1.GetRow(7).GetCell(1).SetCellValue("Сотрудники, которые ранее работали в Вашей компании и ищут работу - это интересная информация, а иногда и отличные ");
                sheet1.GetRow(8).GetCell(1).SetCellValue("кандидаты, способные быстро войти в курс дела.");
                sheet1.GetRow(10).GetCell(1).SetCellValue("Как использовать настоящий отчет:");
                sheet1.GetRow(11).GetCell(1).SetCellValue("Важно понять -");
                sheet1.GetRow(12).GetCell(1).SetCellValue("1. Подтвердить что резюме размещены для смены работы.");
                sheet1.GetRow(13).GetCell(1).SetCellValue("2. Понять причины такого желания");
                sheet1.GetRow(14).GetCell(1).SetCellValue("3. Оценить риски и принять решение направленное на удержание этого сотрудника или снижение ущерба его ухода.");
                sheet1.GetRow(16).GetCell(1).SetCellValue("Возможно ли, что отчет содержит лишние данные?");
                sheet1.GetRow(17).GetCell(1).SetCellValue("Возможно, это экспресс отчет и его возможности ограничены.");
                sheet1.GetRow(18).GetCell(1).SetCellValue("Так же, по нашему опыту, поиск HRSave находит на 20-50% больше резюме, благодаря используемым алгоритмам поиска.");
            }
            #endregion
            #region WriteCompaniesList
            var today = DateTime.Today;
            foreach (var company in this.companiesResumesTrackerItems)
            {
                if (company.type == 0)
                {
                    var sheet2 = workbook.CreateSheet("Данные - " + company.Name);
                    company.CompanyResumes.Sort((a, b) => b.ResumeUpdate.CompareTo(a.ResumeUpdate));
                    var groupedResumes = this.SortResumes(company.CompanyResumes);
                    {
                        for (var i = 0; i < company.CompanyResumes.Count + 5; i++)
                        {
                            var row = sheet2.CreateRow(i);
                            for (var z = 0; z <= 14; z++)
                            {
                                row.CreateCell(z);
                            }
                        }
                        sheet2.GetRow(0).GetCell(0).SetCellValue("Имя");
                        sheet2.GetRow(0).GetCell(1).SetCellValue("Лет");
                        sheet2.GetRow(0).GetCell(2).SetCellValue("Стаж");
                        sheet2.GetRow(0).GetCell(3).SetCellValue("Город");
                        sheet2.GetRow(0).GetCell(4).SetCellValue("Должность");
                        sheet2.GetRow(0).GetCell(5).SetCellValue("Зарплата");
                        sheet2.GetRow(0).GetCell(6).SetCellValue("Валюта");
                        sheet2.GetRow(0).GetCell(7).SetCellValue("Размещено");
                        sheet2.GetRow(0).GetCell(8).SetCellValue("Обновлено");
                        sheet2.GetRow(0).GetCell(9).SetCellValue("Работает сейчас");
                        sheet2.GetRow(0).GetCell(10).SetCellValue("Дата начала работы в компании");
                        sheet2.GetRow(0).GetCell(11).SetCellValue("Работал до");
                        sheet2.GetRow(0).GetCell(12).SetCellValue("Ссылка");
                        sheet2.GetRow(0).GetCell(13).SetCellValue("Место работы в резюме");
                        sheet2.GetRow(0).GetCell(14).SetCellValue("Новое");

                        var groupHeaderCellStyle = workbook.CreateCellStyle();
                        groupHeaderCellStyle.FillForegroundColor = IndexedColors.DarkRed.Index;
                        groupHeaderCellStyle.FillPattern = FillPattern.SolidForeground;
                        var font = workbook.CreateFont();
                        font.Color = IndexedColors.Yellow.Index;
                        font.FontHeight = 15;
                        groupHeaderCellStyle.SetFont(font);

                        var underlinecellstyle = workbook.CreateCellStyle();
                        var underlinefont = workbook.CreateFont();
                        underlinefont.Color = IndexedColors.LightBlue.Index;
                        underlinefont.Underline = FontUnderlineType.Single;
                        underlinecellstyle.SetFont(underlinefont);
                        var rowindex = 2;

                        this.SetStyleToAll(sheet2.GetRow(1), groupHeaderCellStyle, 13);
                        sheet2.GetRow(1).GetCell(0).SetCellValue($"Сотрудник работает в компании, обновил резюме менее 2 недель назад - {groupedResumes[1].Count} резюме");
                        rowindex = this.WriteCompaniesInSheet(sheet2, rowindex, groupedResumes[1], underlinecellstyle);

                        this.SetStyleToAll(sheet2.GetRow(rowindex), groupHeaderCellStyle, 14);
                        sheet2.GetRow(rowindex++).GetCell(0).SetCellValue($"Сотрудник работает в компании, обновил резюме от 2 недель до 3 месяцев - {groupedResumes[2].Count} резюме");
                        rowindex = this.WriteCompaniesInSheet(sheet2, rowindex, groupedResumes[2], underlinecellstyle);


                        this.SetStyleToAll(sheet2.GetRow(rowindex), groupHeaderCellStyle, 14);
                        sheet2.GetRow(rowindex++).GetCell(0).SetCellValue($"Указано что сотрудник работает в компании, обновил резюме более 3 месяцев назад - {groupedResumes[3].Count} резюме");
                        rowindex = this.WriteCompaniesInSheet(sheet2, rowindex, groupedResumes[3], underlinecellstyle);

                        this.SetStyleToAll(sheet2.GetRow(rowindex), groupHeaderCellStyle, 14);
                        sheet2.GetRow(rowindex++).GetCell(0).SetCellValue($"Сотрудник работал ранее в компании, сейчас ищет работу - {groupedResumes[0].Count} резюме, упорядочены по дате обновления резюме");
                        rowindex = this.WriteCompaniesInSheet(sheet2, rowindex, groupedResumes[0], underlinecellstyle);

                        sheet2.SetColumnWidth(0, 5384);  //A
                        sheet2.SetColumnWidth(1, 769);   //B
                        sheet2.SetColumnWidth(2, 5);     //C
                        sheet2.SetColumnWidth(3, 3382);  //D
                        sheet2.SetColumnWidth(4, 8691);  //E
                        sheet2.SetColumnWidth(5, 1615);  //F
                        sheet2.SetColumnWidth(6, 350);   //G
                        sheet2.SetColumnWidth(7, 2700);  //H
                        sheet2.SetColumnWidth(8, 2700);  //H
                        sheet2.SetColumnWidth(9, 5);     //I
                        sheet2.SetColumnWidth(10, 5);  //J
                        sheet2.SetColumnWidth(11, 2613); //K
                        sheet2.SetColumnWidth(12, 1921);//L
                        sheet2.SetColumnWidth(13, 12535);//M
                    }
                }
                else
                {
                    var variables = company.Name.Split('?')[1].Split('&');
                    var textVariable = variables.Where(t => t.StartsWith("text=")).ToList();
                    var companyName = "Не указано";
                    if (textVariable.Count > 0)
                    {
                        companyName = System.Web.HttpUtility.UrlDecode(textVariable[0].Substring(5));
                    }
                    var sheet = workbook.CreateSheet("Данные - " + companyName);
                    company.CompanyResumes.Sort((a, b) => b.ResumeUpdate.CompareTo(a.ResumeUpdate));
                    var groupedResumes = this.SortResumes(company.CompanyResumes);
                    {
                        for (var i = 0; i < company.CompanyResumes.Count + 10; i++)
                        {
                            var row = sheet.CreateRow(i);
                            for (var z = 0; z <= 12; z++)
                            {
                                row.CreateCell(z);
                            }
                        }
                        sheet.GetRow(0).GetCell(0).SetCellValue("Имя");
                        sheet.GetRow(0).GetCell(1).SetCellValue("Лет");
                        sheet.GetRow(0).GetCell(2).SetCellValue("Стаж");
                        sheet.GetRow(0).GetCell(3).SetCellValue("Город");
                        sheet.GetRow(0).GetCell(4).SetCellValue("Должность");
                        sheet.GetRow(0).GetCell(5).SetCellValue("Зарплата");
                        sheet.GetRow(0).GetCell(6).SetCellValue("Валюта");
                        sheet.GetRow(0).GetCell(7).SetCellValue("Размещено");
                        sheet.GetRow(0).GetCell(8).SetCellValue("Обновлено");
                        sheet.GetRow(0).GetCell(9).SetCellValue("Работает сейчас");
                        sheet.GetRow(0).GetCell(10).SetCellValue("Места работы");
                        sheet.GetRow(0).GetCell(11).SetCellValue("Ссылка");
                        sheet.GetRow(0).GetCell(12).SetCellValue("Новое");

                        var cellstyle = workbook.CreateCellStyle();
                        cellstyle.FillForegroundColor = IndexedColors.DarkRed.Index;
                        cellstyle.FillPattern = FillPattern.SolidForeground;
                        var font = workbook.CreateFont();
                        font.Color = IndexedColors.Yellow.Index;
                        font.FontHeight = 15;
                        cellstyle.SetFont(font);
                        this.SetStyleToAll(sheet.GetRow(1), cellstyle, 12);
                        sheet.GetRow(1).GetCell(0).SetCellValue($"Пользователь обновил резюме менее 2 недель назад - {groupedResumes[1].Count} резюме");

                        var underlinecellstyle = workbook.CreateCellStyle();
                        var underlinefont = workbook.CreateFont();
                        underlinefont.Color = IndexedColors.LightBlue.Index;
                        underlinefont.Underline = FontUnderlineType.Single;
                        underlinecellstyle.SetFont(underlinefont);
                        var rowindex = 2;

                        this.SetStyleToAll(sheet.GetRow(1), cellstyle, 12);
                        sheet.GetRow(1).GetCell(0).SetCellValue($"Пользователь обновил резюме менее 2 недель назад - {groupedResumes[1].Count} резюме");
                        this.WriteLinkResumesInSheet(sheet, rowindex, groupedResumes[1], underlinecellstyle);

                        this.SetStyleToAll(sheet.GetRow(rowindex), cellstyle, 12);
                        sheet.GetRow(rowindex++).GetCell(0).SetCellValue($"Пользователь обновил резюме от 2 недель до 3 месяцев - {groupedResumes[2].Count} резюме");
                        this.WriteLinkResumesInSheet(sheet, rowindex, groupedResumes[2], underlinecellstyle);

                        this.SetStyleToAll(sheet.GetRow(rowindex), cellstyle, 12);
                        sheet.GetRow(rowindex++).GetCell(0).SetCellValue($"Пользователь обновил резюме более 3 месяцев назад - {groupedResumes[3].Count} резюме");
                        this.WriteLinkResumesInSheet(sheet, rowindex, groupedResumes[3], underlinecellstyle);

                        this.SetStyleToAll(sheet.GetRow(rowindex), cellstyle, 12);
                        sheet.GetRow(rowindex++).GetCell(0).SetCellValue($"Пользователь сейчас ищет работу - {groupedResumes[0].Count} резюме, упорядочены по дате обновления резюме");
                        this.WriteLinkResumesInSheet(sheet, rowindex, groupedResumes[0], underlinecellstyle);

                        sheet.SetColumnWidth(0, 5384);  //A
                        sheet.SetColumnWidth(1, 769);   //B
                        sheet.SetColumnWidth(2, 5);     //C
                        sheet.SetColumnWidth(3, 3382);  //D
                        sheet.SetColumnWidth(4, 8691);  //E
                        sheet.SetColumnWidth(5, 1615);  //F
                        sheet.SetColumnWidth(6, 350);   //G
                        sheet.SetColumnWidth(7, 2700);  //H
                        sheet.SetColumnWidth(8, 2700);  //I
                        sheet.SetColumnWidth(9, 5);     //J
                        sheet.SetColumnWidth(10, 2000);  //K
                        sheet.SetColumnWidth(11, 2613); //L
                        sheet.SetColumnWidth(12, 1921);//M
                    }

                }

            }
            #endregion
            #region WriteTotalList
            {
                var sheet = workbook.CreateSheet("Общие данные");
                var allresumes = this.companiesResumesTrackerItems.SelectMany(t => t.CompanyResumes).ToList();
                allresumes.Sort((a, b) => b.ResumeUpdate.CompareTo(a.ResumeUpdate));
                var groupedResumes = this.SortResumes(allresumes);
                if (this.Tab5ParseType == 0)
                {
                    for (var i = 0; i < allresumes.Count + 5; i++)
                    {
                        var row = sheet.CreateRow(i);
                        for (var z = 0; z <= 14; z++)
                        {
                            row.CreateCell(z);
                        }
                    }
                    sheet.GetRow(0).GetCell(0).SetCellValue("Имя");
                    sheet.GetRow(0).GetCell(1).SetCellValue("Лет");
                    sheet.GetRow(0).GetCell(2).SetCellValue("Стаж");
                    sheet.GetRow(0).GetCell(3).SetCellValue("Город");
                    sheet.GetRow(0).GetCell(4).SetCellValue("Должность");
                    sheet.GetRow(0).GetCell(5).SetCellValue("Зарплата");
                    sheet.GetRow(0).GetCell(6).SetCellValue("Валюта");
                    sheet.GetRow(0).GetCell(7).SetCellValue("Размещено");
                    sheet.GetRow(0).GetCell(8).SetCellValue("Обновлено");
                    sheet.GetRow(0).GetCell(9).SetCellValue("Работает сейчас");
                    sheet.GetRow(0).GetCell(10).SetCellValue("Дата начала работы в компании");
                    sheet.GetRow(0).GetCell(11).SetCellValue("Работал до");
                    sheet.GetRow(0).GetCell(12).SetCellValue("Ссылка");
                    sheet.GetRow(0).GetCell(13).SetCellValue("Место работы в резюме");
                    sheet.GetRow(0).GetCell(14).SetCellValue("Новое");

                    var cellstyle = workbook.CreateCellStyle();
                    cellstyle.FillForegroundColor = IndexedColors.DarkRed.Index;
                    cellstyle.FillPattern = FillPattern.SolidForeground;
                    var font = workbook.CreateFont();
                    font.Color = IndexedColors.Yellow.Index;
                    font.FontHeight = 15;
                    cellstyle.SetFont(font);
                    var underlinecellstyle = workbook.CreateCellStyle();
                    var underlinefont = workbook.CreateFont();
                    underlinefont.Color = IndexedColors.LightBlue.Index;
                    underlinefont.Underline = FontUnderlineType.Single;
                    underlinecellstyle.SetFont(underlinefont);
                    var rowindex = 2;

                    this.SetStyleToAll(sheet.GetRow(1), cellstyle, 14);
                    sheet.GetRow(1).GetCell(0).SetCellValue($"Сотрудник работает в компании, обновил резюме менее 2 недель назад - {groupedResumes[1].Count} резюме");
                    rowindex = this.WriteCompaniesInSheet(sheet, rowindex, groupedResumes[1], underlinecellstyle);

                    this.SetStyleToAll(sheet.GetRow(rowindex), cellstyle, 14);
                    sheet.GetRow(rowindex++).GetCell(0).SetCellValue($"Сотрудник работает в компании, обновил резюме от 2 недель до 3 месяцев - {groupedResumes[2].Count} резюме");
                    rowindex = this.WriteCompaniesInSheet(sheet, rowindex, groupedResumes[2], underlinecellstyle);

                    this.SetStyleToAll(sheet.GetRow(rowindex), cellstyle, 14);
                    sheet.GetRow(rowindex++).GetCell(0).SetCellValue($"Указано что сотрудник работает в компании, обновил резюме более 3 месяцев назад - {groupedResumes[3].Count} резюме");
                    rowindex = this.WriteCompaniesInSheet(sheet, rowindex, groupedResumes[3], underlinecellstyle);

                    this.SetStyleToAll(sheet.GetRow(rowindex), cellstyle, 14);
                    sheet.GetRow(rowindex++).GetCell(0).SetCellValue($"Сотрудник работал ранее в компании, сейчас ищет работу - {groupedResumes[0].Count} резюме, упорядочены по дате обновления резюме");
                    rowindex = this.WriteCompaniesInSheet(sheet, rowindex, groupedResumes[0], underlinecellstyle);
                    sheet.SetColumnWidth(0, 5384);  //A
                    sheet.SetColumnWidth(1, 769);   //B
                    sheet.SetColumnWidth(2, 5);     //C
                    sheet.SetColumnWidth(3, 3382);  //D
                    sheet.SetColumnWidth(4, 8691);  //E
                    sheet.SetColumnWidth(5, 1615);  //F
                    sheet.SetColumnWidth(6, 350);   //G
                    sheet.SetColumnWidth(7, 2700);  //H
                    sheet.SetColumnWidth(8, 2700);  //I
                    sheet.SetColumnWidth(9, 5);     //J
                    sheet.SetColumnWidth(10, 5);  //K
                    sheet.SetColumnWidth(11, 2613); //L
                    sheet.SetColumnWidth(12, 1921);//M
                    sheet.SetColumnWidth(13, 12535);//N
                }
                else
                {
                    for (var i = 0; i < allresumes.Count + 10; i++)
                    {
                        var row = sheet.CreateRow(i);
                        for (var z = 0; z <= 12; z++)
                        {
                            row.CreateCell(z);
                        }
                    }
                    sheet.GetRow(0).GetCell(0).SetCellValue("Имя");
                    sheet.GetRow(0).GetCell(1).SetCellValue("Лет");
                    sheet.GetRow(0).GetCell(2).SetCellValue("Стаж");
                    sheet.GetRow(0).GetCell(3).SetCellValue("Город");
                    sheet.GetRow(0).GetCell(4).SetCellValue("Должность");
                    sheet.GetRow(0).GetCell(5).SetCellValue("Зарплата");
                    sheet.GetRow(0).GetCell(6).SetCellValue("Валюта");
                    sheet.GetRow(0).GetCell(7).SetCellValue("Размещено");
                    sheet.GetRow(0).GetCell(8).SetCellValue("Обновлено");
                    sheet.GetRow(0).GetCell(9).SetCellValue("Работает сейчас");
                    sheet.GetRow(0).GetCell(10).SetCellValue("Места работы");
                    sheet.GetRow(0).GetCell(11).SetCellValue("Ссылка");
                    sheet.GetRow(0).GetCell(12).SetCellValue("Новое");

                    var cellstyle = workbook.CreateCellStyle();
                    cellstyle.FillForegroundColor = IndexedColors.DarkRed.Index;
                    cellstyle.FillPattern = FillPattern.SolidForeground;
                    var font = workbook.CreateFont();
                    font.Color = IndexedColors.Yellow.Index;
                    font.FontHeight = 15;
                    cellstyle.SetFont(font);
                    this.SetStyleToAll(sheet.GetRow(1), cellstyle, 12);
                    sheet.GetRow(1).GetCell(0).SetCellValue($"Пользователь обновил резюме менее 2 недель назад - {groupedResumes[1].Count} резюме");
                    var underlinecellstyle = workbook.CreateCellStyle();
                    var underlinefont = workbook.CreateFont();
                    underlinefont.Color = IndexedColors.LightBlue.Index;
                    underlinefont.Underline = FontUnderlineType.Single;
                    underlinecellstyle.SetFont(underlinefont);
                    var rowindex = 2;

                    this.SetStyleToAll(sheet.GetRow(1), cellstyle, 12);
                    sheet.GetRow(1).GetCell(0).SetCellValue($"Пользователь обновил резюме менее 2 недель назад - {groupedResumes[1].Count} резюме");
                    this.WriteLinkResumesInSheet(sheet, rowindex, groupedResumes[1], underlinecellstyle);

                    this.SetStyleToAll(sheet.GetRow(rowindex), cellstyle, 12);
                    sheet.GetRow(rowindex++).GetCell(0).SetCellValue($"Пользователь обновил резюме от 2 недель до 3 месяцев - {groupedResumes[2].Count} резюме");
                    this.WriteLinkResumesInSheet(sheet, rowindex, groupedResumes[2], underlinecellstyle);

                    this.SetStyleToAll(sheet.GetRow(rowindex), cellstyle, 12);
                    sheet.GetRow(rowindex++).GetCell(0).SetCellValue($"Пользователь обновил резюме более 3 месяцев назад - {groupedResumes[3].Count} резюме");
                    this.WriteLinkResumesInSheet(sheet, rowindex, groupedResumes[3], underlinecellstyle);

                    this.SetStyleToAll(sheet.GetRow(rowindex), cellstyle, 12);
                    sheet.GetRow(rowindex++).GetCell(0).SetCellValue($"Пользователь сейчас ищет работу - {groupedResumes[0].Count} резюме, упорядочены по дате обновления резюме");
                    this.WriteLinkResumesInSheet(sheet, rowindex, groupedResumes[0], underlinecellstyle);

                    sheet.SetColumnWidth(0, 5384);  //A
                    sheet.SetColumnWidth(1, 769);   //B
                    sheet.SetColumnWidth(2, 5);     //C
                    sheet.SetColumnWidth(3, 3382);  //D
                    sheet.SetColumnWidth(4, 8691);  //E
                    sheet.SetColumnWidth(5, 1615);  //F
                    sheet.SetColumnWidth(6, 350);   //G
                    sheet.SetColumnWidth(7, 2700);  //H
                    sheet.SetColumnWidth(8, 2700);  //I
                    sheet.SetColumnWidth(9, 5);     //J
                    sheet.SetColumnWidth(10, 2000);  //K
                    sheet.SetColumnWidth(11, 2613); //L
                    sheet.SetColumnWidth(12, 1921);//M



                }
            }
            #endregion

            var companies = workbook.CreateSheet("Не редактировать!!!");
            var serializer = new JsonSerializer();
            var sw = new StringWriter();
            serializer.Serialize(sw, this.companiesResumesTrackerItems, typeof(List<CompanySearchItem>));
            sw.Dispose();
            companies.CreateRow(0).CreateCell(0);
            var json = sw.ToString();
            companies.GetRow(0).GetCell(0).SetCellValue(json);
            var companyName1 = this.companiesResumesTrackerItems[0].Name;
            if (this.companiesResumesTrackerItems[0].type == 1)
            {
                var variables1 = companyName1.Split('?')[1].Split('&');
                var textVariable1 = variables1.Where(t => t.StartsWith("text=")).ToList();
                if (textVariable1.Count > 0)
                {
                    companyName1 = "Результат - " + System.Web.HttpUtility.UrlDecode(textVariable1[0].Substring(5));
                }
            }


            var fs = new FileStream(sfd.SelectedPath + "\\" + companyName1.Replace(":", "").Replace("?", "").Replace("/", "")
                                    .Replace("\\", "").Replace("\"", "").Replace("*", "").Replace("<", "")
                                    .Replace(">", "").Replace("|", "") + ".xlsx", FileMode.Create);
            workbook.Write(fs);
            var htmlpath = sfd.SelectedPath + "\\htmls " + companyName1.Replace(":", "").Replace("?", "").Replace("/", "")
                                    .Replace("\\", "").Replace("\"", "").Replace("*", "").Replace("<", "")
                                    .Replace(">", "").Replace("|", "");
            if (Directory.Exists(Application.StartupPath + "\\htmls") && Directory.GetFiles(Application.StartupPath + "\\htmls").Length > 0)
            {
                if (Directory.Exists(htmlpath))
                {
                    MessageBox.Show("Папка \"" + htmlpath + "\" уже существует.Удалите или переименуйте ее и нажмите на ОК", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                Directory.Move(Application.StartupPath + "\\htmls", htmlpath);
            }
        }
        private void LoadTrackerCompanies_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                Filter = "XLSX Files|*.xlsx"
            };
            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                var workbook = new XSSFWorkbook(ofd.FileName);
                var serializer = new JsonSerializer();
                var stringvalue = workbook.GetSheetAt(workbook.NumberOfSheets - 1).GetRow(0).GetCell(0).StringCellValue;
                JsonReader reader = new JsonTextReader(new StringReader(stringvalue));
                this.companiesResumesTrackerItems = serializer.Deserialize<List<CompanySearchItem>>(reader);
                if (this.companiesResumesTrackerItems[0].type == 0)
                {
                    this.ParseByListTab5.Checked = true;
                    for (var i = 1; i < workbook.NumberOfSheets - 2; i++)
                    {
                        var sheet = workbook.GetSheetAt(i);
                        this.companiesResumesTrackerItems[i - 1].CompanyResumes = new List<Resume>();
                        for (var z = 1; z < sheet.PhysicalNumberOfRows; z++)
                        {
                            try
                            {
                                var resume = new Resume();
                                var row = sheet.GetRow(z);
                                var cell0 = row.GetCell(0).GetCellValue();
                                if (cell0.StartsWith("Сотрудник"))
                                {
                                    continue;
                                }
                                var cell1 = row.GetCell(1).GetCellValue();
                                var cell2 = row.GetCell(2).GetCellValue();
                                var cell3 = row.GetCell(3).GetCellValue();
                                var cell4 = row.GetCell(4).GetCellValue();
                                var cell5 = row.GetCell(5).GetCellValue();
                                var cell6 = row.GetCell(6).GetCellValue();
                                var cell7 = row.GetCell(7).GetCellValue();
                                var cell8 = row.GetCell(8).GetCellValue();
                                var cell9 = row.GetCell(9).GetCellValue();
                                var cell10 = row.GetCell(10).GetCellValue();
                                var cell11 = row.GetCell(11).GetCellValue();
                                var cell12 = row.GetCell(12).GetCellValue();
                                var cell13 = row.GetCell(13).GetCellValue();
                                resume.OwnerName = cell0;
                                resume.Age = cell1;
                                resume.WorkingSummary = cell2;
                                resume.WorkingCity = cell3;
                                resume.Position = cell4;
                                resume.Salary = cell5;
                                resume.SalaryCurrency = cell6;
                                resume.ResumeCreated = row.GetCell(7).DateCellValue;
                                resume.ResumeUpdate = DateTime.ParseExact(row.GetCell(8).StringCellValue, "d MMM yyyy", new CultureInfo("ru-RU", true));
                                resume.WorkingNow = row.GetCell(9).BooleanCellValue;
                                resume.Start = row.GetCell(10).DateCellValue;
                                resume.End = row.GetCell(11).DateCellValue;
                                var r = new Work
                                {
                                    Start = row.GetCell(10).DateCellValue,
                                    End = row.GetCell(11).DateCellValue,
                                    Name = row.GetCell(13).StringCellValue
                                };
                                resume.WorkedPlaces.Add(r);
                                resume.Link = row.GetCell(12).Hyperlink.Address;
                                this.companiesResumesTrackerItems[i - 1].CompanyResumes.Add(resume);
                            }
                            catch
                            {

                            }
                        }
                    }
                }
                else
                {
                    for (var i = 1; i < workbook.NumberOfSheets - 2; i++)
                    {
                        var sheet = workbook.GetSheetAt(i);
                        this.companiesResumesTrackerItems[i - 1].CompanyResumes = new List<Resume>();
                        for (var z = 1; z < sheet.PhysicalNumberOfRows; z++)
                        {
                            try
                            {
                                var resume = new Resume();
                                var row = sheet.GetRow(z);
                                var cell0 = row.GetCell(0).GetCellValue();
                                if (cell0.StartsWith("Пользователь"))
                                {
                                    continue;
                                }
                                var cell1 = row.GetCell(1).GetCellValue();
                                var cell2 = row.GetCell(2).GetCellValue();
                                var cell3 = row.GetCell(3).GetCellValue();
                                var cell4 = row.GetCell(4).GetCellValue();
                                var cell5 = row.GetCell(5).GetCellValue();
                                var cell6 = row.GetCell(6).GetCellValue();
                                var cell7 = row.GetCell(7).GetCellValue();
                                var cell8 = row.GetCell(8).GetCellValue();
                                var cell9 = row.GetCell(9).GetCellValue();
                                var cell10 = row.GetCell(10).GetCellValue();
                                var cell11 = row.GetCell(11).GetCellValue();
                                var cell12 = row.GetCell(12).GetCellValue();
                                var cell13 = row.GetCell(13).GetCellValue();
                                resume.OwnerName = cell0;
                                resume.Age = cell1;
                                resume.WorkingSummary = cell2;
                                resume.WorkingCity = cell3;
                                resume.Position = cell4;
                                resume.Salary = cell5;
                                resume.SalaryCurrency = cell6;
                                resume.ResumeCreated = row.GetCell(7).DateCellValue;
                                resume.ResumeUpdate = DateTime.ParseExact(row.GetCell(8).StringCellValue, "d MMM yyyy", new CultureInfo("ru-RU", true));
                                resume.WorkingNow = row.GetCell(9).BooleanCellValue;
                                //resume.Start = row.GetCell(10).DateCellValue;
                                //resume.End = row.GetCell(11).DateCellValue;
                                //var r = new Work
                                //{
                                //    Start = row.GetCell(10).DateCellValue,
                                //    End = row.GetCell(11).DateCellValue,
                                //    Name = row.GetCell(13).StringCellValue
                                //};
                                //resume.WorkedPlaces.Add(r);
                                resume.Link = row.GetCell(11).Hyperlink.Address;
                                this.companiesResumesTrackerItems[i - 1].CompanyResumes.Add(resume);
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                    this.ParseByLinkTab5.Checked = true;
                }
            }
        }
        private void ParseByListTab5_CheckedChanged(object sender, EventArgs e)
        {
            this.EditCompaniesList.Text = "Добавить компании";
            this.metroLabel2.Text = "Компания:";
            this.Tab5ParseType = 0;
            this.companiesResumesTrackerItems.Clear();
        }
        private void ParseByLinkTab5_CheckedChanged(object sender, EventArgs e)
        {
            this.EditCompaniesList.Text = "Указать ссылку";
            this.metroLabel2.Text = "-------------";
            this.Tab5ParseType = 1;
            this.companiesResumesTrackerItems.Clear();
        }
        private string DateToString(DateTime dt)
        {
            var month = dt.Month;
            var c = new CultureInfo("ru-RU", true);
            return dt.ToString($"d MMM yyyy", c);
        }
    }

    public class OgrnCompany
    {
        public string CasesCount;
        public string Inn;
        public string Ogrn;
        public string Okpo;
        public string PersonalAverage;
        public string RegistrationDate;
        public string Revenue;
        public string Status;
        public string WebSiteUrl;
    }
}