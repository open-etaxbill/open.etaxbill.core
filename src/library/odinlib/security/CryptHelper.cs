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
using System.Security.Cryptography;
using System.Text;

namespace OdinSdk.OdinLib.Security
{
    //-----------------------------------------------------------------------------------------------------------------------------
    //
    //-----------------------------------------------------------------------------------------------------------------------------
    /// <summary></summary>
    public class CryptHelper
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private static readonly Lazy<CryptHelper> m_lzyHelper = new Lazy<CryptHelper>(() =>
        {
            return new CryptHelper();
        });

        /// <summary>
        /// use default cryption-key
        /// </summary>
        public static CryptHelper SNG
        {
            get
            {
                return m_lzyHelper.Value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        private CryptHelper()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private static readonly Lazy<List<Cryption>> __Cryptors = new Lazy<List<Cryption>>(() =>
        {
            return new List<Cryption>();
        });

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private Cryption GetCryptor(string crypto_key = "")
        {
            crypto_key = Cryption.IsExistKey(crypto_key) == false ? Cryption.DefaultKey : crypto_key;

            Cryption _cryptor = __Cryptors.Value.Find
                (
                    delegate (Cryption cryptor)
                    {
                        return cryptor.SelectedKey == crypto_key;
                    }
                );

            if (_cryptor == null)
            {
                _cryptor = new OdinSdk.OdinLib.Security.Cryption(crypto_key);
                __Cryptors.Value.Add(_cryptor);
            }

            return _cryptor;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 평문을 암호화 합니다.
        /// </summary>
        /// <param name="plain_text"></param>
        /// <param name="cryptor_key"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public string PlainToChiper(string plain_text, string cryptor_key = "", Encoding encoding = null)
        {
            var _result = plain_text;

            if (String.IsNullOrEmpty(plain_text) == false)
                _result = GetCryptor(cryptor_key).PlainToChiper(plain_text, encoding);

            return _result;
        }

        /// <summary>
        /// 암호화된 내역을 복호화한다.
        /// </summary>
        /// <param name="chiper_text"></param>
        /// <param name="cryptor_key"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public string ChiperToPlain(string chiper_text, string cryptor_key = "", Encoding encoding = null)
        {
            var _result = chiper_text;

            if (String.IsNullOrEmpty(chiper_text) == false)
                _result = GetCryptor(cryptor_key).ChiperToPlain(chiper_text, encoding);

            return _result;
        }

        /// <summary>
        /// 평문을 암호화 합니다.
        /// </summary>
        /// <param name="plain_text"></param>
        /// <param name="is_compress"></param>
        /// <param name="cryptor_key"></param>
        /// <returns></returns>
        public string PlainToChiperText(string plain_text, bool is_compress = false, string cryptor_key = "")
        {
            var _result = plain_text;

            if (String.IsNullOrEmpty(plain_text) == false)
                _result = GetCryptor(cryptor_key).PlainToChiperText(plain_text, is_compress);

            return _result;
        }

        /// <summary>
        /// 암호화된 내역을 복호화한다.
        /// </summary>
        /// <param name="chiper_text"></param>
        /// <param name="is_compress"></param>
        /// <param name="cryptor_key"></param>
        /// <returns></returns>
        public string ChiperTextToPlain(string chiper_text, bool is_compress = false, string cryptor_key = "")
        {
            var _result = chiper_text;

            if (String.IsNullOrEmpty(chiper_text) == false)
                _result = GetCryptor(cryptor_key).ChiperTextToPlain(chiper_text, is_compress);

            return _result;
        }

        /// <summary>
        /// 평문을 암호화 합니다.
        /// </summary>
        /// <param name="object_value"></param>
        /// <param name="is_compress"></param>
        /// <param name="cryptor_key"></param>
        /// <returns></returns>
        public byte[] PlainToChiperBytes(object object_value, bool is_compress = false, string cryptor_key = "")
        {
            return GetCryptor(cryptor_key).PlainToChiperBytes(object_value, is_compress);
        }

        /// <summary>string cryptor_key = ""
        /// 암호화된 바이트열을 평문으로 변환 합니다.
        /// </summary>
        /// <param name="chiper_bytes"></param>
        /// <param name="is_compress"></param>
        /// <param name="cryptor_key"></param>
        /// <returns></returns>
        public object ChiperBytesToPlain(byte[] chiper_bytes, bool is_compress = false, string cryptor_key = "")
        {
            return GetCryptor(cryptor_key).ChiperBytesToPlain(chiper_bytes, is_compress);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string[] CreateNewCryptoKey()
        {
            string[] _result = new string[2];

            using (RijndaelManaged _rijndael = new RijndaelManaged())
            {
                _result[0] = Convert.ToBase64String(_rijndael.Key);
                _result[1] = Convert.ToBase64String(_rijndael.IV);
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}