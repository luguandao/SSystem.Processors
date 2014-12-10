using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSystem.Processors
{
    public class EstimateEventArgs:EventArgs
    {
        public int EstimateRemainMilliseconds { get; protected set; }

        public override string ToString()
        {
            return TimeSpan.FromMilliseconds(EstimateRemainMilliseconds).ToString();
        }
        public EstimateEventArgs(int remain)
        {
            EstimateRemainMilliseconds = remain;
        }
    }
}
