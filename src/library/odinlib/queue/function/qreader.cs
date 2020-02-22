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
using OdinSdk.OdinLib.Security;
using System;
using System.Collections;
using System.Messaging;

namespace OdinSdk.OdinLib.Queue.Function
{
    //-----------------------------------------------------------------------------------------------------------------------------
    //
    //-----------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    ///
    /// </summary>
    public class QReader : OdinSdk.OdinLib.Queue.Function.Subscriber
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="queue_master"></param>
        public QReader(QAgency queue_master)
        {
            m_qmaster = (QAgency)queue_master.Clone();
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private OdinSdk.OdinLib.Queue.QAgency m_qmaster = null;

        private OdinSdk.OdinLib.Queue.QAgency QMaster
        {
            get
            {
                return m_qmaster;
            }
        }

        private OdinSdk.OdinLib.Queue.Function.QWriter m_qwriter = null;

        private OdinSdk.OdinLib.Queue.Function.QWriter QWriter
        {
            get
            {
                if (m_qwriter == null)
                    m_qwriter = new OdinSdk.OdinLib.Queue.Function.QWriter();

                return m_qwriter;
            }
        }

        private static OdinSdk.OdinLib.Caching.Cache m_publishers = null;

        private OdinSdk.OdinLib.Caching.Cache Publishers
        {
            get
            {
                if (m_publishers == null)
                    m_publishers = new OdinSdk.OdinLib.Caching.Cache();

                return m_publishers;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private void Remove(QAgency queue_server, ReceiveCompletedEventArgs e)
        {
            var _queuePath = QWriter.QueuePath(queue_server);
            if (Publishers.Contains(_queuePath) == true)
                Publishers.Remove(_queuePath);

            if (e != null && QRemoveEvents != null)
                QRemoveEvents(this, e);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="queue_server"></param>
        /// <param name="e"></param>
        public void Insert(QAgency queue_server, ReceiveCompletedEventArgs e)
        {
            var _queuePath = QWriter.QueuePath(queue_server);
            Publishers.Add(_queuePath, queue_server);

            if (e != null && QInsertEvents != null)
                QInsertEvents(this, e);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="queue_server"></param>
        /// <returns></returns>
        public Guid Certkey(QAgency queue_server)
        {
            var _result = Guid.Empty;

            var _queuePath = QWriter.QueuePath(queue_server);
            if (Publishers.Contains(_queuePath) == true)
            {
                QAgency _agency = Publishers.GetData(_queuePath) as QAgency;
                _result = _agency.Certkey;
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        public static event EventHandler<ReceiveCompletedEventArgs> QReadEvents;

        /// <summary>
        ///
        /// </summary>
        public static event EventHandler<ReceiveCompletedEventArgs> QInsertEvents;

        /// <summary>
        ///
        /// </summary>
        public static event EventHandler<ReceiveCompletedEventArgs> QRemoveEvents;

        /// <summary>
        ///
        /// </summary>
        /// <param name="open_with_purge"></param>
        public void QStart(bool open_with_purge)
        {
            if (Subscriber.IsQEventEmpty == true)
                Subscriber.QSubscriberEvents += Subscriber_QRecveiveEvent;

            OpenWithPurge = open_with_purge;
            MessageQueue _messageQ = OpenQueue(QMaster.QueueName);
            if (_messageQ != null)
            {
                _messageQ.Formatter = new XmlMessageFormatter(new Type[] { typeof(QMessage) });
                //_mq.Formatter = new BinaryMessageFormatter();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="queue_server"></param>
        /// <param name="args"></param>
        /// <param name="open_with_purge"></param>
        public void QStart(QAgency queue_server, string args, bool open_with_purge)
        {
            QStart(open_with_purge);
            QWriter.SendSignIn(QMaster, queue_server, args);
        }

        /// <summary>
        ///
        /// </summary>
        public void QStop()
        {
            foreach (DictionaryEntry _q in Publishers.CurrentCacheState)
            {
                CacheItem _citem = _q.Value as CacheItem;

                QAgency _agency = _citem.Value as QAgency;
                QWriter.SendSignOut(QMaster, _agency, _agency.Certkey);

                //Remove(_agency);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="queue_server"></param>
        public void QStop(QAgency queue_server)
        {
            Guid _certkey = Certkey(queue_server);

            // I will service out using the certification key.
            if (_certkey != Guid.Empty)
                QWriter.SendSignOut(QMaster, queue_server, _certkey);
            else
                Remove(queue_server, null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private void Subscriber_QRecveiveEvent(object sender, ReceiveCompletedEventArgs e)
        {
            var _qmessage = e.Message.Body as QMessage;

            if (e.Message.Label == "CMD")         // command
            {
                var _product = _qmessage.ProductId;
                var _command = _qmessage.Command.ToLower();

                var _message = _qmessage.Message;
                if (_qmessage.UsePackage == true)
                    _message = Serialization.SNG.ReadPackage<string>(_qmessage.Package);

                if (_product != QMaster.ProductId)
                {
                    if (_command == "ping")
                    {
                        QWriter.SendPong(QMaster, _qmessage, _message);
                    }
                    else if (_command == "certkey")
                    {
                        _qmessage.Certkey = new Guid(_message);
                        Insert(_qmessage, e);
                    }
                    else if (_command == "remove")
                    {
                        Remove(_qmessage, e);
                    }
                    else if (_command == "reset")
                    {
                        Remove(_qmessage, e);

                        if (String.Compare(_qmessage.IssuedHost, QMaster.IssuedHost, true) == 0)
                            QWriter.SendSignIn(QMaster, _qmessage, _message);
                    }
                }
            }

            if (QReadEvents != null)
                QReadEvents(this, e);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}