using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace HH_Parser
{
    public partial class ReportGeneratorForm : Form
    {
        public ReportGeneratorForm(List<User> users)
        {
            InitializeComponent();
            Users = users;
            label4.Text = users.Count.ToString();
        }

        private List<User> Users;
        private HtmlAgilityPack.HtmlDocument ReportFile;

        private HtmlAgilityPack.HtmlNode CreateAndGetTable()
        {
            return HtmlAgilityPack.HtmlNode.CreateNode("<table><thead><tr><th style=\"width:5%\">№</th><th style=\"width:32%;\">Фио</th><th style=\"width:29%;\">Должность</th><th style=\"width:10%\">Обновление</th><th style=\"width:10%\">Размещено</th><th style=\"width:11%\">Ссылка</th><th style=\"width:3%\">Символ</th></tr></thead></table>");
        }

        private string[] GetSymbol(User user)
        {
            string ClassAttr = null;
            string Symbol = null;
            if (user.ResumeFound == "+")
            {
                if (user.FirstlyParsedDate != DateTime.MinValue && DateTime.Now.Subtract(user.FirstlyParsedDate).TotalDays < 30)
                {
                    ClassAttr = "RCLTOMAC";
                    Symbol = "!";
                }
                else if (user.LastResume.ResumeUpdate != DateTime.MinValue && DateTime.Now.Subtract(user.LastResume.ResumeUpdate).TotalDays < 30)
                {
                    ClassAttr = "";
                    Symbol = "◊";
                }
                else if (user.LastResume.ResumeUpdate != DateTime.MinValue && DateTime.Now.Subtract(user.LastResume.ResumeUpdate).TotalDays > 30)
                {
                    ClassAttr = "RULTOMAC";
                    Symbol = "♦";
                }
            }
            return new string[] { ClassAttr, Symbol };
        }
        private void AddDepartmentToDiv(HtmlAgilityPack.HtmlNode div, string Office, List<User> workers, bool even)
        {
            var CompanieWorkers = HtmlAgilityPack.HtmlNode.CreateNode("<div></div>");
            CompanieWorkers.AddClass("CompanieWorkers");
            var param1 = HtmlAgilityPack.HtmlNode.CreateNode(string.Format("<p>{0}</p>", workers.Count));
            param1.AddClass("param1");
            var param2 = HtmlAgilityPack.HtmlNode.CreateNode(string.Format("<p>{0}</p>", workers.Count(t => t.ResumeFound == "+")));
            param2.AddClass("param2");
            var param3 = HtmlAgilityPack.HtmlNode.CreateNode(string.Format("<p>{0}</p>", workers.Count(t => t.ResumeFound == "+" && t.FirstlyParsedDate.Subtract(t.LastResume.ResumeUpdate).TotalDays < 30)));
            param3.AddClass("param3");
            CompanieWorkers.AppendChild(param1);
            CompanieWorkers.AppendChild(param2);
            CompanieWorkers.AppendChild(param3);
            div.AppendChild(CompanieWorkers);
            var OfficeDepartmentsDetails = HtmlAgilityPack.HtmlNode.CreateNode("<details></details>");
            var OfficeDepartmentsSummary = HtmlAgilityPack.HtmlNode.CreateNode($"<summary class=\"department\">{Office}</summary>");
            OfficeDepartmentsDetails.AppendChild(OfficeDepartmentsSummary);
            var table = CreateAndGetTable();
            if (!even)
            {
                table.ChildNodes[0].ChildNodes[0].Attributes.Add("class", "color-grey");
            }
            var tablebody = HtmlAgilityPack.HtmlNode.CreateNode("<tbody></tbody>");
            var count = 1;
            foreach (var item in workers)
            {
                var tr = HtmlAgilityPack.HtmlNode.CreateNode("<tr></tr>");
                if (even)
                {
                    tr.Attributes.Add("class", "color-grey");
                }
                var indextd = HtmlAgilityPack.HtmlNode.CreateNode(string.Format("<td>{0}</td>", count));
                var nametd = HtmlAgilityPack.HtmlNode.CreateNode(string.Format("<td>{0}</td>", !string.IsNullOrEmpty(item.SearchName) ? item.SearchName : item.LoadedName));
                var positiontd = HtmlAgilityPack.HtmlNode.CreateNode(string.Format("<td>{0}</td>", item.Position));
                var symbol = GetSymbol(item);
                var updatetd = HtmlAgilityPack.HtmlNode.CreateNode(string.Format("<td>{0}</td>", item.LastResume?.ResumeUpdate == default(DateTime) ? "-" : item.LastResume?.ResumeUpdate.ToShortDateString()));
                if (symbol[0] != "")
                {
                    updatetd.AddClass(symbol[0]);
                }
                var createdtd = HtmlAgilityPack.HtmlNode.CreateNode(string.Format("<td>{0}</td>", item.FirstlyParsedDate == default(DateTime) ? "-" : item.FirstlyParsedDate.ToShortDateString()));
                var linktd = HtmlAgilityPack.HtmlNode.CreateNode(string.Format("<td>{0}</td>", item.ResumeFound == "+" ? string.Format("<a href=\"{0}\">Ссылка</a>", item.LastResume.Link) : "-"));
                var symboltd = HtmlAgilityPack.HtmlNode.CreateNode(string.Format("<td>{0}</td>", symbol[1]));
                if (symbol[0] != "")
                {
                    symboltd.AddClass(symbol[0]);
                }
                tr.AppendChild(indextd);
                tr.AppendChild(nametd);
                tr.AppendChild(positiontd);
                tr.AppendChild(updatetd);
                tr.AppendChild(createdtd);
                tr.AppendChild(linktd);
                tr.AppendChild(symboltd);
                tablebody.AppendChild(tr);
                even = !even;
                count++;
            }
            table.AppendChild(tablebody);
            OfficeDepartmentsDetails.AppendChild(table);
            div.AppendChild(OfficeDepartmentsDetails);
        }

        private void AddOfficeInDiv(HtmlAgilityPack.HtmlNode div, string Office, List<User> workers)
        {
            var CompanieWorkers = HtmlAgilityPack.HtmlNode.CreateNode("<div></div>");
            CompanieWorkers.AddClass("CompanieWorkers");
            var param1 = HtmlAgilityPack.HtmlNode.CreateNode(string.Format("<p>{0}</p>", workers.Count));
            param1.AddClass("param1");
            var param2 = HtmlAgilityPack.HtmlNode.CreateNode(string.Format("<p>{0}</p>", workers.Count(t => t.ResumeFound == "+")));
            param2.AddClass("param2");
            var param3 = HtmlAgilityPack.HtmlNode.CreateNode(string.Format("<p>{0}</p>", workers.Count(t => t.ResumeFound == "+" && t.FirstlyParsedDate.Subtract(t.LastResume.ResumeUpdate).TotalDays < 30)));
            param3.AddClass("param3");
            CompanieWorkers.AppendChild(param1);
            CompanieWorkers.AppendChild(param2);
            CompanieWorkers.AppendChild(param3);
            div.AppendChild(CompanieWorkers);
            var OfficeDepartmentsDetails = HtmlAgilityPack.HtmlNode.CreateNode("<details open></details>");
            var OfficeDepartmentsSummary = HtmlAgilityPack.HtmlNode.CreateNode(string.Format("<summary class=\"companyofficesummary\">ОТДЕЛЕНИЕ - {0}</summary>", Office));
            OfficeDepartmentsDetails.AppendChild(OfficeDepartmentsSummary);
            div.AppendChild(OfficeDepartmentsDetails);
            var departments = workers.GroupBy(t => t.Department);
            var index = 0;
            foreach (var item in departments)
            {
                if (item.Key != null)
                {
                    var OfficeDiv = HtmlAgilityPack.HtmlNode.CreateNode("<div></div>");
                    AddDepartmentToDiv(OfficeDiv, item.Key, item.ToList(), index % 2 == 0);
                    OfficeDepartmentsDetails.AppendChild(OfficeDiv);
                    index++;
                }
            }
        }
        private void CreateReport()
        {
            ReportFile.DocumentNode.SelectSingleNode("/html/body/div[1]/p").InnerHtml = "Отчет от " + textBox2.Text;
            ReportFile.DocumentNode.SelectSingleNode("/html/body/div[3]/div/details/summary").InnerHtml = string.Format("КОМПАНИЯ - \"{0}\"", textBox1.Text);

            ReportFile.DocumentNode.SelectSingleNode("/html/body/div[2]/div[1]/span").InnerHtml = Users.Count.ToString();
            ReportFile.DocumentNode.SelectSingleNode("/html/body/div[2]/div[2]/span").InnerHtml = Users.Count(t => t.ResumeFound == "+").ToString();
            ReportFile.DocumentNode.SelectSingleNode("/html/body/div[2]/div[3]/span").InnerHtml = Users.Count(t => t.ResumeFound == "+" && t.FirstlyParsedDate.Subtract(t.LastResume.ResumeUpdate).TotalDays < 30).ToString();
            ReportFile.DocumentNode.SelectSingleNode("/html/body/div[3]/div/div/p[1]").InnerHtml = Users.Count.ToString();
            ReportFile.DocumentNode.SelectSingleNode("/html/body/div[3]/div/div/p[2]").InnerHtml = Users.Count(t => t.ResumeFound == "+").ToString();
            ReportFile.DocumentNode.SelectSingleNode("/html/body/div[3]/div/div/p[3]").InnerHtml = Users.Count(t => t.ResumeFound == "+" && t.FirstlyParsedDate.Subtract(t.LastResume.ResumeUpdate).TotalDays < 30).ToString();

            var CompanyNode = ReportFile.DocumentNode.SelectSingleNode("/html/body/div[3]/div/details");
            var Offices = Users.GroupBy(t => t.Office);
            foreach (var item in Offices)
            {
                if (item.Key != null)
                {
                    var OfficeDiv = HtmlAgilityPack.HtmlNode.CreateNode("<div></div>");
                    AddOfficeInDiv(OfficeDiv, item.Key, item.ToList());
                    CompanyNode.AppendChild(OfficeDiv);
                }
            }
        }
        private void GenerateAndSaveReport_Click(object sender, EventArgs e)
        {
            var savedialog = new FolderBrowserDialog()
            {
            };
            if (savedialog.ShowDialog() == DialogResult.OK)
            {
                ReportFile = new HtmlAgilityPack.HtmlDocument();
                ReportFile.Load(Application.StartupPath + "\\EmptyReports\\Report Георгий.html");
                CreateReport();
                Save(savedialog.SelectedPath + "\\Report Георгий.html");
                ReportFile = new HtmlAgilityPack.HtmlDocument();
                ReportFile.Load(Application.StartupPath + "\\EmptyReports\\Report Климментий.html");
                CreateReport();
                Save(savedialog.SelectedPath + "\\Report Климментий.html");
                ReportFile = new HtmlAgilityPack.HtmlDocument();
                ReportFile.Load(Application.StartupPath + "\\EmptyReports\\Report Станислав.html");
                CreateReport();
                Save(savedialog.SelectedPath + "\\Report Станислав.html");
            }
        }
        public void Save(string filename)
        {
            ReportFile.Save(filename);
        }
    }
}
