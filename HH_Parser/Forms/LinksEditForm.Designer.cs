namespace HH_Parser
{
    partial class LinksEditForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.companyfileinfo = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.Links = new System.Windows.Forms.RichTextBox();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Controls.Add(this.companyfileinfo);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(698, 374);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Ссылки на страницы поиска резюме";
            // 
            // companyfileinfo
            // 
            this.companyfileinfo.FlatAppearance.BorderSize = 0;
            this.companyfileinfo.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Window;
            this.companyfileinfo.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Window;
            this.companyfileinfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.companyfileinfo.Image = global::HH_Parser.Properties.Resources.info;
            this.companyfileinfo.Location = new System.Drawing.Point(214, -4);
            this.companyfileinfo.Name = "companyfileinfo";
            this.companyfileinfo.Size = new System.Drawing.Size(23, 25);
            this.companyfileinfo.TabIndex = 43;
            this.companyfileinfo.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.companyfileinfo.UseVisualStyleBackColor = true;
            this.companyfileinfo.Click += new System.EventHandler(this.companyfileinfo_Click);
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.Links);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 19);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(692, 352);
            this.panel1.TabIndex = 44;
            // 
            // Links
            // 
            this.Links.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Links.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Links.Location = new System.Drawing.Point(0, 0);
            this.Links.Name = "Links";
            this.Links.Size = new System.Drawing.Size(690, 350);
            this.Links.TabIndex = 0;
            this.Links.Text = "";
            this.Links.WordWrap = false;
            // 
            // LinksEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(698, 374);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LinksEditForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Редактирование ссылок";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LinksEditForm_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button companyfileinfo;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RichTextBox Links;
    }
}