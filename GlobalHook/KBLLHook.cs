using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace GlobalHook
{
    public class KBLLHook : Hook
    {
        private const short HC_ACTION = 0;
        private const short WM_KEYDOWN = 0x100;
        private const short WM_KEYUP = 0x101;
        private const short WM_SYSKEYDOWN = 0x104;
        private const short WM_SYSKEYUP = 0x105;

        public delegate void KBKeyEventHandler(Hook sender, KBKeyEventArgs args);

        public event KBKeyEventHandler KeyDown;
        public event KBKeyEventHandler KeyUp;

        public KBLLHook(uint thread = 0) : base(HookType.WH_KEYBOARD_LL, thread)
        { }

        protected override bool CallBackProcedure(int nCode, IntPtr wParam, IntPtr lParam)
        {
            KBKeyEventArgs e = null;
            if (nCode == HC_ACTION)
            {
                if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
                {
                    e = new KBKeyEventArgs((Keys)Marshal.ReadInt32(lParam), wParam == (IntPtr)WM_SYSKEYDOWN);
                    KeyDown?.Invoke(this, e);
                }
                if (wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP)
                {
                    e = new KBKeyEventArgs((Keys)Marshal.ReadInt32(lParam), wParam == (IntPtr)WM_SYSKEYUP);
                    KeyUp?.Invoke(this, e);
                }
            }
            return e != null ? e.Suppress : false;
        }
    }

    public class KBKeyEventArgs
    {
        private Keys key;
        private bool issyskey;

        public Keys Key
        {
            get
            {
                return key;
            }
        }

        public bool IsSysKey
        {
            get
            {
                return issyskey;
            }
        }

        public bool Suppress { get; set; }

        public KBKeyEventArgs(Keys key, bool issyskey)
        {
            this.key = key;
            this.issyskey = issyskey;
        }
    }
}
