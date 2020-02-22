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

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;
using System;
using System.Collections;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

#pragma warning disable CS8600

namespace OdinSdk.eTaxBill.Security.Encrypt
{
    /// <summary>
    ///
    /// </summary>
    public class CmsManager
    {
        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//

        /// <summary>
        ///
        /// </summary>
        private CmsManager()
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//
        private readonly static Lazy<CmsManager> m_cmsManager = new Lazy<CmsManager>(() =>
        {
            return new CmsManager();
        });

        /// <summary></summary>
        public static CmsManager SNG
        {
            get
            {
                return m_cmsManager.Value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// RFC 3852의 EnvelopedData 구조체 중 EncryptedContentInfo 항목을 생성한다.
        /// </summary>
        /// <param name="encrypted_content">암호화된 데이터</param>
        /// <param name="init_vector">암호화에 사용된 IV 값</param>
        /// <returns></returns>
        private EncryptedContentInfo GetEncryptedContentInfo(byte[] encrypted_content, byte[] init_vector)
        {
            // EncryptedContentInfo 에 포함된 데이터의 타입을 가리키는 OID
            // 표준전자세금계산서 개발지침(v1.0) 56페이지 참조
            DerObjectIdentifier _contentType = new DerObjectIdentifier("1.2.840.113549.1.7.1");

            // 실제 암호화에 이용된 대칭키 알고리즘 정보 설정
            // 암호화 알고리즘 : 3DES
            // 3DES의 OID : 1.2.840.113549.3.7
            Asn1OctetString _paramIV = new BerOctetString(init_vector);               // 암호화 알고리즘의 파라미터(= 암호화에 사용된 초기벡터 값) 설정
            AlgorithmIdentifier _contentEncryptionAlgorithm = new AlgorithmIdentifier(new DerObjectIdentifier("1.2.840.113549.3.7"), _paramIV);

            // 전자세금계산서 패키징 데이터를 암호화한 데이터 포함
            Asn1OctetString _encryptedContent = new BerOctetString(encrypted_content);

            // EncryptedContentInfo 구조체를 설정한다.
            return new EncryptedContentInfo(_contentType, _contentEncryptionAlgorithm, _encryptedContent);
        }

        /// <summary>
        /// RFC 3852의 EnvelopedData 구조체 중 KeyTransRecipientInfo 항목을 생성하고
        /// 이를 이용해 RecipientInfo 구조체를 생성한다.
        /// </summary>
        /// <param name="x509_certificate2">키를 암호화하기 위한 공인인증서(국세청 공인인증서)</param>
        /// <param name="random_key">암호화에 사용된 램덤 키</param>
        /// <returns></returns>
        private RecipientInfo GetKeyTransRecipientInfo(X509Certificate2 x509_certificate2, byte[] random_key)
        {
            // RecipientIdentifier 필드에는 누구의 공개키를 이용하였는지에 대한 정보가 들어간다.
            // IssuerAndSerialNumber(ASN.1 형태) 데이터를 생성하기 위하여 파라미터로 전달받은 cert 를 Org.BouncyCastle.X509.X509Certificate 타입으로 변환한다.
            X509CertificateParser _x509Parser = new X509CertificateParser();
            Org.BouncyCastle.X509.X509Certificate _bouncyCert = _x509Parser.ReadCertificate(x509_certificate2.GetRawCertData());

            // IssuerAndSerialNumber 데이터를 생성한다.
            Org.BouncyCastle.Asn1.Cms.IssuerAndSerialNumber _issuerAndSerial = new Org.BouncyCastle.Asn1.Cms.IssuerAndSerialNumber(_bouncyCert.IssuerDN, new DerInteger(_bouncyCert.SerialNumber));

            // IssuerAndSerialNumber 데이터를 이용하여 RecipientIdentifier 형태의 데이터를 생성한다.
            RecipientIdentifier _rid = new RecipientIdentifier(_issuerAndSerial.ToAsn1Object());

            // 대칭키 알고리즘에 사용된 키를 암호화할 때 이용되는 암호화 알고리즘에 대한 OID
            // 암호화 알고리즘 : RSA (비대칭 알고리즘)
            // OID : 1.2.840.113549.1.1.1
            AlgorithmIdentifier _keyEncryptionAlgorithm = new AlgorithmIdentifier(new DerObjectIdentifier("1.2.840.113549.1.1.1"));

            // 랜덤키를 공개키를 사용해 암호화 한다.
            RSACryptoServiceProvider _rsaCrypto = (RSACryptoServiceProvider)x509_certificate2.PublicKey.Key;
            byte[] _byteEncryptedKey = _rsaCrypto.Encrypt(random_key, false);                  // 대칭키를 암호화
            Asn1OctetString _encryptedKey = new BerOctetString(_byteEncryptedKey);

            // KeyTransRecipientInfo 구조체를 생성, 설정한다.
            KeyTransRecipientInfo _keyTransRecipientInfo = new KeyTransRecipientInfo(_rid, _keyEncryptionAlgorithm, _encryptedKey);

            // KeyTransRecipientInfo 구조체를 이용하여 RecipientInfo를 생성 및 설정한다.
            return new RecipientInfo(_keyTransRecipientInfo);
        }

        /// <summary>
        /// n개의 전자세금계산서를 ASN.1 형태의 TaxInvoicePackage로 변환 합니다.
        /// </summary>
        /// <param name="tax_invoices"></param>
        /// <returns></returns>
        private byte[] GetTaxInvoicePackage(ArrayList tax_invoices)
        {
            Asn1EncodableVector _asn1Vector = new Asn1EncodableVector();

            for (int i = 0; i < tax_invoices.Count; i++)
            {
                TaxInvoiceStruct _taxInvoiceStruct = (TaxInvoiceStruct)tax_invoices[i];

                DerOctetString _taxInvoce = new DerOctetString(_taxInvoiceStruct.TaxInvoice);
                DerOctetString _signerRvalue = new DerOctetString(_taxInvoiceStruct.SignerRValue);

                DerSequence _taxInvoiceData = new DerSequence(_signerRvalue, _taxInvoce);
                _asn1Vector.Add(_taxInvoiceData);
            }

            return
                (
                    new DerSequence
                    (
                        new DerInteger(tax_invoices.Count),
                        new DerSet(_asn1Vector)
                    )
                ).GetDerEncoded();
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// RFC 3852 CMS 에 정의된 ContentInfo 구조체를 생성한다.
        /// </summary>
        /// <param name="x509_certificate2">랜덤키를 암호화하기 위한 공인인증서(국세청 공인인증서)</param>
        /// <param name="plain_data">데이터</param>
        /// <returns></returns>
        public byte[] GetContentInfo(X509Certificate2 x509_certificate2, ArrayList plain_data)
        {
            tDESCrypto _cryptoService = new tDESCrypto();

            // RecipientInfo 구조체 생성 및 설정
            RecipientInfo _recipientInfo = this.GetKeyTransRecipientInfo(x509_certificate2, _cryptoService.Key);

            // EncryptedContentInfo 구조체 생성 및 설정
            byte[] _package = this.GetTaxInvoicePackage(plain_data);
            byte[] _encrypt = _cryptoService.Encrypt(_package);                       // 대칭키로 암호화
            EncryptedContentInfo _encryptedContentInfo = this.GetEncryptedContentInfo(_encrypt, _cryptoService.IV);

            // EnvelopedData 구조체 생성 및 설정
            Asn1Set _asn1Set = new DerSet(_recipientInfo);
            EnvelopedData _envelope = new EnvelopedData((OriginatorInfo)null, _asn1Set, _encryptedContentInfo, (Asn1Set)null);

            // RFC 3852의 구성 데이터인 SignedData, EnvelopedData, EncryptedData 등을 넣어주는 컨테이너인 ContentInfo 구조체를 생성 및 설정한다.
            // ContentInfo 구조체는 표준전자세금계산서 개발지침(v1.0)의 58페이지 참조
            Org.BouncyCastle.Asn1.Cms.ContentInfo _content = new Org.BouncyCastle.Asn1.Cms.ContentInfo(new DerObjectIdentifier("1.2.840.113549.1.7.3"), _envelope);
            return _content.GetEncoded();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x509_certificate2"></param>
        /// <param name="plain_data"></param>
        /// <returns></returns>
        public byte[] GetEncryptedContent(X509Certificate2 x509_certificate2, byte[] plain_data)
        {
            tDESCrypto _cryptoService = new tDESCrypto();

            // RecipientInfo 구조체 생성 및 설정
            RecipientInfo _recipientInfo = this.GetKeyTransRecipientInfo(x509_certificate2, _cryptoService.Key);

            // EncryptedContentInfo 구조체 생성 및 설정
            DerOctetString _taxInvoce = new DerOctetString(plain_data);
            byte[] _package = _taxInvoce.GetOctets();
            byte[] _encrypt = _cryptoService.Encrypt(_package);                       // 대칭키로 암호화
            EncryptedContentInfo _encryptedContentInfo = this.GetEncryptedContentInfo(_encrypt, _cryptoService.IV);

            // EnvelopedData 구조체 생성 및 설정
            Asn1Set _receipientInfos = new DerSet(_recipientInfo);
            EnvelopedData _envelopedData = new EnvelopedData((OriginatorInfo)null, _receipientInfos, _encryptedContentInfo, (Asn1Set)null);

            Org.BouncyCastle.Asn1.Cms.ContentInfo _content = new Org.BouncyCastle.Asn1.Cms.ContentInfo(new DerObjectIdentifier("1.2.840.113549.1.7.3"), _envelopedData);
            return _content.GetEncoded();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x509_certificate2"></param>
        /// <param name="encrypted_data"></param>
        /// <returns></returns>
        public byte[] GetDecryptedContent(X509Certificate2 x509_certificate2, byte[] encrypted_data)
        {
            Org.BouncyCastle.Asn1.Cms.ContentInfo _content = Org.BouncyCastle.Asn1.Cms.ContentInfo.GetInstance(Asn1Sequence.FromByteArray(encrypted_data));

            EnvelopedData _envelopedData = EnvelopedData.GetInstance(_content.Content);

            EncryptedContentInfo _encryptedContentInfo = _envelopedData.EncryptedContentInfo;
            byte[] _encrypt = _encryptedContentInfo.EncryptedContent.GetOctets();

            RecipientInfo _recipientInfo = RecipientInfo.GetInstance(_envelopedData.RecipientInfos[0]);
            KeyTransRecipientInfo _keyTransRecipientInfo = KeyTransRecipientInfo.GetInstance(_recipientInfo.Info);
            byte[] _byteEncryptedKey = _keyTransRecipientInfo.EncryptedKey.GetOctets();

            RSACryptoServiceProvider _rsaCrypto = (RSACryptoServiceProvider)x509_certificate2.PrivateKey;
            byte[] _randomKey = _rsaCrypto.Decrypt(_byteEncryptedKey, false);

            AlgorithmIdentifier _contentEncryptionAlgorithm = _encryptedContentInfo.ContentEncryptionAlgorithm;
            Asn1OctetString _paramIV = Asn1OctetString.GetInstance(_contentEncryptionAlgorithm.Parameters);
            byte[] _initVector = _paramIV.GetOctets();

            tDESCrypto _cryptoService = new tDESCrypto(_randomKey, _initVector);
            return _cryptoService.Decrypt(_encrypt);
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//
    }

    //-------------------------------------------------------------------------------------------------------------------------//
    //
    //-------------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    ///
    /// </summary>
    public struct TaxInvoiceStruct
    {
        /// <summary>
        ///
        /// </summary>
        public byte[] SignerRValue;

        /// <summary>
        ///
        /// </summary>
        public byte[] TaxInvoice;
    }

    //-------------------------------------------------------------------------------------------------------------------------//
    //
    //-------------------------------------------------------------------------------------------------------------------------//
}