using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSystem.Processors
{
    public class Result
    {
        private bool? m_Success;
        /// <summary>
        /// 是否成功的执行完成
        /// </summary>
        public bool Success
        {
            get
            {
                if (m_Success.HasValue) return m_Success.Value;
                return Exceptions.Count == 0;
            }
            internal set { m_Success = value; }
        }

        public IList<Exception> Exceptions = new List<Exception>();
    }
}
