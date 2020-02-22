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
using System.Messaging;

namespace OdinSdk.OdinLib.Queue.Function
{
    /// <summary>
    /// 큐 구독자 관리
    /// </summary>
    public class Subscriber : IDisposable
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private string m_queue_name = "";

        /// <summary>
        /// 큐 명칭
        /// </summary>
        public string QueueName
        {
            get
            {
                return m_queue_name;
            }
            set
            {
                if (m_queue_name == value)
                    return;
                m_queue_name = value;
            }
        }

        private bool m_openWithPurge = true;

        /// <summary>
        /// 최초 open시 큐메시지를 제거 하는지 여부
        /// </summary>
        public bool OpenWithPurge
        {
            get
            {
                return m_openWithPurge;
            }
            set
            {
                if (m_openWithPurge == value)
                    return;
                m_openWithPurge = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="role_name"></param>
        /// <param name="nick_name"></param>
        /// <returns></returns>
        public static string GetQueueName(string role_name, string nick_name)
        {
            return String.Format("{0}_{1}", role_name, nick_name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="queue_name"></param>
        /// <returns></returns>
        public static string GetQueuePath(string queue_name)
        {
            return String.Format(@".\private$\{0}", queue_name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="nick_name"></param>
        /// <returns></returns>
        public static string GetStockerQueueName(string nick_name)
        {
            return GetQueueName("Stock", nick_name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="nick_name"></param>
        /// <returns></returns>
        public static string GetSenderQueueName(string nick_name)
        {
            return GetQueueName("Send", nick_name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="nick_name"></param>
        /// <returns></returns>
        public static string GetReaderQueueName(string nick_name)
        {
            return GetQueueName("Read", nick_name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="queue_path"></param>
        /// <returns></returns>
        public static MessageQueue CreateReaderQueue(string queue_path)
        {
            MessageQueue _result = null;

            if (Publisher.IsRemoteQueuePath(queue_path) == false && MessageQueue.Exists(queue_path) == false)
            {
                _result = MessageQueue.Create(queue_path, true);
                _result.SetPermissions("Everyone", MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow);
            }
            else
            {
                _result = new MessageQueue(queue_path);
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // Receiving
        //-----------------------------------------------------------------------------------------------------------------------------
        private MessageQueue m_serverQueue = null;

        /// <summary>
        ///
        /// </summary>
        /// <param name="queue_name"></param>
        /// <returns></returns>
        public MessageQueue OpenQueue(string queue_name)
        {
            MessageQueue _result = null;

            try
            {
                if (m_serverQueue == null)
                {
                    _result = CreateReaderQueue(GetQueuePath(queue_name));
                    if (OpenWithPurge == true)
                        _result.Purge();

                    // Enable the AppSpecific field in the messages.
                    _result.MessageReadPropertyFilter.AppSpecific = true;

                    // Set the formatter to binary.
                    //_queue.Formatter = new BinaryMessageFormatter();

                    // Set the formatter to Xml.
                    _result.Formatter = new XmlMessageFormatter(new Type[] { typeof(QMessage) });

                    _result.ReceiveCompleted += QReceiveCompleted;
                    _result.BeginReceive();

                    m_serverQueue = _result;
                }
                else
                {
                    _result = m_serverQueue;
                }
            }
            catch (MessageQueueException ex)
            {
                throw new Exception(ex.MessageQueueErrorCode.ToString(), ex);
            }
            catch (Exception)
            {
            }

            return _result;
        }

        private static object m_syncRoot = null;

        /// <summary>
        /// 액세스를 동기화하는 데 사용할 수 있는 개체를 가져옵니다.
        /// </summary>
        public object SyncRoot
        {
            get
            {
                if (m_syncRoot == null)
                    m_syncRoot = new object();

                return m_syncRoot;
            }
        }

        //public delegate void QReceiveCompletedEventHandler(object sender, ReceiveCompletedEventArgs e);

        /// <summary>
        ///
        /// </summary>
        protected static event EventHandler<ReceiveCompletedEventArgs> QSubscriberEvents;

        /// <summary>
        ///
        /// </summary>
        public static bool IsQEventEmpty
        {
            get
            {
                return QSubscriberEvents == null;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void RemoveAllEvents()
        {
            lock (SyncRoot)
            {
                //foreach (QReceiveCompletedEventHandler _eh in m_recvDelegates)
                //    m_recvCompletedEvents -= _eh;

                QSubscriberEvents = null;
                //m_recvDelegates.Clear();
            }
        }

        private void QReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            if (sender != null)
            {
                lock (SyncRoot)
                {
                    MessageQueue _msgQ = sender as MessageQueue;

                    try
                    {
                        using (System.Messaging.Message _message = _msgQ.EndReceive(e.AsyncResult))
                        {
                            // do skip messages
                        }

                        if (QSubscriberEvents != null)
                            QSubscriberEvents(this, e);
                    }
                    catch (MessageQueueException)
                    {
                        //throw new Exception(ex.MessageQueueErrorCode.ToString(), ex);
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        _msgQ.BeginReceive();
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        #region IDisposable Members

        /// <summary>
        ///
        /// </summary>
        private bool IsDisposed
        {
            get;
            set;
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> if disposing; otherwise, <see langword="false"/>.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed == false)
            {
                if (disposing == true)
                {
                    // Dispose managed resources.
                    if (m_serverQueue != null)
                    {
                        m_serverQueue.Dispose();
                        m_serverQueue = null;
                    }
                }

                // Dispose unmanaged resources.

                // Note disposing has been done.
                IsDisposed = true;
            }
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        ~Subscriber()
        {
            Dispose(false);
        }

        #endregion IDisposable Members

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}