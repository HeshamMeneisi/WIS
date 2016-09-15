using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;

namespace Simulator
{
    class FileManager
    {
        public static void WriteToFile(EventSet settowrite,string filename)
        {
            if (Path.GetExtension(filename) == ".set")
            {
                Stream fileStr = File.Create(filename);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fileStr, settowrite);
                fileStr.Close();
            }
            else if (Path.GetExtension(filename) == ".wis")
            {
                StreamWriter sw = new StreamWriter(filename);
                sw.Write(CodeManager.GenerateCode(settowrite));
                sw.Close();
            }
        }
        public static void ReadFile(ref EventSet settoreadto, string filename)
        {
            settoreadto = new EventSet();
            if (File.Exists(filename))
            {
                try
                {
                    if (Path.GetExtension(filename) == ".set")
                    {
                        Stream fileStr = File.Open(filename, FileMode.Open);
                        BinaryFormatter bf = new BinaryFormatter();
                        settoreadto = (EventSet)bf.Deserialize(fileStr);
                        fileStr.Close();
                    }
                    else if (Path.GetExtension(filename) == ".wis")
                    {
                        StreamReader sr = new StreamReader(filename);
                        settoreadto = CodeManager.Interpret(sr.ReadToEnd());
                        sr.Close();
                    }
                }
                catch
                {
                    MessageBox.Show(Windows_Input_Simulator.Program.mainForm, filename + " -Is Corrupted Or Cannot Be Accessed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show(Windows_Input_Simulator.Program.mainForm, filename + " -Doesn't Exist!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
