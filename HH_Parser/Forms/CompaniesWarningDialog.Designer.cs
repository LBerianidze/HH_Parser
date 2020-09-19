namespace HH_Parser
{
    partial class CompaniesWarningDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.ParseAll = new System.Windows.Forms.Button();
            this.ParseCount = new System.Windows.Forms.NumericUpDown();
            this.ParseSet = new System.Windows.Forms.Button();
            this.CancelParsing = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.ParseCount)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.label1.Location = new System.Drawing.Point(2, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(223, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Количество найденых вакансий:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.label2.Location = new System.Drawing.Point(221, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(16, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.label3.Location = new System.Drawing.Point(2, 29);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(275, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "Выберите действие что бы продолжить:";
            // 
            // ParseAll
            // 
            this.ParseAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            this.ParseAll.Location = new System.Drawing.Point(3, 48);
            this.ParseAll.Name = "ParseAll";
            this.ParseAll.Size = new System.Drawing.Size(173, 23);
            this.ParseAll.TabIndex = 3;
            this.ParseAll.Text = "Спарсить все вакансии";
            this.ParseAll.UseVisualStyleBackColor = true;
            this.ParseAll.Click += new System.EventHandler(this.ParseAll_Click);
            // 
            // ParseCount
            // 
            this.ParseCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            this.ParseCount.Location = new System.Drawing.Point(178, 78);
            this.ParseCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ParseCount.Name = "ParseCount";
            this.ParseCount.Size = new System.Drawing.Size(120, 21);
            this.ParseCount.TabIndex = 4;
            this.ParseCount.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.ParseCount.ValueChanged += new System.EventHandler(this.ParseCount_ValueChanged);
            // 
            // ParseSet
            // 
            this.ParseSet.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            this.ParseSet.Location = new System.Drawing.Point(3, 77);
            this.ParseSet.Name = "ParseSet";
            this.ParseSet.Size = new System.Drawing.Size(173, 23);
            this.ParseSet.TabIndex = 5;
            this.ParseSet.Text = "Спарсить первые:";
            this.ParseSet.UseVisualStyleBackColor = true;
            this.ParseSet.Click += new System.EventHandler(this.ParseSet_Click);
            // 
            // CancelParsing
            // 
            this.CancelParsing.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            this.CancelParsing.ForeColor = System.Drawing.Color.Black;
            this.CancelParsing.Location = new System.Drawing.Point(178, 48);
            this.CancelParsing.Name = "CancelParsing";
            this.CancelParsing.Size = new System.Drawing.Size(173, 23);
            this.CancelParsing.TabIndex = 6;
            this.CancelParsing.Text = "Отменить парсинг";
            this.CancelParsing.UseVisualStyleBackColor = true;
            this.CancelParsing.Click += new System.EventHandler(this.CancelParsing_Click);
            // 
            // CompaniesWarningDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(361, 105);
            this.ControlBox = false;
            this.Controls.Add(this.CancelParsing);
            this.Controls.Add(this.ParseSet);
            this.Controls.Add(this.ParseCount);
            this.Controls.Add(this.ParseAll);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "CompaniesWarningDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Предупреждение";
            ((System.ComponentModel.ISupportInitialize)(this.ParseCount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button ParseAll;
        private System.Windows.Forms.NumericUpDown ParseCount;
        private System.Windows.Forms.Button ParseSet;
        private System.Windows.Forms.Button CancelParsing;
    }
}