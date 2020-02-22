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

using System.Security.Cryptography;

namespace OdinSdk.eTaxBill.Security.Encrypt
{
    /// <summary>
    ///
    /// </summary>
    public class tDESCrypto
    {
        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//

        /// <summary>
        ///
        /// </summary>
        public tDESCrypto()
        {
            // TripleDESCryptoServiceProvider 는 기본이 CBC 모드이며, 자동으로 랜덤 대칭키와 초기벡터를 생성한다.
            // 따라서, 추가적인 설정없이 바로 암호화, 복호화하는데 사용하면 된다.
            m_tripleCryptor = new TripleDESCryptoServiceProvider();
            //tDes.Padding = PaddingMode.None;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        public tDESCrypto(byte[] key, byte[] iv)
        {
            m_tripleCryptor = new TripleDESCryptoServiceProvider
            {
                Key = key,
                IV = iv
            };
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//
        private readonly TripleDESCryptoServiceProvider m_tripleCryptor = null;

        /// <summary>
        ///
        /// </summary>
        public byte[] Key
        {
            get
            {
                return m_tripleCryptor.Key;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public byte[] IV
        {
            get
            {
                return m_tripleCryptor.IV;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 3-DES 알고리즘을 이용하여 주어진 데이터를 encrypt 한다.
        /// </summary>
        /// <param name="plain_data">원본 데이터</param>
        /// <returns></returns>
        public byte[] Encrypt(byte[] plain_data)
        {
            ICryptoTransform _icrypto = m_tripleCryptor.CreateEncryptor();
            return _icrypto.TransformFinalBlock(plain_data, 0, plain_data.Length);
        }

        /// <summary>
        /// 3-DES 알고리즘을 이용하여 주어진 데이터를 decrypt 한다.
        /// </summary>
        /// <param name="encrypted">암호화된 데이터</param>
        /// <returns></returns>
        public byte[] Decrypt(byte[] encrypted)
        {
            ICryptoTransform _icrypto = m_tripleCryptor.CreateDecryptor();
            return _icrypto.TransformFinalBlock(encrypted, 0, encrypted.Length);
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//
    }
}