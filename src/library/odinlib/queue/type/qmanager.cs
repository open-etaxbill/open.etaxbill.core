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
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Runtime.Serialization;

namespace OdinSdk.OdinLib.Queue
{
    /// <summary>
    ///
    /// </summary>
    [CollectionDataContract(Namespace = "http://www.odinsoftware.co.kr/svc/library/NameValueCollectionWrapper/2015/12")]
    [Serializable]
    public class NameValueCollectionWrapper : IEnumerable
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        public NameValueCollectionWrapper()
            : this(new NameValueCollection())
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="nvc"></param>
        public NameValueCollectionWrapper(NameValueCollection nvc)
        {
            InnerCollection = nvc;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        public NameValueCollection InnerCollection
        {
            get;
            private set;
        }

        /// <summary>
        ///
        /// </summary>
        public int Count
        {
            get
            {
                return InnerCollection.Count;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        public string this[int Index]
        {
            get
            {
                return this.InnerCollection[Index];
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string this[string name]
        {
            get
            {
                return this.InnerCollection[name];
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        public void Add(object value)
        {
            var nvString = value as string;
            if (nvString != null)
            {
                var nv = nvString.Split(',');
                InnerCollection.Add(nv[0], nv[1]);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(string name, string value)
        {
            InnerCollection.Add(name, value);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        public void Remove(string name)
        {
            InnerCollection.Remove(name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            foreach (string key in InnerCollection)
            {
                yield return String.Format("{0},{1}", key, InnerCollection[key]);
            }
        }
    }

    /// <summary>
    /// QManager에 대한 요약 설명입니다.
    /// </summary>
    [DataContract(Namespace = "http://www.odinsoftware.co.kr/sdk/library/QManager/2015/12")]
    [Serializable]
    public class QManager
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        [DataMember(Name = "CompanyId", Order = 0)]
        private string m_company_id = "";

        [DataMember(Name = "CorporateId", Order = 1)]
        private string m_corporate_id = "";

        [DataMember(Name = "CategoryId", Order = 2)]
        private string m_category_id = "";

        [DataMember(Name = "ProductId", Order = 3)]
        private string m_product_id = "";

        [DataMember(Name = "pVersion", Order = 4)]
        private string m_product_version = "";

        [DataMember(Name = "UserId", Order = 5)]
        private string m_user_id = "";

        [DataMember(Name = "IpAddresses", Order = 6)]
        private string[] m_ip_addresses = new string[8];

        [DataMember(Name = "Password", Order = 7)]
        private string m_password = "";

        [DataMember(Name = "Expire", Order = 8)]
        private DateTime m_expire = CUnixTime.UtcNow;

        [DataMember(Name = "Certkey", Order = 9)]
        private Guid m_certkey = Guid.Empty;

        [DataMember(Name = "Values", Order = 10)]
        private NameValueCollectionWrapper m_values = new NameValueCollectionWrapper();

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        public QManager()
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="company_id">모회사 ID</param>
        /// <param name="corporate_id">자회사 ID</param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="product_version">제품의 버전</param>
        /// <param name="ip_address">IP주소</param>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <param name="expire"></param>
        /// <param name="certification_key"></param>
        /// <param name="values"></param>
        public QManager(
            string company_id, string corporate_id, string category_id, string product_id, string product_version,
            string ip_address, string account, string password, DateTime expire, Guid certification_key, DataTable values
            )
            : this(company_id, corporate_id, category_id, product_id, product_version, new string[] { ip_address }, account, password, expire, certification_key, values)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="company_id">모회사 ID</param>
        /// <param name="corporate_id">자회사 ID</param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="product_version">제품의 버전</param>
        /// <param name="ip_addresses"></param>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <param name="expire"></param>
        /// <param name="certification_key"></param>
        /// <param name="values"></param>
        public QManager(
            string company_id, string corporate_id, string category_id, string product_id, string product_version,
            string[] ip_addresses, string account, string password, DateTime expire, Guid certification_key, DataTable values
            )
        {
            var i = 0;
            foreach (string _ip_address in ip_addresses)
            {
                m_ip_addresses[i++] = _ip_address;
                if (i >= m_ip_addresses.Length)
                    break;
            }

            CompanyId = company_id;
            CorporateId = corporate_id;

            CategoryId = category_id;
            ProductId = product_id;
            pVersion = product_version;

            UserId = account;
            Password = password;

            Expire = expire;
            Certkey = certification_key;

            foreach (DataColumn _dc in values.Columns)
                Values.Add(_dc.ColumnName, Convert.ToString(values.Rows[0][_dc.ColumnName]));
        }

        internal void Replace(Guid certification_key)
        {
            Certkey = certification_key;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// company identification
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
        /// corporate identification
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
        /// product identification
        /// </summary>
        public string CategoryId
        {
            get
            {
                return m_category_id;
            }
            set
            {
                m_category_id = value;
            }
        }

        /// <summary>
        /// product identification
        /// </summary>
        public string ProductId
        {
            get
            {
                return m_product_id;
            }
            set
            {
                m_product_id = value;
            }
        }

        /// <summary>
        /// product version
        /// </summary>
        public string pVersion
        {
            get
            {
                return m_product_version;
            }
            set
            {
                m_product_version = value;
            }
        }

        /// <summary>
        /// personal identification
        /// </summary>
        public string UserId
        {
            get
            {
                return m_user_id;
            }
            set
            {
                m_user_id = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string[] IpAddresses
        {
            get
            {
                return m_ip_addresses;
            }
            set
            {
                m_ip_addresses = value;
            }
        }

        /// <summary>
        /// password
        /// </summary>
        public string Password
        {
            get
            {
                return m_password;
            }
            set
            {
                m_password = value;
            }
        }

        /// <summary>
        /// expiration
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
        /// certification key
        /// </summary>
        public Guid Certkey
        {
            get
            {
                return m_certkey;
            }
            set
            {
                m_certkey = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public NameValueCollectionWrapper Values
        {
            get
            {
                return m_values;
            }
            set
            {
                m_values = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}