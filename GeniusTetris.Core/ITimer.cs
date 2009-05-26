using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusTetris.Core
{
    public interface ITimer
    {
        void Start();
        void Stop();
        double Interval { get; set;}
        event EventHandler OnElapsed;
    }
}
