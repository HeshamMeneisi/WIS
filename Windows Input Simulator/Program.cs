using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Windows_Input_Simulator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static mainFrm mainForm;
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
        run:
            try
            {
                Application.Run(mainForm = new mainFrm(args));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                goto run;
            }
        }
    }
}
