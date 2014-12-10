using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSystem.Processors
{
    public class StatusChangedEventArgs : EventArgs
    {
        public string Message { get; protected set; }

        public StatusChangedEventArgs()
        {

        }

        public StatusChangedEventArgs(string message)
        {
            Message = message;
        }
    }
}
