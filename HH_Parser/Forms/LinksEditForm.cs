using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace HH_Parser
{
    public partial class LinksEditForm : Form
    {
        public LinksEditForm(List<CompanySearchItem> companyitems)
        {
            this.CompanyItems = companyitems;
            this.InitializeComponent();
            foreach (var item in this.CompanyItems)
            {
                this.Links.AppendText(item.Name + Environment.NewLine);
            }
            Links.TextChanged += Links_TextChanged;
        }

        private List<CompanySearchItem> CompanyItems;
        private void companyfileinfo_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Функция \"Парсинг резюме по ссылке\"\r" +
    "В поле ниже необходимо указать ссылку или ссылки на страницы поиска резюме,к примеру\r" +
    "https://hh.ru/search/resume?text=%D0%94%D0%BC%D0%B8%D1%82%D1%80%D0%B8%D0%B9&clusters=true&exp_period=all_time&logic=normal&pos=full_text&clusters=true&area=1&order_by=relevance&no_magic=false\r" +
    "ВАЖНО: ссылка должна быть с первой страницы поиска,что бы в ней не был задействован параметр page", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void LinksEditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var item in CompanyItems)
            {
                if (item.Name.Contains("&page="))
                {
                    e.Cancel = true;
                    MessageBox.Show("Одна или несколько ссылок содержат параметр page,пожалуйста удалите его");
                }
            }
        }

        private void Links_TextChanged(object sender, EventArgs e)
        {
            CompanyItems.Clear();
            foreach (var item in Links.Lines)
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                CompanySearchItem it = new CompanySearchItem();
                it.Name = item;
                it.type = 1;
                CompanyItems.Add(it);
            }
        }
    }
}
