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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace OdinSdk.OdinLib.Queue
{
    /// <summary>
    ///
    /// </summary>
    [DataContract(Namespace = "http://www.odinsoftware.co.kr/sdk/queue/QClient/2015/12")]
    [Serializable]
    public class QClient : QAgency, ICloneable
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        public QClient()
            : this("", "")
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="protocol">통신 프로토콜(tcp, http)</param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        public QClient(string protocol, string category_id)
            : base(protocol, category_id)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="protocol">통신 프로토콜(tcp, http)</param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_name">제품명칭</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="product_version">제품의 버전</param>
        public QClient(string protocol, string category_id, string product_name, string product_id, string product_version)
            : base(protocol, category_id, product_name, product_id, product_version)
        {
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
        public QClient(string protocol, string category_id, string product_name, string product_id, string product_version, string queue_name)
            : base(protocol, category_id, product_name, product_id, product_version, queue_name)
        {
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
        public QClient(string protocol, string category_id, string product_name, string product_id, string product_version, string user_name, string user_id)
            : base(protocol, category_id, product_name, product_id, product_version, user_name, user_id)
        {
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
        public QClient(string protocol, string category_id, string product_name, string product_id, string product_version, string user_name, string user_id, string queue_name)
            : base(protocol, category_id, product_name, product_id, product_version, user_name, user_id, queue_name)
        {
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
        public QClient(string protocol, string category_id, string product_name, string product_id, string product_version, bool is_service)
            : base(protocol, category_id, product_name, product_id, product_version, is_service)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="agency"></param>
        public QClient(QAgency agency)
        {
            Array.ForEach(agency.GetType().GetProperties(), _prop =>
            {
                if (typeof(QAgency).GetProperty(_prop.Name) != null)
                {
                    PropertyInfo _prop2 = agency.GetType().GetProperty(_prop.Name);
                    _prop2.SetValue(this, _prop.GetValue(agency, null), null);
                }
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        [DataMember(Name = "services", Order = 0)]
        private List<string> m_services = new List<string>();

        /// <summary>
        ///
        /// </summary>
        public List<string> Services
        {
            get
            {
                return m_services;
            }
            set
            {
                m_services = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "cVersion", Order = 1)]
        public string cVersion
        {
            get;
            set;
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