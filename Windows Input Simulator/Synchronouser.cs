using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simulator
{
    class Synchronouser
    {
        // Declarations
        private DateTime starttime;
        private DateTime stoptime;
        private bool running;
        private int pauseelapsed;
        // Events
        public EventHandler Started;
        public EventHandler Stopped;
        // Methods
        public void StartSync()
        {
            starttime = DateTime.Now;
            running = true;
            Started?.Invoke(this, new EventArgs());
        }
        public void StopSync()
        {
            stoptime = DateTime.Now;
            running = false;
            Stopped?.Invoke(this, new EventArgs());
        }
        public void Pause()
        {
            pauseelapsed = Elapsed;
        }
        public void Resume()
        {
            starttime = DateTime.Now.Subtract(new TimeSpan(pauseelapsed * TimeSpan.TicksPerMillisecond));
        }
        public void Cycle()
        {
            starttime = DateTime.Now;
        }
        public int Elapsed
        {
            get
            {
                if (running)
                {
                    TimeSpan elapsed = DateTime.Now.Subtract(starttime);
                    return Convert.ToInt32(elapsed.TotalMilliseconds);
                }
                else
                {
                    TimeSpan elapsed = stoptime.Subtract(starttime);
                    return Convert.ToInt32(elapsed.TotalMilliseconds);
                }
            }
        }
        public  bool IsRunning
        {
            get
            {
                return running;
            }
        }
    }
}
