using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSystem.Processors
{
    public class ProcessorCompletedEventArgs:EventArgs
    {
        public Result Result { get; protected set; }
        /// <summary>
        /// 是否要阻止执行下一个Processor
        /// </summary>
        public bool StopExecuteNextProcessor { get; set; }

        public ProcessorCompletedEventArgs() { }
        public ProcessorCompletedEventArgs(Result result,bool stopExecuteNextProcessor=false)
        {
            Result = result;
            StopExecuteNextProcessor = stopExecuteNextProcessor;
        }
    }
}
