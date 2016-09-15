using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GlobalHook
{
    /// <summary>
    /// A windows hook that uses the SetWindowsHookEx of user32.dll
    /// </summary>
    public abstract class Hook : IDisposable
    {
        public enum HookType
        {
            WH_CALLWNDPROC = 4,
            WH_CALLWNDPROCRET = 12,
            WH_CBT = 5,
            WH_DEBUG = 9,
            WH_FOREGROUNDIDLE = 11,
            WH_GETMESSAGE = 3,
            WH_JOURNALPLAYBACK = 1,
            WH_JOURNALRECORD = 0,
            WH_KEYBOARD = 2,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE = 7,
            WH_MOUSE_LL = 14,
            WH_MSGFILTER = -1,
            WH_SHELL = 10,
            WH_SYSMSGFILTER = 6
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr Procedure(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr hookID;
        private HookType hooktype;
        private uint tid;
        private Procedure proc; // Do not remove, this keeps the GC from collecting the delegate's memory

        /// <summary>
        /// The handle retured by SetWindowsHookEx.
        /// </summary>
        public IntPtr HookID
        {
            get
            {
                return hookID;
            }
        }

        /// <summary>
        // The type used to construct this hook.
        /// </summary>
        public HookType Hooktype
        {
            get
            {
                return hooktype;
            }
        }

        /// <summary>
        /// The ID of the target thread. 0 Means all system wide threads.
        /// </summary>
        public uint TID
        {
            get
            {
                return tid;
            }
        }

        /// <summary>
        /// Constructs a new hook and adds it to the chain.
        /// </summary>
        /// <param name="type">Hook type.</param>
        /// <param name="thread">Thread ID to attach to. 0 Means all system wide threads.</param>
        public Hook(HookType type, uint thread = 0)
        {
            hooktype = type;
            tid = thread;
            proc = CBProc;
            hookID = SetHook();
            if (hookID == IntPtr.Zero)
                throw new Exception("Failed to create hook.");
        }

        private IntPtr SetHook()
        {
            using (Process cproc = Process.GetCurrentProcess())            
                return SetWindowsHookEx((int)hooktype, proc, cproc.MainModule.BaseAddress, tid);
        }

        private IntPtr CBProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            bool handled = CallBackProcedure(nCode, wParam, lParam);
            return handled ? (IntPtr)0xFF : CallNextHookEx(hookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// Called when the hook captures a message. Refer to MSDN for parameter meaning.
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns>The returned value determines whether or not the message was handled. 
        /// If set to true, it will not be forwarded.</returns>
        protected abstract bool CallBackProcedure(int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Dispose and unhook using UnhookWindowsHookEx of user32.dll
        /// </summary>
        public void Dispose()
        {
            if (hookID != IntPtr.Zero && !UnhookWindowsHookEx(hookID))
                throw new Exception("Failed to unhook.");
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, Procedure lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
    }
}
