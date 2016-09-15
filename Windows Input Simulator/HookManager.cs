using System;
using System.Windows.Forms;
using GlobalHook;
namespace Simulator
{
    public static class HookManager
    {
        static KBLLHook khook;
        static MSLLHook mhook;

        public static Action<KBKeyEventArgs> KeyDown;
        public static Action<KBKeyEventArgs> KeyUp;
        public static Action<MSButtonEventArgs> MouseDown;
        public static Action<MSButtonEventArgs> MouseUp;
        public static Action<MSWheelEventArgs> MouseWheel;
        public static Action<MSMovedEventArgs> MouseMove;

        public static void Init()
        {
            khook = new KBLLHook();
            mhook = new MSLLHook();

            khook.KeyDown += (s, e) => KeyDown?.Invoke(e);
            khook.KeyUp += (s, e) => KeyUp?.Invoke(e);

            mhook.ButtonDown += (s, e) => MouseDown?.Invoke(e);
            mhook.ButtonUp += (s, e) => MouseUp?.Invoke(e);
            mhook.Moved += (s, e) => MouseMove?.Invoke(e);
            mhook.Wheel += (s, e) => MouseWheel?.Invoke(e);
        }

        public static int GetDoubleClickTime()
        {
            return SystemInformation.DoubleClickTime;
        }
    }
}