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
    public partial class pmHelp : Form
    {
        public pmHelp()
        {
            InitializeComponent();
        }

        private void keyParameters_Load(object sender, EventArgs e)
        {
            Array avals = Keys.GetValues(typeof(Keys));
            Keys[] fvals = new Keys[avals.Length - 7];
            Array.Copy(avals, 7, fvals, 0, fvals.Length);
            foreach(Keys key in fvals)
            {
                ListViewItem item = new ListViewItem();
                item.Text = key.ToString();
                ListViewItem.ListViewSubItem sub = new ListViewItem.ListViewSubItem();
                sub.Text = key.GetHashCode().ToString();
                item.SubItems.Add(sub);
                listView1.Items.Add(item);
            }
        }
    }
}
