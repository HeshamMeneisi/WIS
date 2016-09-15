using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Windows.Forms;
using BrendanGrant.Helpers.FileAssociation;
using System.IO;

namespace Windows_Input_Simulator
{
    class SettingsManager
    {
        public static void SaveSettings(bool intset,bool intwas,int maxexeact,bool executeonload, int dcdmode, bool topm)
        {
            Properties.Settings.Default.intset = intset;
            Properties.Settings.Default.intwis = intwas;
            Properties.Settings.Default.evespersecond = maxexeact;
            Properties.Settings.Default.executeonload = executeonload;
            Properties.Settings.Default.dcdm = dcdmode;
            Properties.Settings.Default.topmost = topm;
            Properties.Settings.Default.Save();
        }
        public static void ReLoadSettings()
        {
            Properties.Settings.Default.Reload();
        }
        public static void ApplySettings()
        {
            FileAssociationInfo fai;
            string s = Application.ExecutablePath;
            if (Properties.Settings.Default.intset)
            {
                fai = new FileAssociationInfo(".set");
                fai.Create(Application.ProductName);
                List<string> list = new List<string> { Application.ExecutablePath };
                list.AddRange(fai.OpenWithList);
                fai.OpenWithList = list.ToArray();
                ProgramAssociationInfo pai = new ProgramAssociationInfo(fai.ProgID);
                pai.Create("Events Set", new ProgramVerb("open", Application.ExecutablePath + " %1"));
                pai.DefaultIcon = new ProgramIcon(s);
            }
            else
            {
                fai = new FileAssociationInfo(".set");
                if (fai.Exists)
                {
                    fai.Delete();
                }
                ProgramAssociationInfo pai = new ProgramAssociationInfo(fai.ProgID);
            }
            if (Properties.Settings.Default.intwis)
            {
                fai = new FileAssociationInfo(".wis");
                fai.Create(Application.ProductName);
                List<string> list = new List<string> { Application.ExecutablePath };
                list.AddRange(fai.OpenWithList);
                fai.OpenWithList = list.ToArray();
                ProgramAssociationInfo pai = new ProgramAssociationInfo(fai.ProgID);
                pai.Create("Windows Input Script", new ProgramVerb("open", Application.ExecutablePath + " %1"));
                pai.DefaultIcon = new ProgramIcon(s);
            }
            else
            {
                fai = new FileAssociationInfo(".wis");
                if (fai.Exists && fai.ProgID == Application.ProductName)
                    fai.Delete(); 
            }
        }
    }
}
