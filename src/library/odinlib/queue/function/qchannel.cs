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

namespace OdinSdk.OdinLib.Queue.Function
{
    //-----------------------------------------------------------------------------------------------------------------------------
    //
    //-----------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    ///
    /// </summary>
    public class QChannel : IDisposable
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        private QChannel()
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="queue_master"></param>
        public QChannel(QService queue_master)
        {
            m_qmaster = (QService)queue_master.Clone();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="queue_master"></param>
        /// <param name="queue_slave"></param>
        /// <param name="ip_address">IP주소</param>
        public QChannel(QService queue_master, QService queue_slave, string ip_address)
        {
            m_qmaster = (QService)queue_master.Clone();

            m_qslave = (QService)queue_slave.Clone();
            m_qslave.IpAddress = ip_address;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private QService m_qmaster = null;

        /// <summary>
        ///
        /// </summary>
        public QService QMaster
        {
            get
            {
                return m_qmaster;
            }
            set
            {
                m_qmaster = value;
            }
        }

        private QService m_qslave = null;

        /// <summary>
        ///
        /// </summary>
        public QService QSlave
        {
            get
            {
                return m_qslave;
            }
            set
            {
                m_qslave = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------

        #region Queue

        //-----------------------------------------------------------------------------------------------------------------------------
        private OdinSdk.OdinLib.Queue.Function.QReader m_qreader = null;

        /// <summary>
        ///
        /// </summary>
        public OdinSdk.OdinLib.Queue.Function.QReader QReader
        {
            get
            {
                if (m_qreader == null)
                    m_qreader = new OdinSdk.OdinLib.Queue.Function.QReader(QMaster);

                return m_qreader;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void QStart()
        {
            this.QStart(true);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        public void QStart(string args)
        {
            this.QStart(args, true);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="open_with_purge"></param>
        public void QStart(bool open_with_purge)
        {
            this.QStart(QSlave.IpAddress, open_with_purge);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        /// <param name="open_with_purge"></param>
        public void QStart(string args, bool open_with_purge)
        {
            QReader.QStart(QSlave, args, open_with_purge);
        }

        /// <summary>
        ///
        /// </summary>
        public void QStop()
        {
            QReader.QStop(QSlave);
        }

        /// <summary>
        ///
        /// </summary>
        public void QStopAll()
        {
            QReader.QStop();
        }

        /// <summary>
        ///
        /// </summary>
        public Guid Certkey
        {
            get
            {
                return QReader.Certkey(QSlave);
            }
        }

        #endregion Queue

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
                    if (m_qmaster != null)
                    {
                        m_qmaster.Dispose();
                        m_qmaster = null;
                    }
                    if (m_qslave != null)
                    {
                        m_qslave.Dispose();
                        m_qslave = null;
                    }
                    if (m_qreader != null)
                    {
                        m_qreader.Dispose();
                        m_qreader = null;
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
        ~QChannel()
        {
            Dispose(false);
        }

        #endregion IDisposable Members
    }
}