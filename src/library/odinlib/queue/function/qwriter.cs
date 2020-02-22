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

using OdinSdk.OdinLib.Caching;
using OdinSdk.OdinLib.Configuration;
using OdinSdk.OdinLib.Security;
using System;
using System.Collections;
using System.Messaging;

namespace OdinSdk.OdinLib.Queue.Function
{
    //-----------------------------------------------------------------------------------------------------------------------------
    //
    //-----------------------------------------------------------------------------------------------------------------------------
    /// <summary></summary>
    public class QWriter : OdinSdk.OdinLib.Queue.Function.Publisher
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        // subscriber must be static variable.
        //-----------------------------------------------------------------------------------------------------------------------------

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

        private static OdinSdk.OdinLib.Caching.Cache m_subscribers = null;

        /// <summary>
        ///
        /// </summary>
        public OdinSdk.OdinLib.Caching.Cache Subscribers
        {
            get
            {
                if (m_subscribers == null)
                    m_subscribers = new OdinSdk.OdinLib.Caching.Cache();

                return m_subscribers;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="agency"></param>
        /// <returns></returns>
        public string QueuePath(QAgency agency)
        {
            return GetRemoteQueuePath(agency.Protocol, agency.IpAddress, agency.QueueName);
        }

        /// <summary>
        ///
        /// </summary>
        public void Clear()
        {
            lock (SyncRoot)
            {
                Subscribers.Clear();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // ping test
        //-----------------------------------------------------------------------------------------------------------------------------

        private const int PingRetryMax = 3;

        /// <summary>
        ///
        /// </summary>
        /// <param name="agency"></param>
        public void InsertReader(QAgency agency)
        {
            Subscribers.Add(QueuePath(agency), agency);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="queue_owner"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public int DoPingTest(QAgency queue_owner, string args)
        {
            var _result = 0;

            foreach (var _q in Subscribers.CurrentCacheState)
            {
                CacheItem _citem = ((DictionaryEntry)_q).Value as CacheItem;

                var _agency = _citem.Value as QAgency;
                if (String.Compare(queue_owner.IssuedHost, _agency.IssuedHost, true) != 0)
                    continue;

                var _qpath = QueuePath(_agency);

                _agency.PingRetry++;

                if (_agency.PingRetry <= PingRetryMax)
                {
                    SendPing(queue_owner, _agency, args);
                    Subscribers.Add(_qpath, _agency);
                }
                else
                {
                    Subscribers.Remove(_qpath);
                    //SendRemove(queue_owner, _agency);
                }

                _result++;
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="client_id"></param>
        /// <returns></returns>
        public bool SetPingFlag(Guid client_id)
        {
            bool _result;

            var _agency = SelectAgency(client_id);
            if (_agency != null)
                _result = SetPingFlag(_agency);
            else
                _result = false;

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        public bool SetPingFlag(QAgency sender)
        {
            var _result = false;

            lock (SyncRoot)
            {
                var _qpath = QueuePath(sender);
                if (Subscribers.Contains(_qpath) == true)
                {
                    var _agency = Subscribers.GetData(_qpath) as QAgency;
                    _agency.PingRetry = 0;

                    Subscribers.Add(_qpath, _agency);

                    _result = true;
                }
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // Start & Stop
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="message_queue"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool WriteStorage(MessageQueue message_queue, object message)
        {
            var _result = false;

            //if (message_queue.Transactional == true)
            {
                using (MessageQueueTransaction _qTransaction = new MessageQueueTransaction())
                {
                    _qTransaction.Begin();

                    try
                    {
                        using (Message _message = new Message
                        {
                            Formatter = new XmlMessageFormatter(new Type[] { typeof(QMessage) }),
                            //Formatter = new BinaryMessageFormatter(),
                            Body = message,
                            Label = "store",
                            AppSpecific = 1,
                            Priority = MessagePriority.High
                        })
                        {
                            message_queue.Send(_message, _qTransaction);
                        }
                        _qTransaction.Commit();

                        _result = true;
                    }
                    catch (MessageQueueException ex)
                    {
                        _qTransaction.Abort();
                        throw new Exception(ex.MessageQueueErrorCode.ToString(), ex);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="queue_owner"></param>
        /// <returns></returns>
        public int QStop(QAgency queue_owner)
        {
            return QStop(queue_owner, queue_owner.HostName);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="queue_owner"></param>
        /// <param name="issued_host"></param>
        /// <returns></returns>
        public int QStop(QAgency queue_owner, string issued_host)
        {
            return QStop(queue_owner, typeof(QMessage), issued_host);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="queue_owner"></param>
        /// <param name="queue_type"></param>
        /// <returns></returns>
        public int QStop(QAgency queue_owner, Type queue_type)
        {
            return QStop(queue_owner, queue_type, queue_owner.HostName);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="queue_owner"></param>
        /// <param name="queue_type"></param>
        /// <param name="issued_host"></param>
        /// <returns></returns>
        public int QStop(QAgency queue_owner, Type queue_type, string issued_host)
        {
            var _result = 0;

            MessageQueue _pushQ = CreateServiceQueue(Subscriber.GetQueuePath(Subscriber.GetStockerQueueName(queue_owner.QueueName)));
            if (_pushQ != null)
            {
                _pushQ.Formatter = new XmlMessageFormatter(new Type[] { queue_type });

                Hashtable _readers = Subscribers.CurrentCacheState;
                foreach (DictionaryEntry _q in _readers)
                {
                    CacheItem _citem = _q.Value as CacheItem;

                    var _agency = _citem.Value as QAgency;
                    if (String.Compare(_agency.IssuedHost, issued_host, true) != 0)
                        continue;

                    if (SendRemove(queue_owner, _agency) == true)
                    {
                        if (WriteStorage(_pushQ, _agency) == true)
                            _result++;
                    }
                }

                _pushQ.Close();
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="queue_owner"></param>
        /// <returns></returns>
        public int QStart(QAgency queue_owner)
        {
            return QStart(queue_owner, queue_owner.HostName);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="queue_owner"></param>
        /// <param name="issued_host"></param>
        /// <returns></returns>
        public int QStart(QAgency queue_owner, string issued_host)
        {
            return QStart(queue_owner, typeof(QMessage), issued_host);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="queue_owner"></param>
        /// <param name="queue_type"></param>
        /// <returns></returns>
        public int QStart(QAgency queue_owner, Type queue_type)
        {
            return QStart(queue_owner, queue_type, queue_owner.HostName);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="queue_owner"></param>
        /// <param name="queue_type"></param>
        /// <param name="issued_host"></param>
        /// <returns></returns>
        public int QStart(QAgency queue_owner, Type queue_type, string issued_host)
        {
            var _result = 0;

            MessageQueue _popQ = CreateServiceQueue(Subscriber.GetQueuePath(Subscriber.GetStockerQueueName(queue_owner.QueueName)));
            if (_popQ != null)
            {
                try
                {
                    _popQ.Formatter = new XmlMessageFormatter(new Type[] { queue_type });

                    Message[] _messages = _popQ.GetAllMessages();
                    foreach (var _message in _messages)
                    {
                        var _agency = _message.Body as QAgency;
                        if (String.Compare(_agency.IssuedHost, issued_host, true) != 0)
                            continue;

                        if (SendReset(queue_owner, _agency) == true)
                            _result++;
                    }
                }
                catch (MessageQueueException ex)
                {
                    throw new Exception(ex.MessageQueueErrorCode.ToString(), ex);
                }
                catch (Exception)
                {
                }
                finally
                {
                    _popQ.Purge();
                }
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // selection
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="client_id"></param>
        /// <returns></returns>
        public QAgency SelectAgency(Guid client_id)
        {
            QAgency _result = null;

            foreach (DictionaryEntry _q in Subscribers.CurrentCacheState)
            {
                CacheItem _citem = _q.Value as CacheItem;

                var _agency = _citem.Value as QAgency;
                if (_agency.Certkey == client_id)
                {
                    _result = _agency;
                    break;
                }
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ip_address">IP주소</param>
        /// <returns></returns>
        public QAgency SelectAgencyByIpAddress(string ip_address)
        {
            QAgency _result = null;

            foreach (DictionaryEntry _q in Subscribers.CurrentCacheState)
            {
                CacheItem _citem = _q.Value as CacheItem;

                var _agency = _citem.Value as QAgency;
                if (_agency.IpAddress == ip_address)
                {
                    _result = _agency;
                    break;
                }
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public QAgency SelectAgencyByUserId(string user_id)
        {
            QAgency _result = null;

            foreach (DictionaryEntry _q in Subscribers.CurrentCacheState)
            {
                CacheItem _citem = _q.Value as CacheItem;

                var _agency = _citem.Value as QAgency;
                if (_agency.UserId == user_id)
                {
                    _result = _agency;
                    break;
                }
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="user_name"></param>
        /// <returns></returns>
        public QAgency SelectAgencyByUserName(string user_name)
        {
            QAgency _result = null;

            foreach (DictionaryEntry _q in Subscribers.CurrentCacheState)
            {
                CacheItem _citem = _q.Value as CacheItem;

                var _agency = _citem.Value as QAgency;
                if (_agency.UserName == user_name)
                {
                    _result = _agency;
                    break;
                }
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // packet
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        /// <param name="reader"></param>
        /// <param name="exception"></param>
        /// <param name="label"></param>
        /// <param name="app_secific"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public bool WritePacket(QMessage message, QAgency reader, string exception, string label, int app_secific, MessagePriority priority)
        {
            var _result = false;

            MessageQueue _messageQ = OpenQueue(QueuePath(reader));
            if (_messageQ != null)
            {
                _messageQ.Formatter = new XmlMessageFormatter(new Type[] { typeof(QMessage) });
                //_messageQ.Formatter = new BinaryMessageFormatter();

                message.Exception = exception;
                message.Connected = CUnixTime.UtcNow;

                _result = WriteQueue(_messageQ, message, label, app_secific, priority);
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reader"></param>
        /// <param name="exception"></param>
        /// <param name="command"></param>
        /// <param name="label"></param>
        /// <param name="package"></param>
        /// <param name="app_secific"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public bool WritePacket(
            QAgency sender, QAgency reader, string exception, string command, string label,
            XmlPackage package,
            int app_secific = 5, System.Messaging.MessagePriority priority = System.Messaging.MessagePriority.Normal
            )
        {
            using (QMessage _qmessage = new QMessage(sender)
            {
                Command = command,
                Package = package,
                UsePackage = false
            })
            {
                return WritePacket(_qmessage, reader, exception, label, app_secific, priority);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reader"></param>
        /// <param name="exception"></param>
        /// <param name="command"></param>
        /// <param name="label"></param>
        /// <param name="message"></param>
        /// <param name="kind_of_packing"></param>
        /// <param name="crypto_key"></param>
        /// <param name="app_secific"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public bool WritePacket(
            QAgency sender, QAgency reader, string exception, string command, string label,
            string message, MKindOfPacking kind_of_packing = MKindOfPacking.None, string crypto_key = "",
            int app_secific = 5, MessagePriority priority = MessagePriority.Normal
            )
        {
            var _result = false;

            if (String.IsNullOrEmpty(message) == false)
            {
                using (QMessage _qmessage = new QMessage(sender)
                {
                    Command = command,
                    Package = Serialization.SNG.WritePackage<string>(message, kind_of_packing, crypto_key),
                    UsePackage = true
                })
                {
                    _result = WritePacket(_qmessage, reader, exception, label, app_secific, priority);
                }
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // send to server
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 로그 기록 함수
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reader"></param>
        /// <param name="exception"></param>
        /// <param name="command"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool WriteLogging(QAgency sender, QAgency reader, string exception, string command, string message)
        {
            return WritePacket(sender, reader, exception, command, "LOG", message, MKindOfPacking.Compressed, "", 3, MessagePriority.Normal);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reader"></param>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool WriteLogging(QAgency sender, QAgency reader, string exception, string message)
        {
            return WriteLogging(sender, reader, exception, "write", message);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reader"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool WriteLogging(QAgency sender, QAgency reader, string message)
        {
            return WriteLogging(sender, reader, "L", message);
        }

        /// <summary>
        /// MSMQ를 통해 명령-메시지를 직접 전달 합니다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reader"></param>
        /// <param name="exception"></param>
        /// <param name="command"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool WriteCommand(QAgency sender, QAgency reader, string exception, string command, string message)
        {
            return WritePacket(sender, reader, exception, command, "CMD", message, MKindOfPacking.Encrypted, "", 1, MessagePriority.High);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reader"></param>
        /// <param name="command"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool WriteCommand(QAgency sender, QAgency reader, string command, string message)
        {
            return WriteCommand(sender, reader, "C", command, message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // common
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="agency"></param>
        /// <returns></returns>
        public bool SendCertKey(QAgency sender, QAgency agency)
        {
            return WriteCommand(sender, agency, "certkey", agency.Certkey.ToString());
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="agency"></param>
        /// <returns></returns>
        public bool AddAgency(QAgency sender, QAgency agency)
        {
            var _result = false;

            lock (SyncRoot)
            {
                var _qpath = QueuePath(agency);
                if (Subscribers.Contains(_qpath) == false)
                {
                    if (agency.Certkey == Guid.Empty)
                        agency.Certkey = Guid.NewGuid();

                    agency.IssuedHost = sender.HostName;
                    Subscribers.Add(_qpath, agency);

                    _result = true;
                }
                else
                {
                    agency = Subscribers.GetData(_qpath) as QAgency;
                }

                SendCertKey(sender, agency);
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public bool SendReset(QAgency sender, QAgency reader)
        {
            return WriteCommand(sender, reader, "reset", reader.Certkey.ToString());
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ip_address">IP주소</param>
        /// <returns></returns>
        public bool ResetAgency(QAgency sender, string ip_address)
        {
            var _result = false;

            var _agency = SelectAgencyByIpAddress(ip_address);
            if (_agency != null)
            {
                lock (SyncRoot)
                {
                    var _qpath = QueuePath(_agency);
                    if (Subscribers.Contains(_qpath) == true)
                    {
                        Subscribers.Remove(_qpath);
                        SendReset(sender, _agency);

                        _result = true;
                    }
                }
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public bool SendRemove(QAgency sender, QAgency reader)
        {
            return WriteCommand(sender, reader, "remove", reader.Certkey.ToString());
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="client_id"></param>
        /// <returns></returns>
        public bool RemoveAgency(QAgency sender, Guid client_id)
        {
            var _result = false;

            var _agency = SelectAgency(client_id);
            if (_agency != null)
            {
                lock (SyncRoot)
                {
                    var _qpath = QueuePath(_agency);
                    if (Subscribers.Contains(_qpath) == true)
                    {
                        Subscribers.Remove(_qpath);
                        SendRemove(sender, _agency);

                        _result = true;
                    }
                }
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reader"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool SendPing(QAgency sender, QAgency reader, string args)
        {
            return WriteCommand(sender, reader, "ping", args);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reader"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool SendPong(QAgency sender, QAgency reader, string args)
        {
            return WriteCommand(sender, reader, "pong", args);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reader"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool SendSignIn(QAgency sender, QAgency reader, string args)
        {
            return WriteCommand(sender, reader, "signin", args);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reader"></param>
        /// <param name="certification_key"></param>
        /// <returns></returns>
        public bool SendSignOut(QAgency sender, QAgency reader, Guid certification_key)
        {
            return WriteCommand(sender, reader, "signout", certification_key.ToString());
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}