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

using Mono.Security;
using Mono.Security.Cryptography;
using OdinSdk.eTaxBill.Utility;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static Mono.Security.Cryptography.PKCS8;

namespace OdinSdk.eTaxBill.Security.Signature
{
    /// <summary>
    ///
    /// </summary>
    public class X509CertMgr
    {
        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// der 파일의 경우 생성자, 후에 SetPrivateKey 메서드 호출 하여 개인키를 획득하여야 함
        /// </summary>
        /// <param name="public_file"></param>
        /// <param name="private_file"></param>
        /// <param name="password"></param>
        public X509CertMgr(string public_file, string private_file, string password)
            : this(File.ReadAllBytes(public_file), File.ReadAllBytes(private_file), password)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="public_bytes"></param>
        /// <param name="private_bytes"></param>
        /// <param name="password"></param>
        public X509CertMgr(byte[] public_bytes, byte[] private_bytes, string password)
        {
            m_x509Cert2 = new X509Certificate2(public_bytes);

            if (X509Cert2.HasPrivateKey == false)
            {
                m_keyinfo = GetPrivateKey(private_bytes, password);
                X509Cert2.PrivateKey = PKCS8.PrivateKeyInfo.DecodeRSA(m_keyinfo.PrivateKey);
            }
        }

        /// <summary>
        /// pfx 파일의 경우 생성자
        /// </summary>
        /// <param name="pfx_file_name"></param>
        /// <param name="password"></param>
        public X509CertMgr(string pfx_file_name, string password)
        {
            m_x509Cert2 = new X509Certificate2(pfx_file_name, password);

            if (X509Cert2.HasPrivateKey == true)
            {
                m_keyinfo = new PKCS8.PrivateKeyInfo();
                m_keyinfo.PrivateKey = PKCS8.PrivateKeyInfo.Encode(m_x509Cert2.PrivateKey);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//
        private static object m_syncRoot = new object();

        //-------------------------------------------------------------------------------------------------------------------------//
        //  공인인증서 인스턴스
        //-------------------------------------------------------------------------------------------------------------------------//
        private X509Certificate2 m_x509Cert2 = null;

        /// <summary>
        ///
        /// </summary>
        public X509Certificate2 X509Cert2
        {
            get
            {
                if (m_x509Cert2 == null)
                    lock (m_syncRoot)
                    {
                        if (m_x509Cert2 == null)
                            m_x509Cert2 = new X509Certificate2();
                    }

                return m_x509Cert2;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //  공인인증서 개인키
        //-------------------------------------------------------------------------------------------------------------------------//
        private PKCS8.PrivateKeyInfo m_keyinfo = null;

        /// <summary>
        ///
        /// </summary>
        public PKCS8.PrivateKeyInfo KeyInfo
        {
            get
            {
                if (m_keyinfo == null)
                    lock (m_syncRoot)
                    {
                        if (m_keyinfo == null)
                            m_keyinfo = new PKCS8.PrivateKeyInfo();
                    }

                return m_keyinfo;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //  본인 확인을 위한 난수
        //-------------------------------------------------------------------------------------------------------------------------//
        private byte[] m_randomNum = null;

        /// <summary>
        ///
        /// </summary>
        public byte[] RandomNumber
        {
            get
            {
                if (m_randomNum == null)
                    lock (m_syncRoot)
                    {
                        if (m_randomNum == null)
                            m_randomNum = GetRandomNumber();
                    }

                return m_randomNum;
            }
        }

        private string m_hashName = null;

        /// <summary>
        ///
        /// </summary>
        public string HashName
        {
            get
            {
                if (m_hashName == null)
                    lock (m_syncRoot)
                    {
                        if (m_hashName == null)
                            m_hashName = GetHashName();
                    }

                return m_hashName;
            }
        }

        private byte[] m_virtualId = null;

        /// <summary>
        ///
        /// </summary>
        public byte[] VirtualId
        {
            get
            {
                if (m_virtualId == null)
                    lock (m_syncRoot)
                    {
                        if (m_virtualId == null)
                            m_virtualId = GetVirtualId();
                    }

                return m_virtualId;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//
        private byte[] RemoveLeadingZero(byte[] bigInt)
        {
            var start = 0;

            var length = bigInt.Length;
            if (bigInt[0] == 0x00)
            {
                start = 1;
                length--;
            }

            byte[] bi = new byte[length];
            Buffer.BlockCopy(bigInt, start, bi, 0, length);

            return bi;
        }

        private byte[] GetRandomNumber()
        {
            byte[] _randomNum;

            ASN1 _randomTop = (ASN1)KeyInfo.Attributes[0];
            if (_randomTop.Tag != 0x30 || _randomTop.Count < 2)
                throw new ProxyException("invalid random number");

            var _algorithm = ASN1Convert.ToOid(_randomTop[0]);
            if (_algorithm == "1.2.410.200004.10.1.1.3")
            {
                ASN1 _randomSeq = _randomTop[1];
                if (_randomSeq.Tag != 0x31 || _randomSeq.Count < 1)
                    throw new ProxyException("invalid random number");

                byte[] _rmvZero = RemoveLeadingZero(_randomSeq[0].Value);

                var _size = _rmvZero.Length;
                _randomNum = new byte[_size];

                Array.Copy(_rmvZero, 0, _randomNum, 0, _size);
            }
            else
            {
                throw new ProxyException("unknown random algorithm");
            }

            return _randomNum;
        }

        private ASN1 GetVidSequence()
        {
            ASN1 _subjectAltName = null;
            {
                foreach (X509Extension _ext in X509Cert2.Extensions)
                {
                    if (_ext.Oid.Value == "2.5.29.17")
                    {
                        _subjectAltName = new ASN1(_ext.RawData);
                        break;
                    }
                }

                if (_subjectAltName == null)
                    throw new ProxyException("can not find subject alter name extension");

                if (_subjectAltName.Tag != 0x30 || _subjectAltName.Count < 1)
                    throw new ProxyException("invalid subject alter name");
            }

            ASN1 _algorithmId = null;
            {
                for (int i = 0; i < _subjectAltName.Count; i++)
                {
                    if (_subjectAltName[i].Tag == 0xA0)
                    {
                        _algorithmId = _subjectAltName[i];
                        break;
                    }
                }

                if (_algorithmId == null)
                    throw new ProxyException("not exist algorithm id");

                if (_algorithmId.Tag != 0xA0 || _algorithmId.Count < 2)
                    throw new ProxyException("invalid virtual identity");

                ASN1 _algorithm = _algorithmId[0];
                if (_algorithm.Tag != 0x06)
                    throw new ProxyException("invalid algorithm tag");

                var _idNumber = ASN1Convert.ToOid(_algorithm);
                if (_idNumber != "1.2.410.200004.10.1.1")
                    throw new ProxyException("invalid vid algorithm");
            }

            _algorithmId = _algorithmId[1];
            {
                if (_algorithmId.Tag != 0xA0 || _algorithmId.Count < 1)
                    throw new ProxyException("invalid virtual identity");

                _algorithmId = _algorithmId[0];
                if (_algorithmId.Tag != 0x30 || _algorithmId.Count < 2)
                    throw new ProxyException("invalid virtual identity");

                //ASN1 _realName = _algorithmId[0];
            }

            _algorithmId = _algorithmId[1];
            {
                if (_algorithmId.Tag != 0x30 || _algorithmId.Count < 1)
                    throw new ProxyException("invalid virtual identity");

                _algorithmId = _algorithmId[0];
                if (_algorithmId.Tag != 0x30 || _algorithmId.Count < 2)
                    throw new ProxyException("invalid virtual identity");

                ASN1 _algorithm = _algorithmId[0];
                if (_algorithm.Tag != 0x06)
                    throw new ProxyException("invalid algorithm tag");

                var _idNumber = ASN1Convert.ToOid(_algorithm);
                if (_idNumber != "1.2.410.200004.10.1.1.1")
                    throw new ProxyException("invalid vid algorithm");
            }

            _algorithmId = _algorithmId[1];
            if (_algorithmId.Tag != 0x30 || _algorithmId.Count < 2)
                throw new ProxyException("invalid virtual identity");

            return _algorithmId;
        }

        private byte[] GetVirtualId()
        {
            byte[] _virtualId;

            ASN1 _algorithmId = GetVidSequence();
            {
                ASN1 _virtualSpec = _algorithmId[1];
                if (_virtualSpec.Tag != 0xA0 || _virtualSpec.Count < 1)
                    throw new ProxyException("not exist virtual id");

                var _size = _virtualSpec[0].Value.Length;

                _virtualId = new byte[_size];
                Array.Copy(RemoveLeadingZero(_virtualSpec[0].Value), 0, _virtualId, 0, _size);
            }

            return _virtualId;
        }

        private string GetHashName()
        {
            var _hashName = "";

            ASN1 _algorithmId = GetVidSequence();
            {
                ASN1 _hashAlgorithm = _algorithmId[0];
                if (_hashAlgorithm.Tag != 0x30 || _hashAlgorithm.Count < 1)
                    throw new ProxyException("not exist hash algorithm");

                ASN1 _algorithm = _hashAlgorithm[0];
                if (_algorithm.Tag != 0x06)
                    throw new ProxyException("invalid algorithm tag");

                var _idNumber = ASN1Convert.ToOid(_algorithm);
                if (_idNumber == "1.3.14.3.2.26")
                {
                    _hashName = "SHA1";
                }
                else if (_idNumber == "2.16.840.1.101.3.4.2.1")
                {
                    _hashName = "SHA256";
                }
                else
                {
                    throw new ProxyException("invalid hash name");
                }

                /*
                1.3.14.3.2.2 - md4WithRSA
                1.3.14.3.2.3 - md5WithRSA
                1.3.14.3.2.4 - md4WithRSAEncryption
                1.3.14.3.2.6 - desECB
                1.3.14.3.2.7 - desCBC
                1.3.14.3.2.8 - desOFB
                1.3.14.3.2.9 - desCFB
                1.3.14.3.2.10 - desMAC
                1.3.14.3.2.11 - rsaSignature
                1.3.14.3.2.12 - dsa
                1.3.14.3.2.13 - dsaWithSHA
                1.3.14.3.2.14 - mdc2WithRSASignature
                1.3.14.3.2.15 - shaWithRSASignature
                1.3.14.3.2.16 - dhWithCommonModulus
                1.3.14.3.2.17 - desEDE
                1.3.14.3.2.18 - sha
                1.3.14.3.2.19 - mdc-2
                1.3.14.3.2.20 - dsaCommon
                1.3.14.3.2.21 - dsaCommonWithSHA
                1.3.14.3.2.22 - rsaKeyTransport
                1.3.14.3.2.23 - keyed-hash-seal
                1.3.14.3.2.24 - md2WithRSASignature
                1.3.14.3.2.25 - md5WithRSASignature
                1.3.14.3.2.26 - SHA-1 hash algorithm
                1.3.14.3.2.27 - dsa With SHA1
                1.3.14.3.2.28 - dsa With SHA1 with Common Parameters
                1.3.14.3.2.29 - SHA1 with RSA signature
                2.16.840.1.101.3.4.2.1 - SHA-256 hash algorithm
                */
            }

            return _hashName;
        }

        private PKCS8.PrivateKeyInfo GetPrivateKey(byte[] raw_data, string password)
        {
            EncryptedPrivateKeyInfo _keyinfo = new EncryptedPrivateKeyInfo(raw_data);

            byte[] _byteKey = new byte[16];                     // SEED로 암호화된 개인키를 복호화 하기 위한 Key
            byte[] _byteIVt = new byte[16];                     // SEED로 암호화된 개인키를 복호화 하기 위한 초기 벡터

            //------------------------------------------------------------------------------------------------------------------------/
            // seedCBCWithSHA1: signPri.key 파일을 ReadBytes() 한 경우
            // Key Generation with SHA1 and Encryption with SEED CBC mode
            //------------------------------------------------------------------------------------------------------------------------/
            if (_keyinfo.Algorithm == "1.2.410.200004.1.15")
            {
                PasswordDeriveBytes _pbkdf1 = new PasswordDeriveBytes(password, _keyinfo.Salt, "SHA1", _keyinfo.IterationCount);
                byte[] _byteDKey = ((DeriveBytes)_pbkdf1).GetBytes(20);

                HashAlgorithm _sha1 = SHA1.Create();

                byte[] _byteTmp = new byte[4];                      // 초기 벡터를 구하기 위한 DK 값의 끝 4 바이트
                Array.Copy(_byteDKey, 16, _byteTmp, 0, 4);          // 초기 벡터는 DK 값의 뒷 4자리를 SHA1으로 해쉬했을 때 구할 수 있다.
                byte[] _byteDiv = _sha1.ComputeHash(_byteTmp);      // 초기 벡터를 구하기 위한 중간 값 --> DK의 끝 4바이트를 "SHA1"으로 해쉬한 값

                Array.Copy(_byteDiv, 0, _byteIVt, 0, 16);           // SHA1으로 해쉬한 값의 앞 16자리가 초기 벡터가 된다.
                Array.Copy(_byteDKey, 0, _byteKey, 0, 16);          // DK 값의 앞 16자리는 SEED를 복호화하기 위한 Key 값이다.
            }
            //------------------------------------------------------------------------------------------------------------------------/
            // seedCBC: PBES2 encryption scheme
            // SEED Encryption (CBC mode)
            //------------------------------------------------------------------------------------------------------------------------/
            else if (_keyinfo.Algorithm == "1.2.410.200004.1.4")
            {
                Rfc2898DeriveBytes _pbkdf2 = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(password), _keyinfo.Salt, _keyinfo.IterationCount);
                byte[] _byteDKey = _pbkdf2.GetBytes(20);

                Array.Copy(SeedCs.SNG.IV, 0, _byteIVt, 0, 16);      // IV 문자열 값은 "0123456789012345"
                Array.Copy(_byteDKey, 0, _byteKey, 0, 16);          // DK 값의 앞 16자리는 SEED를 복호화하기 위한 Key 값이다.
            }
            else
            {
                throw new ProxyException("undefined private data");
            }

            // SEED로 암호화 된 개인키를 복호화 한다. SEED는 국내표준, http://cnscenter.future.co.kr/std-algorithm/block.html 참조
            byte[] _decryptedKey = SeedCs.SNG.Decrypt(_keyinfo.EncryptedData, _byteKey, true, _byteIVt);

            // 복호화 된 개인키 바이트 배열을 이용하여 PKCS8.PrivateKeyInfo 객체를 생성한다.
            // 그리고, 이를 C#에서 전자서명 또는 암호화 할 수 있는 RSA 객체로 변환한다.
            return new PKCS8.PrivateKeyInfo(_decryptedKey);
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//

        /// <summary>
        ///
        /// </summary>
        /// <param name="idn"></param>
        /// <returns></returns>
        public bool VerifyVID(string idn)
        {
            // HashContent ::= SEQUENCE
            //          {
            //              idn       PrintableString,
            //              randomNum BIT STRING
            //          }

            Org.BouncyCastle.Asn1.DerSequence _hashContent
                = new Org.BouncyCastle.Asn1.DerSequence
                    (
                        new Org.BouncyCastle.Asn1.Asn1Encodable[]
                        {
                            new Org.BouncyCastle.Asn1.DerPrintableString(Encoding.Default.GetBytes(idn)),
                            new Org.BouncyCastle.Asn1.DerBitString(RandomNumber)
                        }
                    );

            // VID' = h(h(IDN, R))
            HashAlgorithm _hash = HashAlgorithm.Create(HashName);
            {
                byte[] _content = _hashContent.GetDerEncoded();

                _hash.Initialize();
                _hash.TransformBlock(_content, 0, _content.Length, _content, 0);
                _hash.TransformFinalBlock(new byte[0], 0, 0);

                byte[] _vid = _hash.Hash;
                _hash.Initialize();
                _hash.ComputeHash(_vid);
            }

            return Enumerable.SequenceEqual<byte>(_hash.Hash, VirtualId);
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------//
    //
    //-------------------------------------------------------------------------------------------------------------------------//
}