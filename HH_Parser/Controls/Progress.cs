using System;
using System.Drawing;
using System.Windows.Forms;

namespace HH_Parser
{
    public partial class Progress : UserControl
    {
        public Progress()
        {
            InitializeComponent();
            this.label1.Text = "0\\100";
        }
        public int Maximum
        {
            get
            {
                return this.progressBar1.Maximum;
            }
            set
            {
                this.progressBar1.Maximum = value;
            }
        }
        public int Value
        {
            get
            {
                return this.progressBar1.Value;
            }
            set
            {
                this.progressBar1.Value = value;
                this.label1.Text = string.Format("{0}\\{1}", value, this.progressBar1.Maximum);
                if (this.progressBar1.Width + 2 + this.label1.Width > this.Width)
                {
                    this.Width = this.progressBar1.Width + 2 + this.label1.Width;
                }
            }
        }
    }
}
