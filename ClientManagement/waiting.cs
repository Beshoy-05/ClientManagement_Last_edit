using System;
using System.Windows.Forms;

namespace ClientManagement
{
    public partial class waiting : Form
    {
        private Timer progressTimer;

        public waiting()
        {
            InitializeComponent();
        }

        private void waiting_Load(object sender, EventArgs e)
        {
            guna2CircleProgressBar1.Minimum = 0;
            guna2CircleProgressBar1.Maximum = 100;
            guna2CircleProgressBar1.Value = 0;

            progressTimer = new Timer();
            progressTimer.Interval = 50;
            progressTimer.Tick += ProgressTimer_Tick;
            progressTimer.Start();
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (guna2CircleProgressBar1.Value < guna2CircleProgressBar1.Maximum)
            {
                guna2CircleProgressBar1.Value += 2; 
            }
            else
            {
                progressTimer.Stop();
                login login = new login();
                login.Show();
                this.Hide(); 
        

            }
        }
    }
}
