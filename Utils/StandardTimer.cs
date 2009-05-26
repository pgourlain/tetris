using System;
using System.Collections.Generic;
using System.Text;
using GeniusTetris.Core;

namespace GeniusTetris.Utils
{
    class StandardTimer : ITimer
    {
        private System.Timers.Timer _Timer = new System.Timers.Timer();


        public StandardTimer()
        {
            _Timer.Elapsed += new System.Timers.ElapsedEventHandler(_Timer_Elapsed);
        }

        void _Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (OnElapsed != null)
                OnElapsed(this, EventArgs.Empty);
        }

        #region ITimer Members

        public void Start()
        {
            _Timer.Start();
        }

        public void Stop()
        {
            _Timer.Stop();
        }

        public double Interval
        {
            get
            {
                return _Timer.Interval;
            }
            set
            {
                _Timer.Interval = value;
            }
        }

        public event EventHandler OnElapsed;

        #endregion
    }
}
