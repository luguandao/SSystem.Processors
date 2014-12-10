using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSystem.Processors
{
    public class ProgressChangedEventArgs : EventArgs
    {
        public int CurrentValue { get; protected set; }
        public int Count { get; protected set; }
        public double DegreeOfCompletion
        {
            get
            {
                if (Count == 0) return 0;
                return CurrentValue * 1.0 / Count;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}%", (DegreeOfCompletion * 100).ToString("F2"));
        }

        public ProgressChangedEventArgs(int curr, int count)
        {
            CurrentValue = curr;
            Count = count;
        }
    }
}
