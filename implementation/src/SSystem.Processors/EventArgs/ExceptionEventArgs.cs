using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSystem.Processors
{
    public class ExceptionEventArgs:EventArgs
    {
        public Exception Exception { get; protected set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            Exception exp = Exception;
            do {
                if (exp != null)
                {
                    sb.AppendLine(exp.Message);
                    sb.AppendLine(exp.StackTrace);
                    exp = exp.InnerException;
                }
            }
            while (exp!=null);
            return sb.ToString();
        }

        public ExceptionEventArgs(Exception ex)
        {
            Exception = ex;
        }
    }
}
