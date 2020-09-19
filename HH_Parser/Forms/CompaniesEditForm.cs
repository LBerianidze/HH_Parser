using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HH_Parser
{
    public partial class CompaniesEditForm : Form
    {
        private List<CompanySearchItem> CompanyItems;

        public CompaniesEditForm(List<CompanySearchItem> companyitems)
        {
            this.CompanyItems = companyitems;
            this.InitializeComponent();
            foreach (var item in this.CompanyItems)
            {
                this.CompaniesListbox.Items.Add(item.Name);
            }
            if (this.CompaniesListbox.Items.Count != 0)
            {
                this.CompaniesListbox.SelectedIndex = 0;
                this.groupBox3.Enabled = true;
            }
        }

        private void AddCompany_Click(object sender, EventArgs e)
        {
            this.CompanyItems.Add(new CompanySearchItem() { Name = "Компания", AlternativeNames = new List<string>(), Places = new List<Place>(), CompanyResumes = new List<Resume>() });
            this.CompaniesListbox.Items.Add("Компания");
            if (this.CompaniesListbox.Items.Count == 1)
            {
                this.CompaniesListbox.SelectedIndex = 0;
            }
            this.groupBox3.Enabled = true;
        }

        private void DeleteCompany_Click(object sender, EventArgs e)
        {
            this.CompanyItems.RemoveAt(this.CompaniesListbox.SelectedIndex);
            var currentindex = this.CompaniesListbox.SelectedIndex;
            this.CompaniesListbox.Items.RemoveAt(this.CompaniesListbox.SelectedIndex);
            if (this.CompanyItems.Count == 0)
            {
                this.groupBox3.Enabled = false;
            }
            else
            {
                var newindex = currentindex;
                if (newindex >= 0 && newindex < this.CompaniesListbox.Items.Count)
                {
                    this.CompaniesListbox.SelectedIndex = newindex;
                }
                else
                {
                    newindex--;
                    if (newindex >= 0 && newindex < this.CompaniesListbox.Items.Count)
                    {
                        this.CompaniesListbox.SelectedIndex = newindex;
                    }
                    else
                    {
                        this.CompaniesListbox.SelectedIndex = 0;
                    }
                }
            }
        }

        private void CompanyName_TextChanged(object sender, EventArgs e)
        {
            if (this.CompaniesListbox.SelectedIndex == -1)
            {
                return;
            }
            this.CompanyItems[this.CompaniesListbox.SelectedIndex].Name = this.CompanyName.Text;
            this.CompaniesListbox.Items[this.CompaniesListbox.SelectedIndex] = this.CompanyName.Text;
        }

        private void CompanyWebsite_TextChanged(object sender, EventArgs e)
        {
            if (this.CompaniesListbox.SelectedIndex == -1)
            {
                return;
            }
            this.CompanyItems[this.CompaniesListbox.SelectedIndex].Website = this.CompanyWebsite.Text;
        }

        private void CheckByWebsite_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CompaniesListbox.SelectedIndex == -1)
            {
                return;
            }
            this.CompanyItems[this.CompaniesListbox.SelectedIndex].CheckOnWebsite = this.CheckByWebsite.Checked;
        }

        private void CheckByWebsiteCount_TextChanged(object sender, EventArgs e)
        {
            if (this.CompaniesListbox.SelectedIndex == -1)
            {
                return;
            }
            Int32.TryParse(this.CheckByWebsiteCount.Text, out var count);
            if (count != 0)
            {
                this.CompanyItems[this.CompaniesListbox.SelectedIndex].CheckOnWebsiteCount = count;
                if (count < 1)
                {
                    this.CompanyItems[this.CompaniesListbox.SelectedIndex].CheckOnWebsiteCount = 1;
                    this.CheckByWebsiteCount.Text = "1";
                }
                this.label5.Visible = false;
            }
            else if (this.CheckByWebsiteCount.Text == "0")
            {
                this.CompanyItems[this.CompaniesListbox.SelectedIndex].CheckOnWebsiteCount = 1;
                this.CheckByWebsiteCount.Text = "1";
                this.label5.Visible = false;
            }
            else
            {
                this.label5.Visible = true;
            }
        }

        private void CompaniesListbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.CompaniesListbox.SelectedIndex == -1)
            {
                return;
            }
            this.CompanyName.Text = this.CompanyItems[this.CompaniesListbox.SelectedIndex].Name;
            this.CompanyWebsite.Text = this.CompanyItems[this.CompaniesListbox.SelectedIndex].Website;
            this.CheckByWebsite.Checked = this.CompanyItems[this.CompaniesListbox.SelectedIndex].CheckOnWebsite;
            this.CheckByWebsiteCount.Text = this.CompanyItems[this.CompaniesListbox.SelectedIndex].CheckOnWebsiteCount.ToString();
            this.YearsLimit.Checked = this.CompanyItems[this.CompaniesListbox.SelectedIndex].RestrictByDate;
            this.YearsNumeric.Value = this.CompanyItems[this.CompaniesListbox.SelectedIndex].RestrictYears;
            this.MonthesNumeric.Value = this.CompanyItems[this.CompaniesListbox.SelectedIndex].RestrictMonthes;
            var places = this.CompanyItems[this.CompaniesListbox.SelectedIndex].Places?.Select(t => t.Name)?.ToList();
            if (places?.Count > 0)
            {
                this.ResumeByCompanyParseSelectCities.Text = String.Join(",", places);
            }
            else
            {
                this.ResumeByCompanyParseSelectCities.Text = "";

            }
            this.AlternativeNames.Lines = this.CompanyItems[this.CompaniesListbox.SelectedIndex].AlternativeNames.ToArray();
            this.ParseWithMetros.Checked = this.CompanyItems[this.CompaniesListbox.SelectedIndex].ParseByMetro;
            this.MinMetroCount.Value = this.CompanyItems[this.CompaniesListbox.SelectedIndex].MinResumesCount;
        }

        private void AlternativeNames_Leave(object sender, EventArgs e)
        {
            if (this.CompaniesListbox.SelectedIndex == -1)
            {
                return;
            }
            this.CompanyItems[this.CompaniesListbox.SelectedIndex].AlternativeNames = this.AlternativeNames.Lines.Where(t => !String.IsNullOrEmpty(t)).ToList();
            this.AlternativeNames.Lines = this.CompanyItems[this.CompaniesListbox.SelectedIndex].AlternativeNames.ToArray();
        }

        private void ResumeByCompanyParseSelectCities_DoubleClick(object sender, EventArgs e)
        {
            if (this.CompaniesListbox.SelectedIndex == -1)
            {
                return;
            }
            var window = new SelectCitiesWindow();
            var companiesCities = window.ShowDialog(this);
            this.CompanyItems[this.CompaniesListbox.SelectedIndex].Places = companiesCities;
            if (companiesCities != null)
            {
                //TODO
                this.ResumeByCompanyParseSelectCities.Text = String.Join(",", this.CompanyItems[this.CompaniesListbox.SelectedIndex].Places.Select(t => t?.Name));
            }
            else
            {
                this.ResumeByCompanyParseSelectCities.Text = "";
            }
        }

        private void YearsLimit_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CompaniesListbox.SelectedIndex == -1)
            {
                return;
            }
            this.CompanyItems[this.CompaniesListbox.SelectedIndex].RestrictByDate = this.YearsLimit.Checked;
        }

        private void YearsNumeric_ValueChanged(object sender, EventArgs e)
        {
            if (this.CompaniesListbox.SelectedIndex == -1)
            {
                return;
            }
            this.CompanyItems[this.CompaniesListbox.SelectedIndex].RestrictYears = (int)this.YearsNumeric.Value;
        }

        private void MonthesNumeric_ValueChanged(object sender, EventArgs e)
        {
            if (this.CompaniesListbox.SelectedIndex == -1)
            {
                return;
            }
            this.CompanyItems[this.CompaniesListbox.SelectedIndex].RestrictMonthes = (int)this.MonthesNumeric.Value;

        }

        private void GetMetroExtraInfo_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Функция \"Пагинация по станциям метро\"\r" +
                "Эта функия доступна для больших компаний,количество резюме которых в больших городах превышает 5000.\r" +
                "Если поиск идет по городу указанному в списке ниже и разрешении этой функции,будет идти дополнительный парсинг по станция метро.\r" +
                "Таким образом,можно спарсить гораздо больше резюме нежели 5000.\r" +
                "Список доступных городов: Москва,Санкт-Петербург,Минск,Казань,Нижний Новгород,Киев,Алматы,Новосибирск,Харьков,Самара,Днепр (Днепропетровск)", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ParseWithMetros_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CompaniesListbox.SelectedIndex == -1)
            {
                return;
            }
            this.CompanyItems[this.CompaniesListbox.SelectedIndex].ParseByMetro = this.ParseWithMetros.Checked;
        }

        private void MinMetroCount_ValueChanged(object sender, EventArgs e)
        {
            if (this.CompaniesListbox.SelectedIndex == -1)
            {
                return;
            }
            this.CompanyItems[this.CompaniesListbox.SelectedIndex].MinResumesCount = (int)this.MinMetroCount.Value;
        }

        private void companyfileinfo_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Функция \"Загрузка списка компаний из .xlsx файла\"\r" +
    "Функция позволяет загрузить множество компаний из таблицы Excel\r" +
    "Файл .xlsx должен содержать к колонки:\r" +
    "1.Название компании\r" +
    "2.Сайт компании\r" +
    "3.Парсить альтернативные названия по сайту(0/1)\r" +
    "4.Количество резюме для парсинга по сайту\r" +
    "Чтение начинается со второй строки,то есть,в первую строку вставляйте соответствующие заголовки в ячейке или оставляйте ее пустой и начинайте заполнять таблицу со второй строки...", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void LoadCompaniesList_Click(object sender, EventArgs e)
        {
            var LoadedItems = new List<CompanySearchItem>();
            try
            {
                var ofd = new OpenFileDialog
                {
                    Filter = "xlsx|*.xlsx"
                };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    var workbook = new XSSFWorkbook(new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                    var sheet = workbook.GetSheetAt(0);
                    var count = sheet.LastRowNum;
                    for (var i = 1; i <= count; i++)
                    {
                        var row = sheet.GetRow(i);
                        if (row != null)
                        {
                            var user = new CompanySearchItem
                            {
                                Name = row.GetCell(0)?.StringCellValue,
                                Website = row.GetCell(1)?.StringCellValue,
                                CheckOnWebsite = row.GetCell(3).CellType == NPOI.SS.UserModel.CellType.Boolean ?
                                (bool)row.GetCell(3)?.BooleanCellValue : (row.GetCell(3).CellType == NPOI.SS.UserModel.CellType.Numeric ?
                                Convert.ToBoolean(row.GetCell(3)?.NumericCellValue) : Convert.ToBoolean(row.GetCell(3)?.StringCellValue)),
                                CheckOnWebsiteCount = row.GetCell(3).CellType == NPOI.SS.UserModel.CellType.Numeric ?
                                (int)row.GetCell(3)?.NumericCellValue : Convert.ToInt32(row.GetCell(3)?.StringCellValue),
                                AlternativeNames = new List<string>(),
                                Places = new List<Place>(),
                                CompanyResumes = new List<Resume>()
                            };
                            LoadedItems.Add(user);
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Загружен неверный файл", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            foreach (var item in LoadedItems)
            {
                this.CompanyItems.Add(item);
                this.CompaniesListbox.Items.Add(item.Name);
            }
        }
    }
}
