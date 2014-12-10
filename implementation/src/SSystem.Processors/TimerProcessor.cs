using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace SSystem.Processors
{
    public class TimerProcessor : Processor
    {
        private System.Timers.Timer m_Timer;

        public EventHandler<EventArgs> Tick;
        public EventHandler<EventArgs> TimeUp;

        public DateTime InvokeDateTime { get; set; }
        public PerTimerType TimerType { get; set; }

        /// <summary>
        /// 计算下一个周期时所用的参数，默认为1，比如间隔1小时执行一次，如果设置ScaleForNextCycle=0.5,则是半小时执行一次
        /// 注意，对年和月无效
        /// </summary>
        public double ScaleForNextCycle { get; set; }

        /// <summary>
        /// 是否是循环触发时间事件，默认为true
        /// </summary>
        public bool IsCycle
        {
            get { return TimerType != PerTimerType.Once; }
        }

        private bool m_isInitTimer;

        private bool m_IsTimerStoped;

        public TimerProcessor()
        {
            ScaleForNextCycle = 1;
        }

        public TimerProcessor(DateTime invokeDateTime, PerTimerType timerType)
            : this()
        {
            InvokeDateTime = invokeDateTime;
            TimerType = timerType;
            m_isInitTimer = true;
        }

        private double CalcRemainSeconds(DateTime end, DateTime start)
        {
            TimeSpan ts = end - start;
            if (ts.TotalSeconds < 0) return 0;
            return ts.TotalSeconds;
        }

        private void CalcNextCycle()
        {
            switch (TimerType)
            {
                case PerTimerType.Second:
                    InvokeDateTime = InvokeDateTime.AddSeconds(ScaleForNextCycle);
                    break;
                case PerTimerType.Minitus:
                    InvokeDateTime = InvokeDateTime.AddMinutes(ScaleForNextCycle);
                    break;
                case PerTimerType.Hour:
                    InvokeDateTime = InvokeDateTime.AddHours(ScaleForNextCycle);
                    break;
                case PerTimerType.Day:
                    InvokeDateTime = InvokeDateTime.AddDays(ScaleForNextCycle);
                    break;
                case PerTimerType.Week:
                    InvokeDateTime = InvokeDateTime.AddDays(7 * ScaleForNextCycle);
                    break;
                case PerTimerType.Month:
                    InvokeDateTime = InvokeDateTime.AddMonths(1);
                    break;
                case PerTimerType.Year:
                    InvokeDateTime = InvokeDateTime.AddYears(1);
                    break;

            }
        }

        protected void OnTick(object sender, EventArgs e)
        {
            if (Tick != null)
            {
                Tick(sender, e);
            }
        }

        protected void OnTimeUp(object sender, EventArgs e)
        {
            if (TimeUp != null)
            {
                TimeUp(this, e);
            }
        }

        protected override void _Start()
        {
            if (m_isInitTimer)
            {
                m_Timer = new System.Timers.Timer(1000); //(TimerCallBack, this, 0, 1000);
                m_Timer.Elapsed += m_Timer_Elapsed;
                m_Timer.Start();
                m_IsTimerStoped = false;
                AdjustInvokeTime();
                while (!m_IsTimerStoped && !m_CancelToken.IsCancellationRequested)
                {
                    if (!IsCycle && DateTime.Now > InvokeDateTime)
                    {
                        break;
                    }
                    Thread.Sleep(100);
                }
                StopTimer();
            }
        }

        private void AdjustInvokeTime()
        {
            while (InvokeDateTime < DateTime.Now)
            {
                CalcNextCycle();
            }
        }

        void m_Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            double remains = CalcRemainSeconds(InvokeDateTime, DateTime.Now);
            Task.Factory.StartNew(() =>
            {
                OnTick(this, new TimerEventArgs(remains));
            });


            if (remains <= 0)
            {
                Task.Factory.StartNew(() =>
                {
                    OnTimeUp(this, new EventArgs());
                });

                if (!IsCycle)
                {
                    StopTimer();
                }
                else
                {
                    CalcNextCycle();
                }
            }
        }

        private void StopTimer()
        {
            if (m_Timer != null)
            {
                m_Timer.Stop();
                m_Timer.Dispose();
            }
            m_IsTimerStoped = true;

        }

        protected override void _Stop()
        {
            StopTimer();
            base._Stop();
        }
    }




}
