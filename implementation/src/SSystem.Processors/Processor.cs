using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSystem.Processors
{
    /// <summary>
    /// 处理任务的基类
    /// </summary>
    public abstract class Processor
    {
        private int m_WaitMaxTimeout = int.MaxValue;

        private Result m_RunResult = new Result();
        private ProcessorCompletedEventArgs m_processorCompletedEventArgs;

        protected bool m_RunAsThread;

        /// <summary>
        /// 获取等待任务完成的最长时间(毫秒)
        /// </summary>
        public int WaitMaxTimeout
        {
            get
            {
                return m_WaitMaxTimeout;
            }
            protected set
            {
                m_WaitMaxTimeout = value;
            }
        }

        #region define of events

        /// <summary>
        /// 当处理器开始时，自动触发此事件
        /// </summary>
        public EventHandler Starting;

        /// <summary>
        /// 当处理器结束时，自动触发此事件
        /// </summary>
        public EventHandler<ProcessorCompletedEventArgs> Completed;
        // public EventHandler <Result, ProcessorCompletedEventArgs> Completed;

        /// <summary>
        /// 进度变化后，触发此事件
        /// </summary>
        public EventHandler<ProgressChangedEventArgs> ProgressChanged;

        /// <summary>
        /// 预估剩余时间，此事件需要先触发事件ProgressChanged
        /// </summary>
        public EventHandler<EstimateEventArgs> EstimateRemains;

        public EventHandler<ExceptionEventArgs> ExceptionOccured;

        public EventHandler StatusChanged;

        #endregion

        protected CancellationToken m_CancelToken;

        private static readonly int _MaxSaved = 500;

        private Stopwatch m_pendTimeMilliSeconds;

        /// <summary>
        /// 返回总共运行的时间(毫秒)
        /// </summary>
        public long TotalTimeByMilliseconds { get; private set; }

        #region method of events
        /// <summary>
        /// [底层使用]当任务开启时，自动会触发
        /// </summary>
        protected virtual void OnStarting()
        {
            m_processorCompletedEventArgs = new ProcessorCompletedEventArgs(m_RunResult);
            m_pendTimeMilliSeconds = Stopwatch.StartNew();
            if (Starting != null)
            {
                if (EstimateRemains != null)
                {
                    m_PrevTime = DateTime.Now;
                    m_PrevStatus = 0;
                }
                Starting(this, new EventArgs());
            }
        }

        /// <summary>
        /// [底层使用]当任务结束时，自动会触发
        /// </summary>
        protected virtual void OnCompleted()
        {
            m_saved.Clear();
            if (m_pendTimeMilliSeconds != null)
            {
                m_pendTimeMilliSeconds.Stop();
                TotalTimeByMilliseconds = m_pendTimeMilliSeconds.ElapsedMilliseconds;
            }

            m_processorCompletedEventArgs.StopExecuteNextProcessor = !m_RunResult.Success;
            if (Completed != null)
            {
                Completed(this, m_processorCompletedEventArgs);
            }
        }

        protected virtual void OnProgressChanged(int current, int count)
        {
            if (ProgressChanged != null)
            {
                m_NextTime = DateTime.Now;
                m_NextStatus = current;

                TimeSpan timeSpan = m_NextTime - m_PrevTime;

                double milli = timeSpan.TotalMilliseconds;
                int interval = m_NextStatus - m_PrevStatus;

                double rate = interval * 1.0 / milli;
                m_saved.Add(rate);

                int remains = count - m_NextStatus;

                double avgRate = m_saved.Average();
                if (avgRate == 0) avgRate = 1;

                int estimate = (int)(remains / avgRate);

                OnEstimateRemains(estimate);
                ProgressChanged(this, new ProgressChangedEventArgs(current, count));

                ClearCache();

                m_PrevTime = DateTime.Now;
                m_PrevStatus = m_NextStatus;
            }
        }

        protected virtual void OnEstimateRemains(int estimateRemainMilliseconds)
        {
            EstimateRemains?.Invoke(this, new EstimateEventArgs(estimateRemainMilliseconds));
        }

        protected virtual void OnExceptionOccured(Exception ex)
        {
            m_RunResult.Exceptions.Add(ex);
            if (ExceptionOccured != null)
            {
                ExceptionOccured(this, new ExceptionEventArgs(ex));
            }
            else
            {
                if (!m_RunAsThread)
                {
                    throw ex;
                }
            }
        }

        protected virtual void OnStatusChanged(string message)
        {
            OnStatusChanged(new StatusChangedEventArgs(message));
        }

        protected virtual void OnStatusChanged(StatusChangedEventArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }

        #endregion

        private void ClearCache()
        {
            if (m_saved.Count > _MaxSaved)
            {
                m_saved = m_saved.Skip(_MaxSaved).ToList();
            }
        }

        private DateTime m_PrevTime = DateTime.MinValue;
        private int m_PrevStatus;
        private int m_NextStatus;
        private DateTime m_NextTime = DateTime.MinValue;
        private IList<double> m_saved = new List<double>();



        //private Task m_MainTask;
        private IList<Task> m_TaskList = new List<Task>();
        private CancellationTokenSource cancelSource;

        /// <summary>
        /// 以异步方式运行
        /// </summary>
        public void StartAsync()
        {
            m_RunAsThread = true;
            cancelSource = new CancellationTokenSource();

            Task m_MainTask = Task.Factory.StartNew((cancelToken) =>
            {
                m_CancelToken = (CancellationToken)cancelToken;
                OnStarting();
                try
                {
                    _Start();
                }
                catch (Exception ex)
                {
                    OnExceptionOccured(ex);
                }

            }, cancelSource.Token).ContinueWith(task => OnCompleted()).ContinueWith(arg =>
            {
                if (!m_processorCompletedEventArgs.StopExecuteNextProcessor)
                {
                    m_TaskList.Add(m_TaskList[m_TaskList.Count - 1].ContinueWith(task =>
                     {
                         if (NextProcessor != null)
                             NextProcessor.StartAsync();
                     }));
                }
            });

            m_TaskList.Add(m_MainTask);
        }

        public void Start()
        {
            m_RunAsThread = false;
            Processor pro = this;
            do
            {
                pro.OnStarting();
                try
                {
                    pro._Start();
                }
                catch (Exception ex)
                {
                    pro.OnExceptionOccured(ex);
                }
                finally
                {
                    pro.OnCompleted();
                }
                if (pro.m_processorCompletedEventArgs.StopExecuteNextProcessor)
                    break;
                pro = pro.NextProcessor;
            }
            while (pro != null);
        }

        /// <summary>
        /// 停止异步任务
        /// </summary>
        public void Stop()
        {
            cancelSource?.Cancel();
            m_processorCompletedEventArgs.StopExecuteNextProcessor = true;
            _Stop();
        }

        protected virtual void _Stop()
        {

        }

        protected abstract void _Start();

        /// <summary>
        /// 当上一个处理器完成后，执行下一个处理器
        /// </summary>
        public Processor NextProcessor { get; set; }

        /// <summary>
        /// 获取或设置其它参数
        /// </summary>
        public object Parameter { get; set; }
    }
}
