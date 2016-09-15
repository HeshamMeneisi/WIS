using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Windows_Input_Simulator
{
    public partial class Log : Form
    {
        public Log()
        {
            InitializeComponent();
        }
        public Point relativePos;
        private void Log_Load(object sender, EventArgs e)
        {
            lastSize = this.Size;
            this.Location = new Point(this.Owner.Location.X + relativePos.X, this.Owner.Location.Y + relativePos.Y);
            this.Owner.LocationChanged += lch;
        }

        private void lch(object sender, EventArgs e)
        {
            this.Location = new Point(this.Owner.Location.X + relativePos.X, this.Owner.Location.Y + relativePos.Y);
            Application.DoEvents();
        }

        public void AddLog(DateTime time, string message)
        {
            ListViewItem item = new ListViewItem();
            item.Text = time.ToShortTimeString();
            ListViewItem.ListViewSubItem sub1 = new ListViewItem.ListViewSubItem();
            sub1.Text = message;
            item.SubItems.Add(sub1);
            this.Invoke((MethodInvoker)delegate { listView1.Items.Add(item); });
            this.Invoke((MethodInvoker)delegate { listView1.EnsureVisible(listView1.Items.Count - 1); });
        }

        private void Log_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Shrink();
            }
        }
        private void sBtn_Click(object sender, EventArgs e)
        {
            Expand();
        }
        Size lastSize;
        private void Shrink()
        {
            lastSize = this.Size;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.listView1.Visible = false;
            this.Size = sBtn.Size;
            this.sBtn.Visible = true;
        }
        private void Expand()
        {
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.listView1.Visible = true;
            this.Size = lastSize;
            this.sBtn.Visible = false;
        }
        bool init = false;
        private void Log_LocationChanged(object sender, EventArgs e)
        {
            if (init)
                relativePos = new Point(this.Location.X - this.Owner.Location.X, this.Location.Y - this.Owner.Location.Y);
            else
                init = true;
        }
    }
}
