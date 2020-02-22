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

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace OdinSdk.OdinLib.Logging
{
    /// <summary>
    ///
    /// </summary>
    public class QFileWriter
    {
        private struct QueueFile
        {
            public string Directory;
            public string FileName;
            public byte[] Buffer;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private static readonly Lazy<System.Collections.Queue> m_syncQueue = new Lazy<System.Collections.Queue>(() =>
        {
            return System.Collections.Queue.Synchronized(new System.Collections.Queue());
        });

        private static System.Collections.Queue FileWriteQ
        {
            get
            {
                return m_syncQueue.Value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private void Parser()
        {
            while (true)
            {
                lock (FileWriteQ.SyncRoot)
                {
                    if (FileWriteQ.Count > 0)
                    {
                        object _dequeue = FileWriteQ.Dequeue();
                        if (_dequeue != null)
                        {
                            QueueFile _qdata = (QueueFile)_dequeue;
                            this.FileWrite(_qdata);
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

        private void FileWrite(QueueFile queue_data)
        {
            FileStream _fs = null;

            try
            {
                if (System.IO.Directory.Exists(queue_data.Directory) == false)
                    System.IO.Directory.CreateDirectory(queue_data.Directory);

                var _path = Path.Combine(queue_data.Directory, queue_data.FileName);
                File.WriteAllBytes(_path, queue_data.Buffer);
            }
            catch (Exception ex)
            {
                OEventLog.SNG.WriteEntry(ex.ToString(), EventLogEntryType.Error);
            }
            finally
            {
                if (_fs != null)
                    _fs.Close();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private static Thread QueueThread = null;

        /// <summary>
        /// 별도 thread에서 QueueFile을 이용해 file에 write 합니다.
        /// </summary>
        /// <param name="directory_name"></param>
        /// <param name="file_name"></param>
        /// <param name="out_buffer"></param>
        public void QueueWrite(string directory_name, string file_name, byte[] out_buffer)
        {
            lock (FileWriteQ.SyncRoot)
            {
                QueueFile _qdata = new QueueFile()
                {
                    Directory = directory_name,
                    FileName = file_name,
                    Buffer = out_buffer
                };

                FileWriteQ.Enqueue(_qdata);

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
    }
}