/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.If not, see<http://www.gnu.org/licenses/>.
*/

using OdinSdk.OdinLib.Configuration;
using System;
using System.Threading;

namespace OdinSdk.OdinLib.Logging
{
    /// <summary>
    ///
    /// </summary>
    public class QFileLog : OFileLog
    {
        private struct QueueData
        {
            public string OrginServer;
            public DateTime LogTime;
            public string Direction;
            public string Message;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 큐로그 객체 생성자, 직접 directory를 지정 해주어야 합니다.
        /// </summary>
        public QFileLog()
            : base()
        {
        }

        /// <summary>
        /// 큐로그 객체 생성자
        /// </summary>
        /// <param name="directory_name">로그를 저장 할 폴더 위치</param>
        public QFileLog(string directory_name)
            : base(directory_name)
        {
        }

        /// <summary>
        /// 큐로그 객체 생성자
        /// </summary>
        /// <param name="directory_name">로그를 저장 할 폴더 위치</param>
        /// <param name="product_id">솔루션명</param>
        public QFileLog(string directory_name, string product_id)
            : base(directory_name, product_id)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private static readonly Lazy<System.Collections.Queue> m_fileQ = new Lazy<System.Collections.Queue>(() =>
        {
            return System.Collections.Queue.Synchronized(new System.Collections.Queue());
        });

        private static System.Collections.Queue FileLogQ
        {
            get
            {
                return m_fileQ.Value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private void Parser()
        {
            while (true)
            {
                lock (FileLogQ.SyncRoot)
                {
                    if (FileLogQ.Count > 0)
                    {
                        object _dequeue = FileLogQ.Dequeue();
                        if (_dequeue != null)
                        {
                            QueueData _q = (QueueData)_dequeue;
                            FileWrite(_q.OrginServer, _q.LogTime, _q.Direction, _q.Message);
                        }
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }

                Thread.Sleep(100);
            }
        }

        private static Thread QueueThread = null;

        private void QueueWrite(string org_server, DateTime log_write_time, string direction, string message)
        {
            lock (FileLogQ.SyncRoot)
            {
                QueueData _data = new QueueData()
                {
                    OrginServer = org_server,
                    LogTime = log_write_time,
                    Direction = direction,
                    Message = message
                };

                FileLogQ.Enqueue(_data);

                if (QueueThread == null || QueueThread.IsAlive == false)
                {
                    if (QueueThread != null)
                        QueueThread.Abort();

                    QueueThread = new Thread(Parser)
                    {
                        IsBackground = true
                    };

                    QueueThread.Start();
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        public void WriteLog(string message)
        {
            WriteLog(CfgHelper.SNG.MachineName, message);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="org_server"></param>
        /// <param name="message"></param>
        public override void WriteLog(string org_server, string message)
        {
            WriteLog(org_server, "L", message);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="org_server"></param>
        /// <param name="direction"></param>
        /// <param name="message"></param>
        public override void WriteLog(string org_server, string direction, string message)
        {
            WriteLog(org_server, CUnixTime.UtcNow, direction, message);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="org_server"></param>
        /// <param name="log_write_time"></param>
        /// <param name="direction"></param>
        /// <param name="message"></param>
        public override void WriteLog(string org_server, DateTime log_write_time, string direction, string message)
        {
            QueueWrite(org_server, log_write_time, direction, message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}