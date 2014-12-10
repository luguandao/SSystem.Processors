using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSystem.Processors
{
    public class TimerEventArgs : EventArgs
    {
        public double RemainSeconds { get; set; }

        public TimerEventArgs(double remainSeconds = 0)
        {
            RemainSeconds = remainSeconds;
        }

        public override string ToString()
        {
            return TimeSpan.FromSeconds(RemainSeconds).ToString();
        }
    }
}
