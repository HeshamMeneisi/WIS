using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using GlobalHook;
using System;

namespace Simulator
{
    class RecorderSim
    {
        // Enumes
        public enum DoubleClickDetectionMethod
        {
            Realtime = 1,
            PostRecord = 2,
            None = 0
        }
        public enum SimStatus
        {
            Running = 0,
            Paused = 1,
            Idle = 2
        }
        // Declarations
        EventSet eveset;
        Synchronouser sync;
        SimStatus status = SimStatus.Idle;
        DoubleClickDetectionMethod dcdm;
        bool realtimeDCD = false;
        bool recordkeyboard;
        bool recordmouseb;
        bool recordmousem;
        // Imports
        [DllImport("user32.dll")]
        public static extern short GetKeyState(int keyCode);
        // Key recorder
        private void KeyDown(KBKeyEventArgs e)
        {
            if (!HotkeyManager.Hotkeys.Contains(e.Key))
            {
                RecordEvent(EventType.KeyDown, (int)e.Key);
            }
        }

        private void RecordEvent(EventType etype, object info)
        {
            lock (eveset)
            {
                eveset.AddEvent(sync.Elapsed, etype, info);
                sync.Cycle();
            }
        }

        private void KeyUp(KBKeyEventArgs e)
        {
            if (!HotkeyManager.Hotkeys.Contains(e.Key))
            {
                RecordEvent(EventType.KeyUp, (int)e.Key);
            }
        }
        // Mouse recorder
        Event lastleftdown;
        Event lastleftup;
        bool cnlu;
        int dcdelay;
        int[] dcpercisionrange = new int[] { -2, -1, 0, 1, 2 };
        private void MouseDown(MSButtonEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Event eve = new Event(sync.Elapsed, EventType.LeftDown, e.Location);
                if (realtimeDCD && lastleftdown != null && eve.EventTime - lastleftdown.EventTime <= dcdelay && dcpercisionrange.Contains(((Point)lastleftdown.Info).X - e.X) && dcpercisionrange.Contains(((Point)lastleftdown.Info).Y - e.Y))
                {
                    eveset.RemoveEvent(lastleftdown);
                    eveset.RemoveEvent(lastleftup);
                    lastleftdown = null;
                    cnlu = true;
                    eve.EventType = EventType.DoubleClick;
                }
                else
                    lastleftdown = eve;
                eveset.AddEvent(eve);
            }
            else if (e.Button == MouseButtons.Right)
            {
                RecordEvent(EventType.RightDown, e.Location);
            }
            else if (e.Button == MouseButtons.Middle)
            {
                RecordEvent(EventType.MiddleDown, e.Location);
            }
            else if (e.Button == MouseButtons.XButton1)
            {
                RecordEvent(EventType.XB1Down, e.Location);
            }
            else if (e.Button == MouseButtons.XButton2)
            {
                RecordEvent(EventType.XB2Down, e.Location);
            }
        }
        private void MouseUp(MSButtonEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (!cnlu)
                    eveset.AddEvent(lastleftup = new Event(sync.Elapsed, EventType.LeftUp, e.Location));
                else
                    cnlu = false;
            }
            else if (e.Button == MouseButtons.Right)
            {
                RecordEvent(EventType.RightUp, e.Location);
            }
            else if (e.Button == MouseButtons.Middle)
            {
                RecordEvent(EventType.MiddleUp, e.Location);
            }
            else if (e.Button == MouseButtons.XButton1)
            {
                RecordEvent(EventType.XB1Up, e.Location);
            }
            else if (e.Button == MouseButtons.XButton2)
            {
                RecordEvent(EventType.XB2Up, e.Location);
            }
        }
        //
        //
        // ALPHA
        private void MouseMove(MSMovedEventArgs e)
        {
            RecordEvent(EventType.MouseMove, e.Location);
        }
        //
        //
        //
        private void Wheel(MSWheelEventArgs e)
        {
            RecordEvent(EventType.Wheel, new WheelInfo(e.Location, e.Delta));
        }
        // Methods
        public void StartRecording(bool keybd, bool mbs, bool mm, bool isk, bool showdesktop, DoubleClickDetectionMethod dcd)
        {
            if (status == SimStatus.Idle)
            {
                status = SimStatus.Running;
                if ((dcdm = dcd) == DoubleClickDetectionMethod.Realtime)
                {
                    realtimeDCD = true;
                }
                lastleftdown = null;
                cnlu = false;
                dcdelay = HookManager.GetDoubleClickTime();
                eveset = new EventSet();
                eveset.Resolution = Screen.PrimaryScreen.Bounds.Size;
                if (!isk)
                {
                    eveset.IgnoreKeyStats = false;
                    eveset.NumLockStat = GetKeyState((int)Keys.NumLock) != 0;
                    eveset.CapsLockStat = GetKeyState((int)Keys.CapsLock) != 0;
                    eveset.ScrollLockStat = GetKeyState((int)Keys.Scroll) != 0;
                }
                else
                    eveset.IgnoreKeyStats = true;
                eveset.ShowDesktop = showdesktop;
                if (recordkeyboard = keybd)
                {
                    HookManager.KeyDown += KeyDown;
                    HookManager.KeyUp += KeyUp;
                }
                if (recordmouseb = mbs)
                {
                    HookManager.MouseDown += MouseDown;
                    HookManager.MouseUp += MouseUp;
                    HookManager.MouseWheel += Wheel;
                }
                if (recordmousem = mm)
                    HookManager.MouseMove += MouseMove;
                sync = new Synchronouser();
                sync.StartSync();
            }
        }
        public void Pause()
        {
            if (status == SimStatus.Running)
            {
                sync.Pause();
                status = SimStatus.Paused;
                HookManager.KeyDown -= KeyDown;
                HookManager.KeyUp -= KeyUp;
                HookManager.MouseDown -= MouseDown;
                HookManager.MouseUp -= MouseUp;
                HookManager.MouseMove -= MouseMove;
                HookManager.MouseWheel -= Wheel;
            }
        }
        public void Resume()
        {
            if (status == SimStatus.Paused)
            {
                status = SimStatus.Running;
                if (recordkeyboard)
                {
                    HookManager.KeyDown += KeyDown;
                    HookManager.KeyUp += KeyUp;
                }
                if (recordmouseb)
                {
                    HookManager.MouseDown += MouseDown;
                    HookManager.MouseUp += MouseUp;
                    HookManager.MouseWheel += Wheel;
                }
                if (recordmousem)
                    HookManager.MouseMove += MouseMove;
                sync.Resume();
            }
        }
        public void StopRecording()
        {
            if (status != SimStatus.Idle)
            {
                sync.StopSync();
                HookManager.KeyDown -= KeyDown;
                HookManager.KeyUp -= KeyUp;
                HookManager.MouseDown -= MouseDown;
                HookManager.MouseUp -= MouseUp;
                HookManager.MouseMove -= MouseMove;
                HookManager.MouseWheel -= Wheel;
                if (dcdm == DoubleClickDetectionMethod.PostRecord)
                {
                    Event lastld = null;
                    Event lastlu = null;
                    bool rnlu = false;
                    foreach (Event eve in eveset.GetEvents())
                    {
                        if (eve.EventType == EventType.LeftDown)
                        {
                            if (lastld != null && eve.EventTime - lastld.EventTime <= dcdelay && dcpercisionrange.Contains(((Point)lastld.Info).X - ((Point)eve.Info).X) && dcpercisionrange.Contains(((Point)lastld.Info).Y - ((Point)eve.Info).Y))
                            {
                                eveset.RemoveEvent(lastld);
                                eveset.RemoveEvent(lastlu);
                                rnlu = true;
                                eve.EventType = EventType.DoubleClick;
                            }
                            else
                                lastld = eve;
                        }
                        else if (eve.EventType == EventType.LeftUp)
                        {
                            if (rnlu)
                            {
                                eveset.RemoveEvent(eve);
                                rnlu = false;
                            }
                            else
                                lastlu = eve;
                        }
                    }
                }
                status = SimStatus.Idle;
            }
        }
        public EventSet GetEventSet()
        {
            return eveset;
        }
        // public Declarations
        public SimStatus Status
        {
            get
            {
                return status;
            }
        }
        public Synchronouser Synchronouser
        {
            get
            {
                return sync;
            }
        }
    }
}
