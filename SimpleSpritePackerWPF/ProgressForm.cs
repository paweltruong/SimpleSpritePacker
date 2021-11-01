using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SimpleSpritePacker
{
    public partial class ProgressForm : Form
    {
        public event EventHandler Canceled;
        public ProgressForm()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
            Canceled?.Invoke(this,new EventArgs());
        }

        public void Init(int fileCount)
        {
            lbProgress.Text = string.Empty;
            pbProgress.Value = 0;
            pbProgress.Maximum = fileCount;
        }

        internal void UpdateProgress(int progressPercentage, object userState)
        {
            lbProgress.Text = userState.ToString();
            pbProgress.Value = progressPercentage;
        }

        private void ProgressForm_Load(object sender, EventArgs e)
        {
            this.CenterToParent();
        }
    }
}
