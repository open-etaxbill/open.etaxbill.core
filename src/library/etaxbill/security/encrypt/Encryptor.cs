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
using System.Text;

namespace OdinSdk.eTaxBill.Security.Encrypt
{
    /// <summary>
    ///
    /// </summary>
    public class Encryptor : tDESCrypto
    {
        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//

        /// <summary>
        ///
        /// </summary>
        private Encryptor(byte[] PrivateKey, byte[] InitVector)
            : base(PrivateKey, InitVector)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//
        private readonly static Lazy<Encryptor> m_encryptor = new Lazy<Encryptor>(() =>
        {
            byte[] _initVector = OdinSdk.eTaxBill.Properties.Resources.initVector;
            byte[] _privateKey = OdinSdk.eTaxBill.Properties.Resources.privateKey;

            return new Encryptor(_privateKey, _initVector);
        });

        /// <summary></summary>
        public static Encryptor SNG
        {
            get
            {
                return m_encryptor.Value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 평문 Base64 문자열을 암호화된 Base64 문자열로 변환 합니다.
        /// </summary>
        /// <param name="plain_string"></param>
        /// <returns></returns>
        public string PlainBase64ToChiperBase64(string plain_string)
        {
            return Convert.ToBase64String(base.Encrypt(Convert.FromBase64String(plain_string)));
        }

        /// <summary>
        /// 암호화된 Base64 문자열을 평문 Base64 문자열로 변환 합니다.
        /// </summary>
        /// <param name="chiper_string"></param>
        /// <returns></returns>
        public string ChiperBase64ToPlainBase64(string chiper_string)
        {
            return Convert.ToBase64String(base.Decrypt(Convert.FromBase64String(chiper_string)));
        }

        /// <summary>
        /// 평문 바이트 배열을 암호화된 Base64 문자열로 변환 합니다.
        /// </summary>
        /// <param name="plain_bytes"></param>
        /// <returns></returns>
        public string PlainBytesToChiperBase64(byte[] plain_bytes)
        {
            return Convert.ToBase64String(base.Encrypt(plain_bytes));
        }

        /// <summary>
        /// 암호화된 Base64 문자열을 평문 바이트 배열로 변환 합니다.
        /// </summary>
        /// <param name="chiper_string"></param>
        /// <returns></returns>
        public byte[] ChiperBase64ToPlainBytes(string chiper_string)
        {
            return base.Decrypt(Convert.FromBase64String(chiper_string));
        }

        /// <summary>
        /// 평문 문자열을 암호화된 Base64 문자열로 변환 합니다.
        /// </summary>
        /// <param name="plain_string"></param>
        /// <returns></returns>
        public string PlainStringToChiperBase64(string plain_string)
        {
            return Convert.ToBase64String(base.Encrypt(Encoding.UTF8.GetBytes(plain_string)));
        }

        /// <summary>
        /// 암호화된 Base64 문자열을 평문 문자열로 변환 합니다.
        /// </summary>
        /// <param name="chiper_string"></param>
        /// <returns></returns>
        public string ChiperBase64ToPlainString(string chiper_string)
        {
            return Encoding.UTF8.GetString(base.Decrypt(Convert.FromBase64String(chiper_string)));
        }

        /// <summary>
        /// 평문 문자열을 암호화된 문자열로 변환 합니다.
        /// </summary>
        /// <param name="plain_string"></param>
        /// <returns></returns>
        public string PlainStringToChiperString(string plain_string)
        {
            return Encoding.UTF8.GetString(base.Encrypt(Encoding.UTF8.GetBytes(plain_string)));
        }

        /// <summary>
        /// 암호화된 문자열을 평문 문자열로 변환 합니다.
        /// </summary>
        /// <param name="chiper_string"></param>
        /// <returns></returns>
        public string ChiperStringToPlainString(string chiper_string)
        {
            return Encoding.UTF8.GetString(base.Decrypt(Encoding.UTF8.GetBytes(chiper_string)));
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//
    }
}