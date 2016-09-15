using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Simulator
{
    class KeyManager
    {
        public static string GetKeyName(Keys key)
        {
            return key.ToString();
        }
        public static Keys GetKey(string displayName)
        {
            return (Keys)Enum.Parse(typeof(Keys), displayName);
        }
        public static string[] GetAllKeyNames()
        {
            return Enum.GetNames(typeof(Keys));
        }
    }
}
