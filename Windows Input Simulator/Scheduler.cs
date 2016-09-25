using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace Simulator
{
    ///////////////////////////
    /// <summary>
    /// Scheduler
    /// </summary>
    ///////////////////////////
    class Scheduler
    {
        // Declarations
        List<SimEvent> events;
        System.Timers.Timer schedulertimer;
        bool enabled;
        SimEvent eventintprogress;
        // constructor
        public Scheduler(bool enable)
        {
            events = new List<SimEvent>();
            schedulertimer = new System.Timers.Timer { Interval = 1000, AutoReset = true };
            enabled = enable;
            schedulertimer.Elapsed += elapsed;
            schedulertimer.Start();
            eventintprogress = null;
        }
        // event handler
        public event EventRaisedEventHandler EventRaised;
        // Methods
        public void SaveEvents()
        {
            Stream str = File.Create(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "SchedulerTable.tbl"));
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(str, events.ToArray());
            str.Close();
        }
        public void LoadEvents()
        {
            if (File.Exists(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "SchedulerTable.tbl")))
            {
                Stream str = File.OpenRead(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "SchedulerTable.tbl"));
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    events = ((SimEvent[])bf.Deserialize(str)).ToList();
                }
                catch
                {
                    MessageBox.Show(Windows_Input_Simulator.Program.mainForm, "SchedulerTable.tbl -Is Curropted!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                str.Close();
            }
        }
        public void AddEvent(SimEvent eventtoadd)
        {
            events.Add(eventtoadd);
        }
        public ListViewItem[] GetEventsTable()
        {
            List<ListViewItem> list = new List<ListViewItem>();
            if (events.Count > 0)
            {
                foreach (SimEvent eve in events)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = eve.Type.ToString();
                    ListViewItem.ListViewSubItem sub1 = new ListViewItem.ListViewSubItem();
                    if (eve.Type == SimEventType.Daily)
                    {
                        sub1.Text = eve.Time.ToShortTimeString();
                    }
                    else if (eve.Type == SimEventType.Hourly)
                    {
                        sub1.Text = "At Minute " + eve.Time.Minute.ToString();
                    }
                    else if (eve.Type == SimEventType.Weekly)
                    {
                        sub1.Text = "On " + eve.Time.DayOfWeek.ToString() + " At " + eve.Time.ToShortTimeString();
                    }
                    else if (eve.Type == SimEventType.Monthly)
                    {
                        sub1.Text = "On " + eve.Time.Day + " At " + eve.Time.ToShortTimeString();
                    }
                    else if (eve.Type == SimEventType.Yearly)
                    {
                        sub1.Text = "On " + eve.Time.Day + "/" + eve.Time.Month + " At " + eve.Time.ToShortTimeString();
                    }
                    else
                    {
                        sub1.Text = "On " + eve.Time.ToShortDateString() + " At " + eve.Time.ToShortTimeString();
                    }
                    ListViewItem.ListViewSubItem sub2 = new ListViewItem.ListViewSubItem();
                    sub2.Text = eve.FileName;
                    item.SubItems.AddRange(new ListViewItem.ListViewSubItem[] { sub1, sub2 });
                    item.Tag = eve;
                    list.Add(item);
                }
            }
            return list.ToArray();
        }
        public void ClearEvents()
        {
            events.Clear();
        }
        // public Declarations
        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
            }
        }
        public SimEvent EventInProgress
        {
            get
            {
                return eventintprogress;
            }
            set
            {
                eventintprogress = value;
            }
        }
        // timer
        private void elapsed(object sender, EventArgs e)
        {
            if (enabled)
            {
                foreach (SimEvent eve in events)
                {
                    if (eve != eventintprogress && eve.TimeToRaise())
                    {
                        eventintprogress = eve;
                        eve.Raise();
                        eve.LastRaise = DateTime.Now;
                        EventRaised(this, new EventRaisedEventArgs(eve));
                    }
                }
            }
        }

    }
    ///////////////////////////
    /// <summary>
    /// SimEvent
    /// </summary>
    ///////////////////////////
    public enum SimEventType
    {
        Hourly = 0, Daily = 1, Weekly = 2, Monthly = 3, Yearly = 4, Once = 5
    }
    [Serializable]
    public class SimEvent : ISerializable
    {
        // Declarations
        DateTime eventtime;
        string filename;
        SimEventType type;
        DateTime lastraise;
        // constructors
        public SimEvent(DateTime time, string fname, SimEventType evtype, DateTime last)
        {
            eventtime = time;
            filename = fname;
            type = evtype;
        }
        public SimEvent(SerializationInfo info, StreamingContext context)
        {
            eventtime = info.GetDateTime("et");
            lastraise = info.GetDateTime("lr");
            type = (SimEventType)info.GetInt32("ty");
            filename = info.GetString("fn");
        }
        public SimEvent(string eventstring)
        {
            string[] vals = eventstring.Substring(7, eventstring.Length - 12).Split('|');
            eventtime = DateTime.Parse(vals[0]);
            filename = vals[1];
            type = (SimEventType)Convert.ToInt32(vals[2]);
            lastraise = DateTime.Parse(vals[3]);
        }
        // Methods
        public bool TimeToRaise()
        {
            DateTime now = DateTime.Now;
            if (type == SimEventType.Once)
            {
                if (eventtime.Year == now.Year && eventtime.Month == now.Month && eventtime.Day == now.Day && eventtime.Hour == now.Hour && eventtime.Minute == now.Minute && DateTime.Now.Subtract(lastraise).TotalMinutes > 1)
                {
                    return true;
                }
                return false;
            }
            else if (type == SimEventType.Yearly)
            {
                if (eventtime.Month == now.Month && eventtime.Day == now.Day && eventtime.Hour == now.Hour && eventtime.Minute == now.Minute && DateTime.Now.Subtract(lastraise).TotalMinutes > 1)
                {
                    return true;
                }
            }
            else if (type == SimEventType.Monthly)
            {
                if (eventtime.Day == now.Day && eventtime.Hour == now.Hour && eventtime.Minute == now.Minute && DateTime.Now.Subtract(lastraise).TotalMinutes > 1)
                {
                    return true;
                }
                return false;
            }
            else if (type == SimEventType.Daily)
            {
                if (eventtime.Hour == now.Hour && eventtime.Minute == now.Minute && DateTime.Now.Subtract(lastraise).TotalMinutes > 1)
                {
                    return true;
                }
                return false;
            }
            else if (type == SimEventType.Weekly)
            {
                if (eventtime.DayOfWeek == now.DayOfWeek && eventtime.Hour == now.Hour && eventtime.Minute == now.Minute && DateTime.Now.Subtract(lastraise).TotalMinutes > 1)
                {
                    return true;
                }
                return false;
            }
            else if (type == SimEventType.Hourly)
            {
                if (eventtime.Hour == now.Hour && eventtime.Minute == now.Minute && DateTime.Now.Subtract(lastraise).TotalMinutes > 1)
                {
                    return true;
                }
            }
            return false;
        }
        public void Raise()
        {
            ExecuterSim executer = new ExecuterSim();
            if (File.Exists(filename))
            {
                EventSet set = new EventSet();
                FileManager.ReadFile(ref set, filename);
                executer.LoadSet(set);
                executer.StartExecuting(true, true, true, !set.IgnoreKeyStats, set.Multiplier, set.ShowDesktop, set.RestoreMouse);
            }
        }
        // public Declarations
        public DateTime Time
        {
            get
            {
                return eventtime;
            }
        }
        public string FileName
        {
            get
            {
                return filename;
            }
        }
        public SimEventType Type
        {
            get
            {
                return type;
            }
        }
        public DateTime LastRaise
        {
            get
            {
                return lastraise;
            }
            set
            {
                lastraise = value;
            }
        }

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("et", eventtime);
            info.AddValue("lr", lastraise);
            info.AddValue("ty", type.GetHashCode());
            info.AddValue("fn", filename);
        }

        #endregion
    }
    ///////////////////////////
    /// <summary>
    /// Event Handler/Args
    /// </summary>
    ///////////////////////////
    public delegate void EventRaisedEventHandler(object sender, EventRaisedEventArgs e);
    public class EventRaisedEventArgs
    {
        // Declarations
        SimEvent raisedevent;
        // constructor
        public EventRaisedEventArgs(SimEvent eve)
        {
            raisedevent = eve;
        }
        // public Declarations
        public SimEvent RaisedEvent
        {
            get
            {
                return raisedevent;
            }
        }
    }
}
