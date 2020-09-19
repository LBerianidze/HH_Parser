using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UsersGenerator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private List<User> users = new List<User>();
        private HtmlAgilityPack.HtmlDocument document;
        private void Save_Click(object sender, EventArgs e)
        {
            this.users = new List<User>();
            List<string> UserNames = this.richTextBox1.Lines.ToList();
            List<string> Offices = this.richTextBox2.Lines.ToList();
            List<string> Departments = this.richTextBox3.Lines.ToList();
            List<string> Positions = this.richTextBox4.Lines.ToList();

            Random random = new Random();
            for (int i = 0; i < UserNames.Count; i++)
            {
                User us = new User
                {
                    Name = UserNames[i],
                    Department = Departments[random.Next(0, Departments.Count)],
                    Position = Positions[random.Next(0, Positions.Count)],
                    Office = Offices[random.Next(0, Offices.Count)],
                    Found = random.Next(0, 2) == 1 ? true : false
                };
                this.users.Add(us);
            }
            LoadFile();
            SetDate();
            SetCompanieName();
            SetCompanieAllEmployersCount();
            SetCompanieEmployersWithResumeCount();
            SetCompanieEmployersWithNewResumeCount();
            CreateCompaniesNode();
            SaveFile();
        }
        public void CreateCompaniesNode()
        {
            HtmlAgilityPack.HtmlNode CompaniesHeaderDiv = this.document.DocumentNode.SelectSingleNode("/html/body/div[4]");
            HtmlAgilityPack.HtmlNode CompanieDiv = new HtmlAgilityPack.HtmlNode(HtmlAgilityPack.HtmlNodeType.Element, CompaniesHeaderDiv.OwnerDocument, 0)
            {
                Name = "div"
            };
            CompaniesHeaderDiv.ChildNodes.Add(CompanieDiv);
        }
        public void SetCompanieAllEmployersCount()
        {
            HtmlAgilityPack.HtmlNode datep = this.document.DocumentNode.SelectSingleNode("/html/body/div[3]/p/span[1]");
            HtmlAgilityPack.HtmlNode textnode = HtmlAgilityPack.HtmlTextNode.CreateNode(this.users.Count.ToString());
            datep.ReplaceChild(textnode, datep.ChildNodes[0]);
        }
        public void SetCompanieEmployersWithResumeCount()
        {
            HtmlAgilityPack.HtmlNode datep = this.document.DocumentNode.SelectSingleNode("/html/body/div[3]/p/span[2]");
            HtmlAgilityPack.HtmlNode textnode = HtmlAgilityPack.HtmlTextNode.CreateNode(this.users.Count(t => t.Found).ToString());
            datep.ReplaceChild(textnode, datep.ChildNodes[0]);
        }
        public void SetCompanieEmployersWithNewResumeCount()
        {
            HtmlAgilityPack.HtmlNode datep = this.document.DocumentNode.SelectSingleNode("/html/body/div[3]/p/span[3]");
            HtmlAgilityPack.HtmlNode textnode = HtmlAgilityPack.HtmlTextNode.CreateNode(this.users.Count(t => t.Found).ToString());
            datep.ReplaceChild(textnode, datep.ChildNodes[0]);
        }
        public void SetCompanieName()
        {
            HtmlAgilityPack.HtmlNode datep = this.document.DocumentNode.SelectSingleNode("/html/body/div[2]/p");
            HtmlAgilityPack.HtmlNode textnode = HtmlAgilityPack.HtmlTextNode.CreateNode(string.Format("Группа компаний \"{0}\"", this.textBox1.Text));
            datep.ReplaceChild(textnode, datep.ChildNodes[0]);
        }
        public void SetDate()
        {
            HtmlAgilityPack.HtmlNode datep = this.document.DocumentNode.SelectSingleNode("/html/body/div[1]/p[2]");
            HtmlAgilityPack.HtmlNode textnode = HtmlAgilityPack.HtmlTextNode.CreateNode("От " + this.textBox2.Text);
            datep.ReplaceChild(textnode, datep.ChildNodes[0]);
        }
        public void LoadFile()
        {
            this.document = new HtmlAgilityPack.HtmlDocument();
            this.document.Load(@"C:\Users\Luka\Desktop\Report.html", true);
        }
        public void SaveFile()
        {
            this.document.Save(@"C:\Users\Luka\Desktop\Report1.html", Encoding.UTF8);
        }
    }
    public class User
    {
        public string Name;
        public string Office;
        public string Department;
        public string Position;
        public string ResumeCreateDate = "04.18";
        public string ResumeUpdateDate = "01.19";
        public string Link = "https://itvdn.ru";
        public bool Found = false;
    }
}
