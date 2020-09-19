namespace HH_Parser
{
    partial class CompaniesEditForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.CompaniesListbox = new System.Windows.Forms.ListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.LoadCompaniesList = new System.Windows.Forms.Button();
            this.AddCompany = new System.Windows.Forms.Button();
            this.DeleteCompany = new System.Windows.Forms.Button();
            this.CompanyName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.CompanyWebsite = new System.Windows.Forms.TextBox();
            this.CheckByWebsite = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.CheckByWebsiteCount = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.AlternativeNames = new System.Windows.Forms.RichTextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.MinMetroCount = new System.Windows.Forms.NumericUpDown();
            this.ResumeByCompanyParseSelectCities = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.MonthesNumeric = new System.Windows.Forms.NumericUpDown();
            this.YearsNumeric = new System.Windows.Forms.NumericUpDown();
            this.YearsLimit = new System.Windows.Forms.CheckBox();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.ParseWithMetros = new System.Windows.Forms.CheckBox();
            this.GetMetroExtraInfo = new System.Windows.Forms.Button();
            this.companyfileinfo = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MinMetroCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MonthesNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.YearsNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // CompaniesListbox
            // 
            this.CompaniesListbox.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.CompaniesListbox.FormattingEnabled = true;
            this.CompaniesListbox.ItemHeight = 17;
            this.CompaniesListbox.Location = new System.Drawing.Point(3, 45);
            this.CompaniesListbox.Name = "CompaniesListbox";
            this.CompaniesListbox.Size = new System.Drawing.Size(209, 327);
            this.CompaniesListbox.TabIndex = 0;
            this.CompaniesListbox.SelectedIndexChanged += new System.EventHandler(this.CompaniesListbox_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.LoadCompaniesList);
            this.groupBox1.Controls.Add(this.CompaniesListbox);
            this.groupBox1.Controls.Add(this.companyfileinfo);
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.groupBox1.Location = new System.Drawing.Point(1, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(215, 388);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Компании";
            // 
            // LoadCompaniesList
            // 
            this.LoadCompaniesList.AutoEllipsis = true;
            this.LoadCompaniesList.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.LoadCompaniesList.Location = new System.Drawing.Point(2, 15);
            this.LoadCompaniesList.Name = "LoadCompaniesList";
            this.LoadCompaniesList.Size = new System.Drawing.Size(189, 29);
            this.LoadCompaniesList.TabIndex = 3;
            this.LoadCompaniesList.Text = "Загрузить список компаний";
            this.LoadCompaniesList.UseVisualStyleBackColor = true;
            this.LoadCompaniesList.Click += new System.EventHandler(this.LoadCompaniesList_Click);
            // 
            // AddCompany
            // 
            this.AddCompany.AutoEllipsis = true;
            this.AddCompany.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.AddCompany.Location = new System.Drawing.Point(2, 393);
            this.AddCompany.Name = "AddCompany";
            this.AddCompany.Size = new System.Drawing.Size(212, 29);
            this.AddCompany.TabIndex = 2;
            this.AddCompany.Text = "Добавить компанию";
            this.AddCompany.UseVisualStyleBackColor = true;
            this.AddCompany.Click += new System.EventHandler(this.AddCompany_Click);
            // 
            // DeleteCompany
            // 
            this.DeleteCompany.AutoEllipsis = true;
            this.DeleteCompany.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.DeleteCompany.Location = new System.Drawing.Point(1, 424);
            this.DeleteCompany.Name = "DeleteCompany";
            this.DeleteCompany.Size = new System.Drawing.Size(212, 29);
            this.DeleteCompany.TabIndex = 3;
            this.DeleteCompany.Text = "Удалить компанию";
            this.DeleteCompany.UseVisualStyleBackColor = true;
            this.DeleteCompany.Click += new System.EventHandler(this.DeleteCompany_Click);
            // 
            // CompanyName
            // 
            this.CompanyName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.CompanyName.Location = new System.Drawing.Point(211, 14);
            this.CompanyName.Name = "CompanyName";
            this.CompanyName.Size = new System.Drawing.Size(218, 23);
            this.CompanyName.TabIndex = 4;
            this.CompanyName.TextChanged += new System.EventHandler(this.CompanyName_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(6, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(204, 17);
            this.label1.TabIndex = 5;
            this.label1.Text = "Название компании для поиска :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(6, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 17);
            this.label2.TabIndex = 7;
            this.label2.Text = "Сайт компании:";
            // 
            // CompanyWebsite
            // 
            this.CompanyWebsite.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.CompanyWebsite.Location = new System.Drawing.Point(211, 43);
            this.CompanyWebsite.Name = "CompanyWebsite";
            this.CompanyWebsite.Size = new System.Drawing.Size(218, 23);
            this.CompanyWebsite.TabIndex = 6;
            this.CompanyWebsite.TextChanged += new System.EventHandler(this.CompanyWebsite_TextChanged);
            // 
            // CheckByWebsite
            // 
            this.CheckByWebsite.AutoSize = true;
            this.CheckByWebsite.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.CheckByWebsite.Location = new System.Drawing.Point(211, 72);
            this.CheckByWebsite.Name = "CheckByWebsite";
            this.CheckByWebsite.Size = new System.Drawing.Size(155, 19);
            this.CheckByWebsite.TabIndex = 8;
            this.CheckByWebsite.Text = "Искать сперва по сайту";
            this.CheckByWebsite.UseVisualStyleBackColor = true;
            this.CheckByWebsite.CheckedChanged += new System.EventHandler(this.CheckByWebsite_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(207, 97);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(110, 17);
            this.label3.TabIndex = 10;
            this.label3.Text = "спарсить первые";
            // 
            // CheckByWebsiteCount
            // 
            this.CheckByWebsiteCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.CheckByWebsiteCount.Location = new System.Drawing.Point(319, 96);
            this.CheckByWebsiteCount.Name = "CheckByWebsiteCount";
            this.CheckByWebsiteCount.Size = new System.Drawing.Size(61, 23);
            this.CheckByWebsiteCount.TabIndex = 9;
            this.CheckByWebsiteCount.Text = "0";
            this.CheckByWebsiteCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.CheckByWebsiteCount.TextChanged += new System.EventHandler(this.CheckByWebsiteCount_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoEllipsis = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.Location = new System.Drawing.Point(382, 97);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(243, 38);
            this.label4.TabIndex = 11;
            this.label4.Text = "резюме для поиска альтернативных имен по сайту(0 для всех)";
            this.label4.UseCompatibleTextRendering = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.AlternativeNames);
            this.groupBox2.Location = new System.Drawing.Point(11, 214);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(418, 228);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Альтернативные имена компании";
            // 
            // AlternativeNames
            // 
            this.AlternativeNames.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.AlternativeNames.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AlternativeNames.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.AlternativeNames.Location = new System.Drawing.Point(3, 19);
            this.AlternativeNames.Name = "AlternativeNames";
            this.AlternativeNames.Size = new System.Drawing.Size(412, 206);
            this.AlternativeNames.TabIndex = 0;
            this.AlternativeNames.Text = "";
            this.AlternativeNames.Leave += new System.EventHandler(this.AlternativeNames_Leave);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe Print", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label5.ForeColor = System.Drawing.Color.Red;
            this.label5.Location = new System.Drawing.Point(299, 116);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(88, 19);
            this.label5.TabIndex = 13;
            this.label5.Text = "Ошибка ввода";
            this.label5.Visible = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.MinMetroCount);
            this.groupBox3.Controls.Add(this.ResumeByCompanyParseSelectCities);
            this.groupBox3.Controls.Add(this.label14);
            this.groupBox3.Controls.Add(this.MonthesNumeric);
            this.groupBox3.Controls.Add(this.YearsNumeric);
            this.groupBox3.Controls.Add(this.YearsLimit);
            this.groupBox3.Controls.Add(this.label15);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.groupBox2);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.CompanyName);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.CheckByWebsiteCount);
            this.groupBox3.Controls.Add(this.CompanyWebsite);
            this.groupBox3.Controls.Add(this.CheckByWebsite);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.label16);
            this.groupBox3.Controls.Add(this.ParseWithMetros);
            this.groupBox3.Controls.Add(this.GetMetroExtraInfo);
            this.groupBox3.Enabled = false;
            this.groupBox3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.groupBox3.Location = new System.Drawing.Point(219, 4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(631, 449);
            this.groupBox3.TabIndex = 14;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Настройки компании";
            // 
            // MinMetroCount
            // 
            this.MinMetroCount.Location = new System.Drawing.Point(571, 180);
            this.MinMetroCount.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.MinMetroCount.Minimum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.MinMetroCount.Name = "MinMetroCount";
            this.MinMetroCount.Size = new System.Drawing.Size(58, 23);
            this.MinMetroCount.TabIndex = 43;
            this.MinMetroCount.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.MinMetroCount.ValueChanged += new System.EventHandler(this.MinMetroCount_ValueChanged);
            // 
            // ResumeByCompanyParseSelectCities
            // 
            this.ResumeByCompanyParseSelectCities.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.ResumeByCompanyParseSelectCities.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            this.ResumeByCompanyParseSelectCities.Location = new System.Drawing.Point(210, 160);
            this.ResumeByCompanyParseSelectCities.Multiline = true;
            this.ResumeByCompanyParseSelectCities.Name = "ResumeByCompanyParseSelectCities";
            this.ResumeByCompanyParseSelectCities.ReadOnly = true;
            this.ResumeByCompanyParseSelectCities.Size = new System.Drawing.Size(219, 56);
            this.ResumeByCompanyParseSelectCities.TabIndex = 40;
            this.ResumeByCompanyParseSelectCities.DoubleClick += new System.EventHandler(this.ResumeByCompanyParseSelectCities_DoubleClick);
            // 
            // label14
            // 
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label14.Location = new System.Drawing.Point(6, 161);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(204, 55);
            this.label14.TabIndex = 39;
            this.label14.Text = "Города(Нажмите дважды для редактирования):";
            // 
            // MonthesNumeric
            // 
            this.MonthesNumeric.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MonthesNumeric.Location = new System.Drawing.Point(464, 136);
            this.MonthesNumeric.Maximum = new decimal(new int[] {
            11,
            0,
            0,
            0});
            this.MonthesNumeric.Name = "MonthesNumeric";
            this.MonthesNumeric.Size = new System.Drawing.Size(47, 22);
            this.MonthesNumeric.TabIndex = 37;
            this.MonthesNumeric.UpDownAlign = System.Windows.Forms.LeftRightAlignment.Left;
            this.MonthesNumeric.ValueChanged += new System.EventHandler(this.MonthesNumeric_ValueChanged);
            // 
            // YearsNumeric
            // 
            this.YearsNumeric.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.YearsNumeric.Location = new System.Drawing.Point(382, 136);
            this.YearsNumeric.Name = "YearsNumeric";
            this.YearsNumeric.Size = new System.Drawing.Size(53, 22);
            this.YearsNumeric.TabIndex = 35;
            this.YearsNumeric.UpDownAlign = System.Windows.Forms.LeftRightAlignment.Left;
            this.YearsNumeric.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.YearsNumeric.ValueChanged += new System.EventHandler(this.YearsNumeric_ValueChanged);
            // 
            // YearsLimit
            // 
            this.YearsLimit.AutoSize = true;
            this.YearsLimit.Checked = true;
            this.YearsLimit.CheckState = System.Windows.Forms.CheckState.Checked;
            this.YearsLimit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            this.YearsLimit.Location = new System.Drawing.Point(211, 138);
            this.YearsLimit.Name = "YearsLimit";
            this.YearsLimit.Size = new System.Drawing.Size(176, 20);
            this.YearsLimit.TabIndex = 34;
            this.YearsLimit.Text = "Ограничение по годам";
            this.YearsLimit.UseVisualStyleBackColor = true;
            this.YearsLimit.CheckedChanged += new System.EventHandler(this.YearsLimit_CheckedChanged);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label15.Location = new System.Drawing.Point(433, 137);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(31, 20);
            this.label15.TabIndex = 36;
            this.label15.Text = "лет";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label16.Location = new System.Drawing.Point(511, 136);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(70, 20);
            this.label16.TabIndex = 38;
            this.label16.Text = "месяц(а)";
            // 
            // ParseWithMetros
            // 
            this.ParseWithMetros.Font = new System.Drawing.Font("Segoe UI", 9.25F);
            this.ParseWithMetros.Location = new System.Drawing.Point(430, 159);
            this.ParseWithMetros.Name = "ParseWithMetros";
            this.ParseWithMetros.Size = new System.Drawing.Size(195, 47);
            this.ParseWithMetros.TabIndex = 41;
            this.ParseWithMetros.Text = "Пагинация по метро если количество больше";
            this.ParseWithMetros.UseVisualStyleBackColor = true;
            this.ParseWithMetros.CheckedChanged += new System.EventHandler(this.ParseWithMetros_CheckedChanged);
            // 
            // GetMetroExtraInfo
            // 
            this.GetMetroExtraInfo.FlatAppearance.BorderSize = 0;
            this.GetMetroExtraInfo.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Window;
            this.GetMetroExtraInfo.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Window;
            this.GetMetroExtraInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.GetMetroExtraInfo.Image = global::HH_Parser.Properties.Resources.info;
            this.GetMetroExtraInfo.Location = new System.Drawing.Point(511, 203);
            this.GetMetroExtraInfo.Name = "GetMetroExtraInfo";
            this.GetMetroExtraInfo.Size = new System.Drawing.Size(116, 25);
            this.GetMetroExtraInfo.TabIndex = 42;
            this.GetMetroExtraInfo.Text = "Информация";
            this.GetMetroExtraInfo.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.GetMetroExtraInfo.UseVisualStyleBackColor = true;
            this.GetMetroExtraInfo.Click += new System.EventHandler(this.GetMetroExtraInfo_Click);
            // 
            // companyfileinfo
            // 
            this.companyfileinfo.FlatAppearance.BorderSize = 0;
            this.companyfileinfo.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Window;
            this.companyfileinfo.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Window;
            this.companyfileinfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.companyfileinfo.Image = global::HH_Parser.Properties.Resources.info;
            this.companyfileinfo.Location = new System.Drawing.Point(190, 16);
            this.companyfileinfo.Name = "companyfileinfo";
            this.companyfileinfo.Size = new System.Drawing.Size(23, 25);
            this.companyfileinfo.TabIndex = 43;
            this.companyfileinfo.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.companyfileinfo.UseVisualStyleBackColor = true;
            this.companyfileinfo.Click += new System.EventHandler(this.companyfileinfo_Click);
            // 
            // CompaniesEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(854, 456);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.DeleteCompany);
            this.Controls.Add(this.AddCompany);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CompaniesEditForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Редактирование компаний";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MinMetroCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MonthesNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.YearsNumeric)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox CompaniesListbox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button AddCompany;
        private System.Windows.Forms.Button DeleteCompany;
        private System.Windows.Forms.TextBox CompanyName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox CompanyWebsite;
        private System.Windows.Forms.CheckBox CheckByWebsite;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox CheckByWebsiteCount;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RichTextBox AlternativeNames;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.NumericUpDown MonthesNumeric;
        private System.Windows.Forms.NumericUpDown YearsNumeric;
        private System.Windows.Forms.CheckBox YearsLimit;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox ResumeByCompanyParseSelectCities;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button GetMetroExtraInfo;
        private System.Windows.Forms.CheckBox ParseWithMetros;
        private System.Windows.Forms.NumericUpDown MinMetroCount;
        private System.Windows.Forms.Button LoadCompaniesList;
        private System.Windows.Forms.Button companyfileinfo;
    }
}