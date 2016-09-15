using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace Simulator
{
    //Event Set
    [Serializable]
    class EventSet : ISerializable
    {
        //Declarations
        List<Event> eves;
        Event currenteve;
        Size recorderRes;
        bool numstat;
        bool capsstat;
        bool scrollstat;
        bool ignorestats;
        bool showdesktop;
        int multiplier;
        int nexteve;
        bool endReached;
        //constructer
        public EventSet()
        {
            eves = new List<Event>();
            currenteve = null;
            recorderRes = Screen.PrimaryScreen.Bounds.Size;
            numstat = false;
            capsstat = false;
            scrollstat = false;
            ignorestats = false;
            showdesktop = false;
            multiplier = 1;
            nexteve = 0;
            endReached = false;
        }
        public EventSet(SerializationInfo info, StreamingContext context)
        {
            recorderRes = (Size)info.GetValue("r", typeof(Size));
            numstat = info.GetBoolean("n");
            capsstat = info.GetBoolean("c");
            scrollstat = info.GetBoolean("s");
            ignorestats = info.GetBoolean("i");
            showdesktop = info.GetBoolean("d");
            multiplier = info.GetInt32("m");
            eves = ((Event[])info.GetValue("a", typeof(Event[]))).ToList();
            currenteve = null;
            nexteve = 0;
            endReached = false;
        }
        // Methods
        public void MoveToNextEvent()
        {
            if (nexteve < eves.Count)
            {
                currenteve = eves[nexteve];
                nexteve++;
            }
            if (nexteve >= eves.Count)
                endReached = true;
        }

        public void RestartQueue()
        {
            this.currenteve = null;
            this.nexteve = 0;
            endReached = false;
        }
        // Public Declarations
        public Event CurrentEvent
        {
            get
            {
                return currenteve;
            }
        }
        public bool EndReached
        {
            get
            {
                return endReached;
            }
        }
        public int EventsCount
        {
            get
            {
                return eves.Count;
            }
        }
        public int TotalTime
        {
            get
            {
                if (eves.Count > 0)
                    return Convert.ToInt32(eves[eves.Count - 1].EventTime);
                else
                    return 0;
            }
        }
        public Size Resolution
        {
            get
            {
                return recorderRes;
            }
            set
            {
                recorderRes = value;
            }
        }
        public bool NumLockStat
        {
            get { return numstat; }
            set { numstat = value; }
        }
        public bool CapsLockStat
        {
            get { return capsstat; }
            set { capsstat = value; }
        }
        public bool ScrollLockStat
        {
            get { return scrollstat; }
            set { scrollstat = value; }
        }
        public bool IgnoreKeyStats
        {
            get { return ignorestats; }
            set { ignorestats = value; }
        }
        public bool ShowDesktop
        {
            get { return showdesktop; }
            set { showdesktop = value; }
        }
        public int Multiplier
        {
            get { return multiplier; }
            set { multiplier = value; }
        }
        // Methods
        public void AddEvent(int time, EventType eve, object eveinfo)
        {
            eves.Add(new Event(time, eve, eveinfo));
        }
        public void AddEvent(Event eve)
        {
            if (eve != null)
                eves.Add(eve);
        }
        public void RemoveEvent(Event eve)
        {
            if (eve != null)
                eves.Remove(eve);
        }
        public Event[] GetEvents()
        {
            return eves.ToArray();
        }

        #region ISerializable Members
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("r", recorderRes);
            info.AddValue("n", numstat);
            info.AddValue("c", capsstat);
            info.AddValue("s", scrollstat);
            info.AddValue("i", ignorestats);
            info.AddValue("d", showdesktop);
            info.AddValue("m", multiplier);
            info.AddValue("a", eves.ToArray());
        }
        #endregion
    }
    //-------------------------
    //---------------------------
    //--------------------------------
    // Event
    //--------------------------------
    //---------------------------
    //-------------------------
    public enum EventType
    {
        MouseMove = 0,
        KeyDown = 1,
        KeyUp = 2,
        LeftDown = 3,
        LeftUp = 4,
        RightDown = 5,
        RightUp = 6,
        MiddleDown = 7,
        MiddleUp = 8,
        Wheel = 9,
        XB1Down = 10,
        XB1Up = 11,
        XB2Down = 12,
        XB2Up = 13,
        DoubleClick = 14,
        ExecuteFile = 15
    }
    [Serializable]
    class Event
    {
        int occuretime;
        EventType eveType;
        object eveInfo;
        public Event(int time, EventType eve, object eveinfo)
        {
            occuretime = time;
            eveType = eve;
            eveInfo = eveinfo;
        }
        public int EventTime
        {
            get
            {
                return occuretime;
            }
            set
            {
                occuretime = value;
            }
        }
        public EventType EventType
        {
            get
            {
                return eveType;
            }
            set
            {
                eveType = value;
            }
        }
        public object Info
        {
            get
            {
                return eveInfo;
            }
            set
            {
                eveInfo = value;
            }
        }
    }
    [Serializable]
    class WheelInfo
    {
        Point point;
        int value;
        public WheelInfo(Point p, int v)
        {
            point = p;
            value = v;
        }
        public Point Point
        {
            get { return point; }
        }
        public int Value
        {
            get { return value; }
        }
    }
}
