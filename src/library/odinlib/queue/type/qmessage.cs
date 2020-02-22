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
using OdinSdk.OdinLib.Security;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace OdinSdk.OdinLib.Queue
{
    /// <summary>
    ///
    /// </summary>
    [DataContract(Namespace = "http://www.odinsoftware.co.kr/sdk/queue/QMessage/2015/12")]
    [Serializable]
    public class QMessage : QAgency, ICloneable
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        public QMessage()
            : this("", "")
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="protocol">통신 프로토콜(tcp, http)</param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        public QMessage(string protocol, string category_id)
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
        /// <param name="command"></param>
        /// <param name="exception"></param>
        /// <param name="package"></param>
        public QMessage(string protocol, string category_id, string product_name, string product_id, string product_version, string command, XmlPackage package, string exception)
            : base(protocol, category_id, product_name, product_id, product_version)
        {
            this.Command = command;
            this.Exception = exception;
            this.Package = package;

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
        /// <param name="command"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public QMessage(string protocol, string category_id, string product_name, string product_id, string product_version, string command, string message, string exception)
            : this(protocol, category_id)
        {
            this.Command = command;
            this.Message = message;
            this.Exception = exception;

            InitializeClass();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="agency"></param>
        public QMessage(QAgency agency)
        {
            Array.ForEach(agency.GetType().GetProperties(), _prop =>
            {
                if (typeof(QAgency).GetProperty(_prop.Name) != null)
                {
                    var _prop2 = agency.GetType().GetProperty(_prop.Name);
                    _prop2.SetValue(this, _prop.GetValue(agency, null), null);
                }
            });

            this.InitializeClass();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="protocol">통신 프로토콜(tcp, http)</param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_name">제품명칭</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="product_version">제품의 버전</param>
        public QMessage(string protocol, string category_id, string product_name, string product_id, string product_version)
            : base(protocol, category_id, product_name, product_id, product_version)
        {
            this.InitializeClass();
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
        public QMessage(string protocol, string category_id, string product_name, string product_id, string product_version, bool is_service)
            : base(protocol, category_id, product_name, product_id, product_version, is_service)
        {
            this.InitializeClass();
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
        public QMessage(string protocol, string category_id, string product_name, string product_id, string product_version, string queue_name)
            : base(protocol, category_id, product_name, product_id, product_version, queue_name)
        {
            this.InitializeClass();
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
        public QMessage(string protocol, string category_id, string product_name, string product_id, string product_version, string user_name, string user_id)
            : base(protocol, category_id, product_name, product_id, product_version, user_name, user_id)
        {
            this.InitializeClass();
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private void InitializeClass()
        {
            this.Version = RegHelper.SDKFrameVersion;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "Command", Order = 0)]
        public string Command
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "Message", Order = 1)]
        public string Message
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "Exception", Order = 2)]
        public string Exception
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "Version", Order = 3)]
        public string Version
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "Package", Order = 4)]
        public XmlPackage Package
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        [DataMember(Name = "UsePackage", Order = 5)]
        public bool UsePackage
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