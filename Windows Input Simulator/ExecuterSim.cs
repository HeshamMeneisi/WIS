using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Windows_Input_Simulator;

namespace Simulator
{
    class ExecuterSim
    {
        // Declarations
        EventSet eveset;
        Synchronouser sync;
        double xdif;
        double ydif;
        bool keybd;
        bool mbs;
        bool mm;
        bool lasteve = false;
        bool showdt = false;
        int multiplier;
        Point imp;
        ExecuterSim subexecuter = null;
        SimStatus status = SimStatus.Idle;
        MicroLibrary.MicroTimer executer;
        // Enums
        public enum SimStatus
        {
            Running = 0,
            Paused = 1,
            Idle = 2
        }
        // Flags
        [Flags]
        public enum KeyboardFlags
        {
            KeyUp = 0x00000002, KeyDown = 0x0
        }
        [Flags]
        public enum MouseEventFlags : uint
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            Absolute = 0x00008000,
            RightDown = 0x00000008,
            RightUp = 0x00000010,
            Wheel = 0x00000800,
            XDown = 0x00000080,
            XUp = 0x00000100
        }
        public enum MouseEventDataXButtons : uint
        {
            XBUTTON1 = 0x00000001,
            XBUTTON2 = 0x00000002
        }
        // Event Handlers
        public delegate void ExeFinishedEventHandler(ExecuterSim sender, ExecuterFinishedEventArgs e);
        public event ExeFinishedEventHandler Finished;
        private bool rsm;

        // Imports
        [DllImport("user32.dll")]
        private static extern void keybd_event(int bVk, int bScan, KeyboardFlags dwFlags, UIntPtr dwExtraInfo);
        [DllImport("User32.dll")]
        static extern void mouse_event(MouseEventFlags dwFlags, int dx, int dy, int dwData, UIntPtr dwExtraInfo);
        [DllImport("user32.dll")]
        public static extern short GetKeyState(int keyCode);
        // Methods
        public void LoadSet(EventSet settoload)
        {
            eveset = settoload;
            eveset.RestartQueue();
            xdif = (double)Screen.PrimaryScreen.Bounds.Width / eveset.Resolution.Width;
            ydif = (double)Screen.PrimaryScreen.Bounds.Height / eveset.Resolution.Height;
        }
        public void StartExecuting(bool exekebdevents, bool exembsevents, bool exemmevents, bool rsk, int mp, bool showdt, bool restorem)
        {
            if (status == SimStatus.Idle)
            {
                status = SimStatus.Running;
                lasteve = false;
                if (mp > 0)
                    multiplier = mp;
                else
                    throw (new Exception("Executer: Multiplier should be an integer larger than 0"));
                eveset.MoveToNextEvent();
                keybd = exekebdevents;
                mbs = exembsevents;
                mm = exemmevents;
                if (rsk)
                {
                    if ((GetKeyState((int)Keys.NumLock) != 0) != eveset.NumLockStat)
                    {
                        keybd_event((int)Keys.NumLock, 0, KeyboardFlags.KeyDown, UIntPtr.Zero); keybd_event((int)Keys.NumLock, 0, KeyboardFlags.KeyUp, UIntPtr.Zero);
                    }
                    if ((GetKeyState((int)Keys.CapsLock) != 0) != eveset.CapsLockStat)
                    {
                        keybd_event((int)Keys.CapsLock, 0, KeyboardFlags.KeyDown, UIntPtr.Zero); keybd_event((int)Keys.CapsLock, 0, KeyboardFlags.KeyUp, UIntPtr.Zero);
                    }
                    if ((GetKeyState((int)Keys.Scroll) != 0) != eveset.ScrollLockStat)
                    {
                        keybd_event((int)Keys.Scroll, 0, KeyboardFlags.KeyDown, UIntPtr.Zero); keybd_event((int)Keys.Scroll, 0, KeyboardFlags.KeyUp, UIntPtr.Zero);
                    }

                }
                if (this.showdt = showdt)
                    DisplayControl.MinimizeAll();
                rsm = restorem;
                imp = Cursor.Position;
                sync = new Synchronouser();
                executer = new MicroLibrary.MicroTimer(1000000 / Windows_Input_Simulator.Properties.Settings.Default.evespersecond);
                executer.MicroTimerElapsed += executer_elapsed;
                sync.StartSync();
                executer.Start();
            }
        }
        public void StopExecuting()
        {
            Halt(true);            
        }

        private void Halt(bool intr = false)
        {
            if (status != SimStatus.Idle)
            {
                status = SimStatus.Idle;
                executer.Stop();
                sync.StopSync();
                if (subexecuter != null)
                    subexecuter.Halt(intr);
                OnFinished(intr);
            }
        }

        private void OnFinished(bool intr)
        {
            if (showdt)
                DisplayControl.RestoreAll();
            if (rsm)
                Cursor.Position = imp;
            Finished?.Invoke(this, new ExecuterFinishedEventArgs(intr));
        }

        public void Pause()
        {
            if (status == SimStatus.Running)
            {
                sync.Pause();
                executer.Stop();
                if (subexecuter != null)
                    subexecuter.Pause();
                status = SimStatus.Paused;
            }
        }
        public void Resume()
        {
            if (status == SimStatus.Paused)
            {
                if (subexecuter != null && subexecuter.status == SimStatus.Paused)
                {
                    subexecuter.Resume();
                }
                else
                {
                    sync.Resume();
                    executer.Start();
                }
                status = SimStatus.Running;
            }
        }
        private void Hold()
        {
            sync.Pause();
            executer.Stop();
        }
        public void Continue()
        {
            sync.Resume();
            executer.Start();
        }
        private void Execute(Event evetoexecute)
        {
            EventType type = evetoexecute.EventType;
            Point coordspoint;
            if (type == EventType.KeyDown && keybd)
            {
                if (!HotkeyManager.Hotkeys.Contains((Keys)Convert.ToInt32(evetoexecute.Info)))
                {
                    keybd_event(Convert.ToInt32(evetoexecute.Info), 0, KeyboardFlags.KeyDown, UIntPtr.Zero);
                }
            }
            else if (type == EventType.KeyUp && keybd)
            {
                if (!HotkeyManager.Hotkeys.Contains((Keys)Convert.ToInt32(evetoexecute.Info)))
                {
                    keybd_event(Convert.ToInt32(evetoexecute.Info), 0, KeyboardFlags.KeyUp, UIntPtr.Zero);
                }
            }
            else if (type == EventType.LeftDown && mbs)
            {
                coordspoint = new Point(Convert.ToInt32(((Point)evetoexecute.Info).X * xdif), Convert.ToInt32(((Point)evetoexecute.Info).Y * ydif));
                Cursor.Position = coordspoint;
                mouse_event(MouseEventFlags.LeftDown, 0, 0, 0, UIntPtr.Zero);
            }
            else if (type == EventType.LeftUp && mbs)
            {
                coordspoint = new Point(Convert.ToInt32(((Point)evetoexecute.Info).X * xdif), Convert.ToInt32(((Point)evetoexecute.Info).Y * ydif));
                Cursor.Position = coordspoint;
                mouse_event(MouseEventFlags.LeftUp, coordspoint.X, coordspoint.Y, 0, UIntPtr.Zero);
            }
            else if (type == EventType.RightDown && mbs)
            {
                coordspoint = new Point(Convert.ToInt32(((Point)evetoexecute.Info).X * xdif), Convert.ToInt32(((Point)evetoexecute.Info).Y * ydif));
                Cursor.Position = coordspoint;
                mouse_event(MouseEventFlags.RightDown, coordspoint.X, coordspoint.Y, 0, UIntPtr.Zero);
            }
            else if (type == EventType.RightUp && mbs)
            {
                coordspoint = new Point(Convert.ToInt32(((Point)evetoexecute.Info).X * xdif), Convert.ToInt32(((Point)evetoexecute.Info).Y * ydif));
                Cursor.Position = coordspoint;
                mouse_event(MouseEventFlags.RightUp, coordspoint.X, coordspoint.Y, 0, UIntPtr.Zero);
            }
            else if (type == EventType.MiddleDown && mbs)
            {
                coordspoint = new Point(Convert.ToInt32(((Point)evetoexecute.Info).X * xdif), Convert.ToInt32(((Point)evetoexecute.Info).Y * ydif));
                Cursor.Position = coordspoint;
                mouse_event(MouseEventFlags.MiddleDown, coordspoint.X, coordspoint.Y, 0, UIntPtr.Zero);
            }
            else if (type == EventType.MiddleUp && mbs)
            {
                coordspoint = new Point(Convert.ToInt32(((Point)evetoexecute.Info).X * xdif), Convert.ToInt32(((Point)evetoexecute.Info).Y * ydif));
                Cursor.Position = coordspoint;
                mouse_event(MouseEventFlags.MiddleUp, coordspoint.X, coordspoint.Y, 0, UIntPtr.Zero);
            }
            else if (type == EventType.XB1Down && mbs)
            {
                coordspoint = new Point(Convert.ToInt32(((Point)evetoexecute.Info).X * xdif), Convert.ToInt32(((Point)evetoexecute.Info).Y * ydif));
                Cursor.Position = coordspoint;
                mouse_event(MouseEventFlags.XDown, coordspoint.X, coordspoint.Y, (int)MouseEventDataXButtons.XBUTTON1, UIntPtr.Zero);
            }
            else if (type == EventType.XB1Up && mbs)
            {
                coordspoint = new Point(Convert.ToInt32(((Point)evetoexecute.Info).X * xdif), Convert.ToInt32(((Point)evetoexecute.Info).Y * ydif));
                Cursor.Position = coordspoint;
                mouse_event(MouseEventFlags.XUp, coordspoint.X, coordspoint.Y, (int)MouseEventDataXButtons.XBUTTON1, UIntPtr.Zero);
            }
            else if (type == EventType.XB2Down && mbs)
            {
                coordspoint = new Point(Convert.ToInt32(((Point)evetoexecute.Info).X * xdif), Convert.ToInt32(((Point)evetoexecute.Info).Y * ydif));
                Cursor.Position = coordspoint;
                mouse_event(MouseEventFlags.XDown, coordspoint.X, coordspoint.Y, (int)MouseEventDataXButtons.XBUTTON2, UIntPtr.Zero);
            }
            else if (type == EventType.XB2Up && mbs)
            {
                coordspoint = new Point(Convert.ToInt32(((Point)evetoexecute.Info).X * xdif), Convert.ToInt32(((Point)evetoexecute.Info).Y * ydif));
                Cursor.Position = coordspoint;
                mouse_event(MouseEventFlags.XUp, coordspoint.X, coordspoint.Y, (int)MouseEventDataXButtons.XBUTTON2, UIntPtr.Zero);
            }
            else if (type == EventType.Wheel && mbs)
            {
                WheelInfo wi = ((WheelInfo)evetoexecute.Info);
                coordspoint = new Point(Convert.ToInt32(wi.Point.X * xdif), Convert.ToInt32(wi.Point.Y * ydif));
                Cursor.Position = coordspoint;
                mouse_event(MouseEventFlags.Wheel, 0, 0, wi.Value, UIntPtr.Zero);
            }
            else if (type == EventType.MouseMove && mm)
            {
                coordspoint = new Point(Convert.ToInt32(((Point)evetoexecute.Info).X * xdif), Convert.ToInt32(((Point)evetoexecute.Info).Y * ydif));
                Cursor.Position = coordspoint;
            }
            else if (type == EventType.DoubleClick && mbs)
            {
                coordspoint = new Point(Convert.ToInt32(((Point)evetoexecute.Info).X * xdif), Convert.ToInt32(((Point)evetoexecute.Info).Y * ydif));
                Cursor.Position = coordspoint;
                mouse_event(MouseEventFlags.LeftDown, 0, 0, 0, UIntPtr.Zero);
                mouse_event(MouseEventFlags.LeftUp, 0, 0, 0, UIntPtr.Zero);
                mouse_event(MouseEventFlags.LeftDown, 0, 0, 0, UIntPtr.Zero);
                mouse_event(MouseEventFlags.LeftUp, 0, 0, 0, UIntPtr.Zero);
            }
            else if (type == EventType.ExecuteFile)
            {
                string filename = evetoexecute.Info.ToString();
                if (File.Exists(filename))
                {
                    Hold();
                    EventSet es = new EventSet();
                    FileManager.ReadFile(ref es, filename);
                    subexecuter = new ExecuterSim();
                    subexecuter.LoadSet(es);
                    subexecuter.Finished += subexfinished;
                    subexecuter.StartExecuting(keybd, mbs, mm, !es.IgnoreKeyStats, es.Multiplier, es.ShowDesktop, es.RestoreMouse);
                }
            }
        }
        private void subexfinished(object sender, EventArgs e)
        {
            if (lasteve)
            {
                Halt();
            }
            else
                Continue();
        }
        public void ReleaseAllDownKeys()
        {
            mouse_event(MouseEventFlags.LeftUp, 0, 0, 0, UIntPtr.Zero);
            foreach (int key in Keys.GetValues(typeof(Keys)))
            {
                keybd_event(key, 0, KeyboardFlags.KeyUp, UIntPtr.Zero);
            }
        }
        // executer timer
        private void executer_elapsed(object sender, EventArgs e)
        {
            if (eveset.CurrentEvent != null && ((double)eveset.CurrentEvent.EventTime / multiplier) <= sync.Elapsed && status == SimStatus.Running)
            {
                try
                {
                    Execute(eveset.CurrentEvent);
                }
                catch (Exception ex)
                {
                    // TODO: Should be through a logging event
                    Program.mainForm.AddLog(DateTime.Now, "Couldn't Execute Event " + eveset.GetEvents().ToList().IndexOf(eveset.CurrentEvent) + " --Executing Stopped.\n" + ex.Message);
                    Halt(true);
                }
                if (eveset.EndReached)
                {
                    if (subexecuter == null || subexecuter.status == SimStatus.Idle)
                        Halt();
                    lasteve = true;
                }
                else
                    eveset.MoveToNextEvent();
                sync.Cycle();
            }
        }

        public void StartExecuting(SOptions opt)
        {
            StartExecuting(opt.KeyboardActive, opt.MouseButtonsAactive, opt.MouseMovementActive, opt.RestoreMousePos, opt.Multiplier, opt.ShowDesktop, opt.RestoreMousePos);
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

    internal class ExecuterFinishedEventArgs : EventArgs
    {
        private bool intr;

        public ExecuterFinishedEventArgs(bool intr)
        {
            this.intr = intr;
        }

        public bool Interrupted
        {
            get
            {
                return intr;
            }
        }
    }
}
