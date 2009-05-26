using System;
using System.Collections.Generic;
using System.Text;
using GeniusTetris.Core;

namespace GeniusTetris.Utils
{
    class WpfTimer : ITimer
    {
        System.Windows.Threading.DispatcherTimer _Timer = new System.Windows.Threading.DispatcherTimer();

        public WpfTimer()
        {
            _Timer.Tick += new EventHandler(_Timer_Tick);
        }

        void _Timer_Tick(object sender, EventArgs e)
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
                return _Timer.Interval.TotalMilliseconds;
            }
            set
            {
                _Timer.Interval = TimeSpan.FromMilliseconds(value);
            }
        }

        public event EventHandler OnElapsed;

        #endregion
    }
}
