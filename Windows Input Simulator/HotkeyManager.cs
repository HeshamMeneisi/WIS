using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GlobalHook;

namespace Simulator
{
    class HotkeyManager
    {
        // Declarations
        static Keys startRecording;
        static Keys pauseRecording;
        static Keys stopRecording;
        static Keys startExecuting;
        static Keys pauseExecuting;
        static Keys stopExecuting;
        static Keys[] hotkeys;
        // Methods
        public static void Init()
        {
            HookManager.KeyDown += Key_Down;
            HookManager.KeyUp += Key_Up;
        }
        public static void SetHotkeys(Keys startrc, Keys precord, Keys stoprc, Keys startex, Keys pex, Keys stopex)
        {
            startRecording = startrc;
            pauseRecording = precord;
            stopRecording = stoprc;
            startExecuting = startex;
            pauseExecuting = pex;
            stopExecuting = stopex;
            hotkeys = new Keys[] { startRecording, pauseRecording, stopRecording, startExecuting, pauseExecuting, stopExecuting };
        }
        // Events
        private static void Key_Down(KBKeyEventArgs e)
        {
            if (hotkeys.Contains(e.Key))
            {
                e.Suppress = true;
                if (e.Key == startRecording)
                {
                    HotkeyPressed(new HotkeyPressedEventArgs(Hotkey.StartRecording));
                }
                else if (e.Key == pauseRecording)
                {
                    HotkeyPressed(new HotkeyPressedEventArgs(Hotkey.PauseRecording));
                }
                else if (e.Key == stopRecording)
                {
                    HotkeyPressed(new HotkeyPressedEventArgs(Hotkey.StopRecording));
                }
                else if (e.Key == startExecuting)
                {
                    HotkeyPressed(new HotkeyPressedEventArgs(Hotkey.StartExecuting));
                }
                else if (e.Key == pauseExecuting)
                {
                    HotkeyPressed(new HotkeyPressedEventArgs(Hotkey.PauseExecuting));
                }
                else if(e.Key == stopExecuting)
                {
                    HotkeyPressed(new HotkeyPressedEventArgs(Hotkey.StopExecuting));
                }
            }
        }
        private static void Key_Up(KBKeyEventArgs e)
        {
            if (hotkeys.Contains(e.Key))
            {
                e.Suppress = true;
            }
        }

        public static event HotkeyPressedEventHandler HotkeyPressed;
        // Public Declarations
        public static Keys StartRecordingHotkey
        {
            get
            {
                return startRecording;
            }
            set
            {
                startRecording = value;
                hotkeys = new Keys[] { startRecording, pauseRecording, stopRecording, startExecuting, pauseExecuting, stopExecuting };
            }
        }
        public static Keys PauseRecordingHotkey
        {
            get
            {
                return pauseRecording;
            }
            set
            {
                pauseRecording = value;
                hotkeys = new Keys[] { startRecording, pauseRecording, stopRecording, startExecuting, pauseExecuting, stopExecuting };
            }
        }
        public static Keys StopRecordingHotkey
        {
            get
            {
                return stopRecording;
            }
            set
            {
                stopRecording = value;
                hotkeys = new Keys[] { startRecording, pauseRecording, stopRecording, startExecuting, pauseExecuting, stopExecuting };
            }
        }
        public static Keys StartExecutingHotkey
        {
            get
            {
                return startExecuting;
            }
            set
            {
                startExecuting = value;
                hotkeys = new Keys[] { startRecording, pauseRecording, stopRecording, startExecuting, pauseExecuting, stopExecuting };
            }
        }
        public static Keys PauseExecutingHotkey
        {
            get
            {
                return pauseExecuting;
            }
            set
            {
                pauseExecuting = value;
                hotkeys = new Keys[] { startRecording, pauseRecording, stopRecording, startExecuting, pauseExecuting, stopExecuting };
            }
        }
        public static Keys StopExecutingHotkey
        {
            get
            {
                return stopExecuting;
            }
            set
            {
                stopExecuting = value;
                hotkeys = new Keys[] { startRecording, pauseRecording, stopRecording, startExecuting, pauseExecuting, stopExecuting };
            }
        }
        public static Keys[] Hotkeys
        {
            get
            {
                return hotkeys;
            }
        }
    }
    public enum Hotkey
    {
        StartRecording,PauseRecording,StopRecording,StartExecuting,PauseExecuting,StopExecuting
    }
    public delegate void HotkeyPressedEventHandler(HotkeyPressedEventArgs e);
    public class HotkeyPressedEventArgs
    {
        Hotkey key;
        public HotkeyPressedEventArgs(Hotkey keypressed)
        {
            key = keypressed;
        }
        public Hotkey KeyPressed
        {
            get
            {
                return key;
            }
        }
    }
}
