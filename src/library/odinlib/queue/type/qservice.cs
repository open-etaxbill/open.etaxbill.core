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
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace OdinSdk.OdinLib.Queue
{
    /// <summary>
    /// QService에 대한 요약 설명입니다.
    /// </summary>
    [DataContract(Namespace = "http://www.odinsoftware.co.kr/sdk/queue/QService/2015/12")]
    [Serializable]
    public class QService : QAgency, ICloneable
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        [DataMember(Name = "CompanyId", Order = 0)]
        private string m_company_id;

        [DataMember(Name = "CorporateId", Order = 1)]
        private string m_corporate_id;

        [DataMember(Name = "Expire", Order = 2)]
        private DateTime m_expire = CUnixTime.UtcNow;

        [DataMember(Name = "Validate", Order = 3)]
        private bool m_validate = false;

        [DataMember(Name = "Singleton", Order = 4)]
        private bool m_singleton = false;

        [DataMember(Name = "InstallDay", Order = 5)]
        private DateTime m_installDay = DateTime.MinValue;

        [DataMember(Name = "NoUsers", Order = 6)]
        private int m_noUsers = 0;

        [DataMember(Name = "IsServer", Order = 7)]
        private bool m_isServer = false;

        [DataMember(Name = "IsLive", Order = 8)]
        private bool m_isLive = false;

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        public QService()
            : this("", "")
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="protocol">통신 프로토콜(tcp, http)</param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        public QService(string protocol, string category_id)
            : base(protocol, category_id)
        {
            InitializeClass();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="protocol">통신 프로토콜(tcp, http)</param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_name">제품명칭</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="product_version">제품의 버전</param>
        public QService(string protocol, string category_id, string product_name, string product_id, string product_version)
            : base(protocol, category_id, product_name, product_id, product_version)
        {
            InitializeClass();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="protocol">통신 프로토콜(tcp, http)</param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_name">제품명칭</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="product_version">제품의 버전</param>
        /// <param name="is_service"></param>
        public QService(string protocol, string category_id, string product_name, string product_id, string product_version, bool is_service)
            : base(protocol, category_id, product_name, product_id, product_version, is_service)
        {
            InitializeClass();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="protocol">통신 프로토콜(tcp, http)</param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_name">제품명칭</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="product_version">제품의 버전</param>
        /// <param name="queue_name"></param>
        public QService(string protocol, string category_id, string product_name, string product_id, string product_version, string queue_name)
            : base(protocol, category_id, product_name, product_id, product_version, queue_name)
        {
            InitializeClass();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="protocol">통신 프로토콜(tcp, http)</param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_name">제품명칭</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="product_version">제품의 버전</param>
        /// <param name="user_name"></param>
        /// <param name="user_id"></param>
        public QService(string protocol, string category_id, string product_name, string product_id, string product_version, string user_name, string user_id)
            : base(protocol, category_id, product_name, product_id, product_version, user_name, user_id)
        {
            InitializeClass();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="protocol">통신 프로토콜(tcp, http)</param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_name">제품명칭</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="product_version">제품의 버전</param>
        /// <param name="user_name"></param>
        /// <param name="user_id"></param>
        /// <param name="queue_name"></param>
        public QService(string protocol, string category_id, string product_name, string product_id, string product_version, string user_name, string user_id, string queue_name)
            : base(protocol, category_id, product_name, product_id, product_version, user_name, user_id, queue_name)
        {
            InitializeClass();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="agency"></param>
        public QService(QAgency agency)
        {
            Array.ForEach(agency.GetType().GetProperties(), _prop =>
            {
                if (typeof(QAgency).GetProperty(_prop.Name) != null)
                {
                    PropertyInfo _prop2 = agency.GetType().GetProperty(_prop.Name);
                    _prop2.SetValue(this, _prop.GetValue(agency, null), null);
                }
            });

            InitializeClass();
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        private void InitializeClass()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        public string CompanyId
        {
            get
            {
                return m_company_id;
            }
            set
            {
                m_company_id = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string CorporateId
        {
            get
            {
                return m_corporate_id;
            }
            set
            {
                m_corporate_id = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public DateTime Expire
        {
            get
            {
                return m_expire;
            }
            set
            {
                m_expire = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public bool Validate
        {
            get
            {
                return m_validate;
            }
            set
            {
                m_validate = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public bool Signleton
        {
            get
            {
                return m_singleton;
            }
            set
            {
                m_singleton = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public DateTime InstallDay
        {
            get
            {
                return m_installDay;
            }
            set
            {
                m_installDay = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public int NoUsers
        {
            get
            {
                return m_noUsers;
            }
            set
            {
                m_noUsers = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsServer
        {
            get
            {
                return m_isServer;
            }
            set
            {
                m_isServer = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsLive
        {
            get
            {
                return m_isLive;
            }
            set
            {
                m_isLive = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public new object Clone()
        {
            using (MemoryStream _stream = new MemoryStream())
            {
                BinaryFormatter _formatter = new BinaryFormatter();
                _formatter.Serialize(_stream, this);
                _stream.Position = 0;

                return _formatter.Deserialize(_stream);
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
        /// <param name="disposing">
        /// <see langword="true"/> if disposing; otherwise, <see langword="false"/>.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed == false)
            {
                if (disposing == true)
                {
                    // Dispose managed resources.
                }

                // Dispose unmanaged resources.

                // Note disposing has been done.
                IsDisposed = true;
            }

            // Call Dispose in the base class.
            base.Dispose(disposing);
        }

        #endregion IDisposable Members
    }
}