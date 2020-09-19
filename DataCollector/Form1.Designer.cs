namespace DataCollector
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.GeckoBrowser = new Gecko.GeckoWebBrowser();
            this.SuspendLayout();
            // 
            // GeckoBrowser
            // 
            this.GeckoBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GeckoBrowser.Location = new System.Drawing.Point(0, 0);
            this.GeckoBrowser.Name = "GeckoBrowser";
            this.GeckoBrowser.Size = new System.Drawing.Size(1004, 637);
            this.GeckoBrowser.TabIndex = 0;
            this.GeckoBrowser.UseHttpActivityObserver = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(1004, 637);
            this.Controls.Add(this.GeckoBrowser);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Data Collector";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Shown += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        public Gecko.GeckoWebBrowser GeckoBrowser;
    }
}

