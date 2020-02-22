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
using System.Runtime.Serialization;

namespace OdinSdk.OdinLib.Security
{
    /// <summary>
    /// 패킹 된 메시지 꾸러미
    /// </summary>
    [DataContract(Namespace = "http://www.odinsoftware.co.kr/sdk/security/XmlPackage/2015/12")]
    [Serializable]
    public class XmlPackage
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        public XmlPackage()
        {
            Packings = MKindOfPacking.None;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        public XmlPackage(object value)
            : this()
        {
            Value = value;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="crypto_key"></param>
        /// <param name="value"></param>
        public XmlPackage(string crypto_key, object value)
            : this(value)
        {
            CryptoKey = crypto_key;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="packings"></param>
        /// <param name="crypto_key"></param>
        /// <param name="value"></param>
        public XmlPackage(MKindOfPacking packings, string crypto_key, object value)
            : this(crypto_key, value)
        {
            Packings = packings;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 패킹 방법
        /// </summary>
        [DataMember(Name = "Packings", Order = 0)]
        public MKindOfPacking Packings
        {
            get;
            set;
        }

        /// <summary>
        /// 암호화 키 값
        /// </summary>
        [DataMember(Name = "CryptoKey", Order = 1)]
        public string CryptoKey
        {
            get;
            set;
        }

        /// <summary>
        /// 꾸러미
        /// </summary>
        [DataMember(Name = "Value", Order = 2)]
        public object Value
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 패킷을 설명하는 메시지
        /// 또는
        /// byte[]등 많은 양의 데이터를 ToString() 하지 않고 메시지를 전달 하는데 사용 됨.
        /// </summary>
        public override string ToString()
        {
            var _result = "";

            Type _type = Value.GetType();
            if (_type.IsSealed == true && _type.BaseType != typeof(Array))
                _result = Value.ToString();

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}