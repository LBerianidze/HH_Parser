using System;
using System.Net;
using System.Windows.Forms;

namespace HH_Parser
{
    public partial class CheckAuth : Form
    {
        public CheckAuth()
        {
            this.InitializeComponent();
        }

        private int result = -1;
        public new int ShowDialog(IWin32Window window)
        {
            base.ShowDialog(window);
            return this.result;
        }
        private void Auth_Click(object sender, EventArgs e)
        {
            //result = 1;
            //this.Hide();
            //return;
            var strresult = SendRequest("http://www.vh258879.eurodir.ru/Check.php?Login=" + this.textBox1.Text + "&Password=" + this.textBox2.Text);
            if (strresult == "Success")
            {
                this.result = 1;
                this.Hide();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль!");
            }
        }
        private static string SendRequest(string url)
        {
            try
            {
                using (var client = new WebClient())
                {
                    return client.DownloadString(new Uri(url));
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
