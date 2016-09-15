using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace GlobalHook
{
    public class MSLLHook : Hook
    {
        private const short HC_ACTION = 0;

        private const short WM_LBUTTONDOWN = 0x0201;
        private const short WM_LBUTTONUP = 0x0202;
        private const short WM_MOUSEMOVE = 0x0200;
        private const short WM_MOUSEWHEEL = 0x020A;
        private const short WM_MOUSEHWHEEL = 0x020E;
        private const short WM_RBUTTONDOWN = 0x0204;
        private const short WM_RBUTTONUP = 0x0205;

        public delegate void MSButtonEventHandler(Hook sender, MSButtonEventArgs args);
        public delegate void MSWheelEventHandler(Hook sender, MSWheelEventArgs args);
        public delegate void MSMoveEventHandler(Hook sender, MSMovedEventArgs args);

        public event MSButtonEventHandler ButtonDown;
        public event MSButtonEventHandler ButtonUp;
        public event MSWheelEventHandler Wheel;
        public event MSMoveEventHandler Moved;

        public MSLLHook(uint thread = 0) : base(HookType.WH_MOUSE_LL, thread)
        { }

        struct Info
        {
            public int X, Y; // POINT
            public short RESERVED, DELTA;
        }

        protected override bool CallBackProcedure(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode == HC_ACTION)
            {
                Info i = (Info)Marshal.PtrToStructure(lParam, typeof(Info));
                MSEvent e = null;
                switch ((short)wParam)
                {
                    case WM_LBUTTONDOWN:
                        ButtonDown?.Invoke(this, (MSButtonEventArgs)(e = new MSButtonEventArgs(MouseButtons.Left, i.X, i.Y)));
                        break;
                    case WM_LBUTTONUP:
                        ButtonUp?.Invoke(this, (MSButtonEventArgs)(e = new MSButtonEventArgs(MouseButtons.Left, i.X, i.Y)));
                        break;
                    case WM_MOUSEMOVE:
                        Moved?.Invoke(this, (MSMovedEventArgs)(e = new MSMovedEventArgs(i.X, i.Y)));
                        break;
                    case WM_MOUSEWHEEL:
                        Wheel?.Invoke(this, (MSWheelEventArgs)(e = new MSWheelEventArgs(i.X, i.Y, i.DELTA > 0 ? 120 : -120, false)));
                        break;
                    case WM_MOUSEHWHEEL:
                        Wheel?.Invoke(this, (MSWheelEventArgs)(e = new MSWheelEventArgs(i.X, i.Y, i.DELTA > 0 ? 120 : -120, true)));
                        break;
                    case WM_RBUTTONDOWN:
                        ButtonDown?.Invoke(this, (MSButtonEventArgs)(e = new MSButtonEventArgs(MouseButtons.Right, i.X, i.Y)));
                        break;
                    case WM_RBUTTONUP:
                        ButtonDown?.Invoke(this, (MSButtonEventArgs)(e = new MSButtonEventArgs(MouseButtons.Right, i.X, i.Y)));
                        break;
                    default: Console.WriteLine("MSLLHook Received an Unrecognized Message: " + wParam); return false;
                }
                return e != null ? e.Suppress : false;
            }
            return false;
        }
    }
}

public class MSEvent
{
    public bool Suppress { get; set; }
}
public class MSMovedEventArgs : MSEvent
{
    private int x;
    private int y;

    public int X
    {
        get
        {
            return x;
        }
    }

    public int Y
    {
        get
        {
            return y;
        }
    }

    public Point Location { get { return new Point(x, y); } }

    public MSMovedEventArgs(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

public class MSWheelEventArgs : MSEvent
{
    private int delta;
    private int x;
    private int y;
    private bool horizontal;

    public int X
    {
        get
        {
            return x;
        }
    }

    public int Y
    {
        get
        {
            return y;
        }
    }

    public Point Location { get { return new Point(x, y); } }

    public int Delta
    {
        get
        {
            return delta;
        }
    }

    public bool Horizontal
    {
        get
        {
            return horizontal;
        }
    }

    public MSWheelEventArgs(int x, int y, int delta, bool horizontal)
    {
        this.x = x;
        this.y = y;
        this.delta = delta;
        this.horizontal = horizontal;
    }
}

public class MSButtonEventArgs : MSEvent
{
    private MouseButtons button;
    private int x;
    private int y;

    public MouseButtons Button
    {
        get
        {
            return button;
        }
    }

    public int X
    {
        get
        {
            return x;
        }
    }

    public int Y
    {
        get
        {
            return y;
        }
    }

    public Point Location
    {
        get { return new Point(x, y); }
    }

    public MSButtonEventArgs(MouseButtons key, int x, int y)
    {
        this.button = key;
        this.x = x;
        this.y = y;
    }
}
