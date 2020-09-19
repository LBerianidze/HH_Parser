using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HH_Parser
{
    public partial class CompaniesWarningDialog : Form
    {
        public CompaniesWarningDialog()
        {
            InitializeComponent();
        }
        int Result = -1;
        int MaxCount;
        public int ShowDialog(IWin32Window owner,int Count)
        {
            this.label2.Text = Count.ToString();
            this.MaxCount = Count;
            this.ParseCount.Maximum = this.MaxCount;
            this.ShowDialog(owner);
            return this.Result;
        }

        private void ParseAll_Click(object sender, EventArgs e)
        {
            this.Result = -1;
            this.Hide();
        }

        private void ParseSet_Click(object sender, EventArgs e)
        {
            int ParseCount = Convert.ToInt32(this.ParseCount.Value);
            this.Result = ParseCount;
            this.Hide();
        }

        private void ParseCount_ValueChanged(object sender, EventArgs e)
        {
            int parsecount = Convert.ToInt32(this.ParseCount.Value);
            if (parsecount > this.MaxCount)
                this.ParseCount.Value = this.MaxCount;
            if (parsecount < 1)
                parsecount = 1;
        }

        private void CancelParsing_Click(object sender, EventArgs e)
        {
            this.Result = -2;
            this.Hide();

        }
    }
}
