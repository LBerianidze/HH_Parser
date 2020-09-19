namespace HH_Parser
{
    partial class SelectCitiesWindow
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
            this.CitiesTreeView = new System.Windows.Forms.TreeView();
            this.CitySearchTB = new System.Windows.Forms.TextBox();
            this.AcceptCities = new System.Windows.Forms.Button();
            this.CancelCities = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CitiesTreeView
            // 
            this.CitiesTreeView.CheckBoxes = true;
            this.CitiesTreeView.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.CitiesTreeView.FullRowSelect = true;
            this.CitiesTreeView.Indent = 19;
            this.CitiesTreeView.ItemHeight = 19;
            this.CitiesTreeView.Location = new System.Drawing.Point(0, 23);
            this.CitiesTreeView.Name = "CitiesTreeView";
            this.CitiesTreeView.Size = new System.Drawing.Size(444, 432);
            this.CitiesTreeView.TabIndex = 0;
            this.CitiesTreeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.CitiesTreeView_AfterCheck);
            // 
            // CitySearchTB
            // 
            this.CitySearchTB.Dock = System.Windows.Forms.DockStyle.Top;
            this.CitySearchTB.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            this.CitySearchTB.Location = new System.Drawing.Point(0, 0);
            this.CitySearchTB.Name = "CitySearchTB";
            this.CitySearchTB.Size = new System.Drawing.Size(444, 21);
            this.CitySearchTB.TabIndex = 1;
            this.CitySearchTB.TextChanged += new System.EventHandler(this.CitySearchTB_TextChanged);
            // 
            // AcceptCities
            // 
            this.AcceptCities.Font = new System.Drawing.Font("Microsoft Sans Serif", 18.25F);
            this.AcceptCities.Location = new System.Drawing.Point(312, 457);
            this.AcceptCities.Name = "AcceptCities";
            this.AcceptCities.Size = new System.Drawing.Size(131, 39);
            this.AcceptCities.TabIndex = 2;
            this.AcceptCities.Text = "Выбрать";
            this.AcceptCities.UseVisualStyleBackColor = true;
            this.AcceptCities.Click += new System.EventHandler(this.AcceptCities_Click);
            // 
            // CancelCities
            // 
            this.CancelCities.Font = new System.Drawing.Font("Microsoft Sans Serif", 18.25F);
            this.CancelCities.Location = new System.Drawing.Point(175, 457);
            this.CancelCities.Name = "CancelCities";
            this.CancelCities.Size = new System.Drawing.Size(135, 39);
            this.CancelCities.TabIndex = 3;
            this.CancelCities.Text = "Отменить";
            this.CancelCities.UseVisualStyleBackColor = true;
            this.CancelCities.Click += new System.EventHandler(this.CancelCities_Click);
            // 
            // SelectCitiesWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(444, 497);
            this.ControlBox = false;
            this.Controls.Add(this.CancelCities);
            this.Controls.Add(this.AcceptCities);
            this.Controls.Add(this.CitySearchTB);
            this.Controls.Add(this.CitiesTreeView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectCitiesWindow";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Выбор города";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView CitiesTreeView;
        private System.Windows.Forms.TextBox CitySearchTB;
        private System.Windows.Forms.Button AcceptCities;
        private System.Windows.Forms.Button CancelCities;
    }
}