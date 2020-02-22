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

using OdinSdk.eTaxBill.Security.Issue;
using OdinSdk.eTaxBill.Utility;
using System;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace OdinSdk.eTaxBill.Security.Notice
{
    /// <summary>
    ///
    /// </summary>
    public class Packing
    {
        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//

        /// <summary>
        ///
        /// </summary>
        private Packing()
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//
        private static volatile Packing m_instance = null;

        private readonly static object m_syncRoot = new object();

        /// <summary>
        ///
        /// </summary>
        public static Packing SNG
        {
            get
            {
                if (m_instance == null)
                {
                    lock (m_syncRoot)
                    {
                        if (m_instance == null)
                        {
                            m_instance = new Packing();
                        }
                    }
                }

                return m_instance;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//
        private NameValueCollection m_nameCollections = null;

        /// <summary>
        ///
        /// </summary>
        public NameValueCollection SoapNameCollections
        {
            get
            {
                if (m_nameCollections == null)
                {
                    lock (m_syncRoot)
                    {
                        if (m_nameCollections == null)
                        {
                            m_nameCollections = new NameValueCollection();

                            m_nameCollections.Add("SOAP", "http://schemas.xmlsoap.org/soap/envelope/");
                            m_nameCollections.Add("ds", "http://www.w3.org/2000/09/xmldsig#");
                            m_nameCollections.Add("kec", "http://www.kec.or.kr/standard/Tax/");
                            m_nameCollections.Add("wsa", "http://www.w3.org/2005/08/addressing");
                            m_nameCollections.Add("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
                            m_nameCollections.Add("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
                            m_nameCollections.Add("xsd", "http://www.w3.org/2001/XMLSchema");
                            m_nameCollections.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                        }
                    }
                }

                return m_nameCollections;
            }
        }

        private XmlNamespaceManager m_nameSpaces = null;

        /// <summary>
        ///
        /// </summary>
        public XmlNamespaceManager SoapNamespaces
        {
            get
            {
                if (m_nameSpaces == null)
                {
                    lock (m_syncRoot)
                    {
                        if (m_nameSpaces == null)
                        {
                            m_nameSpaces = new XmlNamespaceManager(new NameTable());

                            foreach (string _key in SoapNameCollections.AllKeys)
                                m_nameSpaces.AddNamespace(_key, SoapNameCollections[_key]);
                        }
                    }
                }

                return m_nameSpaces;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private XmlDocument m_xmldoc = null;

        /// <summary>
        ///
        /// </summary>
        public XmlDocument SoapDoc
        {
            get
            {
                if (m_xmldoc == null)
                {
                    lock (m_syncRoot)
                    {
                        if (m_xmldoc == null)
                            m_xmldoc = new XmlDocument(SoapNamespaces.NameTable);
                    }
                }

                return m_xmldoc;
            }
        }

        private XmlElement CreateElement(string qualified_name)
        {
            XmlElement _result = null;

            string[] _names = qualified_name.Split(':');
            {
                if (_names.Length > 1)
                    _result = CreateElement(_names[0], _names[1]);
                else
                    _result = SoapDoc.CreateElement(qualified_name, SoapNamespaces.DefaultNamespace);
            }

            return _result;
        }

        private XmlElement CreateElement(string prefix, string local_name)
        {
            string _nsUri;

            if (SoapNamespaces.HasNamespace(prefix) == true)
                _nsUri = SoapNamespaces.LookupNamespace(prefix);
            else
                _nsUri = SoapNamespaces.DefaultNamespace;

            return SoapDoc.CreateElement(prefix, local_name, _nsUri);
        }

        private void InsertElement(XmlElement element, string qualified_name, string field_value)
        {
            if (String.IsNullOrEmpty(field_value) == false)
            {
                XmlElement _xel = CreateElement(qualified_name);
                _xel.InnerText = field_value;

                element.AppendChild(_xel);
            }
        }

        private void DateTimeElement(XmlElement element, string qualified_name, DateTime value, string format)
        {
            var _fieldValue = value.ToString(format);
            InsertElement(element, qualified_name, _fieldValue);
        }

        private XmlAttribute CreateAttribute(string name)
        {
            XmlAttribute _result = SoapDoc.CreateAttribute(name);

            string[] _names = name.Split(':');
            if (_names.Length > 1)
            {
                var _nsUri = SoapNamespaces.LookupNamespace(_names[0]);
                _result = SoapDoc.CreateAttribute(_names[0], _names[1], _nsUri);
            }

            return _result;
        }

        private bool RemoveElement(XmlElement source_xml, string node_name)
        {
            var _result = false;

            XPathNavigator _naviogator = source_xml.CreateNavigator();
            {
                _naviogator.MoveToFirstChild();

                XPathNavigator _target = SearchTargetNode(_naviogator, node_name);
                if (_target != null)
                {
                    _target.DeleteSelf();
                    _result = true;
                }
            }

            return _result;
        }

        private XPathNavigator SearchTargetNode(XPathNavigator navigator, string target_name)
        {
            XPathNavigator _result = null;

            while (true)
            {
                if (navigator.Name == target_name)
                {
                    _result = navigator;
                    break;
                }

                if (navigator.HasChildren == true)
                {
                    if (navigator.MoveToChild(XPathNodeType.Element) == true)
                    {
                        _result = SearchTargetNode(navigator, target_name);
                        if (_result != null)
                            break;
                    }
                }

                if (navigator.MoveToNext(XPathNodeType.Element) == true)
                {
                    _result = SearchTargetNode(navigator, target_name);
                    if (_result != null)
                        break;
                }
                else if (navigator.MoveToParent() == true)
                {
                    if (navigator.MoveToNext(XPathNodeType.Element) == true)
                    {
                        _result = SearchTargetNode(navigator, target_name);
                        if (_result != null)
                            break;
                    }
                }

                break;
            }

            return _result;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="time_stamp"></param>
        /// <returns></returns>
        public string GetMessageId(DateTime time_stamp)
        {
            var _messageId = new StringBuilder();

            _messageId.Append(time_stamp.ToString("yyyyMMddHHmmssfff"));
            _messageId.Append("-");
            _messageId.Append(Convertor.SNG.GetRandHexString(32));

            return _messageId.ToString();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="time_stamp"></param>
        /// <param name="kec_reg_id"></param>
        /// <returns></returns>
        public string GetSubmitID(DateTime time_stamp, string kec_reg_id)
        {
            // 국세청 등록번호(8자리) + '-' + 년월일(yyyyMMdd) + '-' + 32byte random number(16진수 문자열로 변환)

            var _messageId = new StringBuilder();

            _messageId.Append(kec_reg_id);
            _messageId.Append('-');
            _messageId.Append(time_stamp.ToString("yyyyMMdd"));
            _messageId.Append('-');
            _messageId.Append(Convertor.SNG.GetRandHexString(32));

            return _messageId.ToString();
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // <SOAP:Header>
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public XmlElement GetSoapSecurity(Header header, Body body)
        {
            XmlElement _security = CreateElement("wsse:Security");
            {
                XmlElement _binarySecurityToken = CreateElement("wsse:BinarySecurityToken");
                {
                    XmlAttribute _encType = CreateAttribute("EncodingType");
                    {
                        _encType.Value = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary";
                        _binarySecurityToken.Attributes.Append(_encType);
                    }

                    XmlAttribute _valType = CreateAttribute("ValueType");
                    {
                        _valType.Value = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3";
                        _binarySecurityToken.Attributes.Append(_valType);
                    }

                    XmlAttribute _509Token = CreateAttribute("wsu:Id");
                    {
                        _509Token.Value = "X509Token";
                        _binarySecurityToken.Attributes.Append(_509Token);
                    }

                    byte[] _x509data = header.X509Cert2.GetRawCertData();
                    _binarySecurityToken.InnerText = Convert.ToBase64String(_x509data);

                    _security.AppendChild(_binarySecurityToken);
                }

                XmlElement _signature = GetSignature(header, body);
                _security.AppendChild(_signature);
            }

            return _security;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public XmlElement GetSignature(Header header, Body body)
        {
            XmlElement _signature = CreateElement("ds:Signature");
            {
                XmlElement _signedInfo = GetSignedInfo(header, body);
                _signature.AppendChild(_signedInfo);

                XmlElement _signatureValue = CreateElement("ds:SignatureValue");
                {
                    if (header.SignatureValue != null)
                        _signatureValue.InnerText = Convert.ToBase64String(header.SignatureValue);

                    _signature.AppendChild(_signatureValue);
                }

                XmlElement _keyInfo = CreateElement("ds:KeyInfo");
                {
                    XmlElement _securityToken = CreateElement("wsse:SecurityTokenReference");
                    {
                        XmlElement _reference = CreateElement("wsse:Reference");
                        {
                            XmlAttribute _uri = CreateAttribute("URI");
                            {
                                _uri.Value = "#X509Token";
                                _reference.Attributes.Append(_uri);
                            }

                            _securityToken.AppendChild(_reference);
                        }

                        _keyInfo.AppendChild(_securityToken);
                    }

                    _signature.AppendChild(_keyInfo);
                }
            }

            return _signature;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public XmlElement GetSignedInfo(Header header, Body body)
        {
            XmlElement _signedInfo = CreateElement("ds:SignedInfo");
            {
                XmlElement _canonicalizationMethod = CreateElement("ds:CanonicalizationMethod");
                {
                    XmlAttribute _algorithm = CreateAttribute("Algorithm");
                    {
                        _algorithm.Value = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
                        _canonicalizationMethod.Attributes.Append(_algorithm);
                    }

                    _signedInfo.AppendChild(_canonicalizationMethod);
                }

                XmlElement _signatureMethod = CreateElement("ds:SignatureMethod");
                {
                    XmlAttribute _algorithm = CreateAttribute("Algorithm");
                    {
                        if (header.X509Cert2.PrivateKey.KeySize == 2048)
                            _algorithm.Value = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
                        else
                            _algorithm.Value = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";

                        _signatureMethod.Attributes.Append(_algorithm);
                    }

                    _signedInfo.AppendChild(_signatureMethod);
                }

                XmlElement _1stReference = CreateElement("ds:Reference");
                {
                    XmlAttribute _uri = CreateAttribute("URI");
                    {
                        _uri.Value = "";
                        _1stReference.Attributes.Append(_uri);
                    }

                    XmlElement _transforms = CreateElement("ds:Transforms");
                    {
                        XmlElement _transform1 = CreateElement("ds:Transform");
                        {
                            XmlAttribute _algorithm = CreateAttribute("Algorithm");
                            {
                                _algorithm.Value = "http://www.w3.org/2000/09/xmldsig#enveloped-signature";
                                _transform1.Attributes.Append(_algorithm);
                            }

                            _transforms.AppendChild(_transform1);
                        }

                        XmlElement _transform2 = CreateElement("ds:Transform");
                        {
                            XmlAttribute _algorithm = CreateAttribute("Algorithm");
                            {
                                _algorithm.Value = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
                                _transform2.Attributes.Append(_algorithm);
                            }

                            _transforms.AppendChild(_transform2);
                        }

                        _1stReference.AppendChild(_transforms);
                    }

                    XmlElement _digestMethod = CreateElement("ds:DigestMethod");
                    {
                        XmlAttribute _algorithm = CreateAttribute("Algorithm");
                        {
                            if (header.X509Cert2.PrivateKey.KeySize == 2048)
                                _algorithm.Value = "http://www.w3.org/2001/04/xmlenc#sha256";
                            else
                                _algorithm.Value = "http://www.w3.org/2000/09/xmldsig#sha1";

                            _digestMethod.Attributes.Append(_algorithm);
                        }

                        _1stReference.AppendChild(_digestMethod);
                    }

                    XmlElement _digestValue = CreateElement("ds:DigestValue");
                    {
                        if (header.DigestValue1 != null)
                            _digestValue.InnerText = Convert.ToBase64String(header.DigestValue1);

                        _1stReference.AppendChild(_digestValue);
                    }

                    _signedInfo.AppendChild(_1stReference);
                }

                if (header.Action == Request.eTaxInvoiceSubmit)
                {
                    XmlElement _2ndReference = CreateElement("ds:Reference");

                    XmlAttribute _uri = CreateAttribute("URI");
                    {
                        _uri.Value = "cid:" + body.ReferenceID;
                        _2ndReference.Attributes.Append(_uri);
                    }

                    XmlElement _transforms = CreateElement("ds:Transforms");
                    {
                        XmlElement _transform1 = CreateElement("ds:Transform");
                        {
                            XmlAttribute _algorithm = CreateAttribute("Algorithm");
                            {
                                _algorithm.Value = "http://docs.oasis-open.org/wss/oasis-wss-SwAProfile-1.1#Attachment-Content-Signature-Transform";
                                _transform1.Attributes.Append(_algorithm);
                            }

                            _transforms.AppendChild(_transform1);
                        }

                        _2ndReference.AppendChild(_transforms);
                    }

                    XmlElement _digestMethod = CreateElement("ds:DigestMethod");
                    {
                        XmlAttribute _algorithm = CreateAttribute("Algorithm");
                        {
                            if (header.X509Cert2.PrivateKey.KeySize == 2048)
                                _algorithm.Value = "http://www.w3.org/2001/04/xmlenc#sha256";
                            else
                                _algorithm.Value = "http://www.w3.org/2000/09/xmldsig#sha1";

                            _digestMethod.Attributes.Append(_algorithm);
                        }

                        _2ndReference.AppendChild(_digestMethod);
                    }

                    XmlElement _digestValue = CreateElement("ds:DigestValue");
                    {
                        _digestValue.InnerText = Convert.ToBase64String(header.DigestValue2);
                        _2ndReference.AppendChild(_digestValue);
                    }

                    _signedInfo.AppendChild(_2ndReference);
                }
            }

            return _signedInfo;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // <SOAP:Header>
        //-------------------------------------------------------------------------------------------------------------------------
        private XmlElement GetSoapHeader(Header header, Body body)
        {
            XmlElement _soapHeader = CreateElement("SOAP:Header");
            {
                if (header.Action != Request.eTaxRequestCertSubmit)
                {
                    // MessageId 엘리먼트 작성
                    InsertElement(_soapHeader, "wsa:MessageID", header.MessageId);
                    //InsertElement(_soapHeader, "wsa:MessageID", GetMessageId(header.TimeStamp));

                    // RelatesTo 엘리먼트 작성 (이전 요청 메시지의 MessageID 값을 설정)
                    // RelatesTo 엘리먼트는 요청에 대한 응답 메시지인 경우에만 존재
                    if (header.Action == Request.eTaxRequestRecvAck || header.Action == Request.eTaxInvoiceRecvAck || header.Action == Request.eTaxResultRecvAck)
                        InsertElement(_soapHeader, "wsa:RelatesTo", header.RelatesTo);

                    // To 엘리먼트 작성
                    InsertElement(_soapHeader, "wsa:To", header.ToAddress);

                    // Action 엘리먼트 작성
                    InsertElement(_soapHeader, "wsa:Action", header.Action);

                    // Message Header 엘리먼트 작성
                    XmlElement _msgHeader = CreateElement("kec:MessageHeader");
                    {
                        // MessageHeader/Version 엘리먼트 작성
                        // 전자세금계산서 통신규약 버전 이번 지침에서는 ‘3.0’으로 설정
                        InsertElement(_msgHeader, "kec:Version", header.Version);

                        // MessageHeader/From 엘리먼트 작성
                        XmlElement _fromParty = CreateElement("kec:From");
                        {
                            // MessageHeader/From/PartyID 엘리먼트
                            InsertElement(_fromParty, "kec:PartyID", header.FromParty.ID);

                            // MessageHeader/From/PartyName 엘리먼트
                            InsertElement(_fromParty, "kec:PartyName", header.FromParty.Name);

                            _msgHeader.AppendChild(_fromParty);
                        }

                        // MessageHeader/To 엘리먼트
                        XmlElement _toParty = CreateElement("kec:To");
                        {
                            // MessageHeader/To/PartyID 엘리먼트
                            InsertElement(_toParty, "kec:PartyID", header.ToParty.ID);

                            // MessageHeader/To/PartyName 엘리먼트
                            InsertElement(_toParty, "kec:PartyName", header.ToParty.Name);

                            _msgHeader.AppendChild(_toParty);
                        }

                        // MessageHeader/ReplyTo 엘리먼트
                        if (header.Action == Request.eTaxInvoiceSubmit)
                            InsertElement(_msgHeader, "kec:ReplyTo", header.ReplyTo);

                        // MessageHeader/OperationType 엘리먼트
                        InsertElement(_msgHeader, "kec:OperationType", header.OperationType);

                        // MessageHeader/MessageType 엘리먼트
                        InsertElement(_msgHeader, "kec:MessageType", header.MessageType);

                        // MessageHeader/TimeStamp 엘리먼트
                        DateTimeElement(_msgHeader, "kec:TimeStamp", header.TimeStamp, "yyyy-MM-ddTHH:mm:ss.fffZ");

                        _soapHeader.AppendChild(_msgHeader);
                    }
                }

                XmlElement _wsseSecurity = GetSoapSecurity(header, body);
                _soapHeader.AppendChild(_wsseSecurity);
            }

            return _soapHeader;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // <SOAP:Body>
        //-------------------------------------------------------------------------------------------------------------------------
        private XmlElement GetSoapBody(Header header, Body body)
        {
            XmlElement _soapBody = CreateElement("SOAP:Body");
            {
                if (header.Action == Request.eTaxResultRecvAck)
                {
                    XmlElement _responseMessage = CreateElement("kec:ResponseMessage");
                    InsertElement(_responseMessage, "kec:RefResultID", body.ResultID);

                    _soapBody.AppendChild(_responseMessage);
                }
                else
                {
                    XmlElement _requestMessage = CreateElement("kec:RequestMessage");
                    if (header.Action == Request.eTaxInvoiceSubmit)
                    {
                        InsertElement(_requestMessage, "kec:SubmitID", body.SubmitID);
                        InsertElement(_requestMessage, "kec:TotalCount", Convert.ToString(body.TotalCount));
                        InsertElement(_requestMessage, "kec:ReferenceID", body.ReferenceID);
                    }
                    else if (header.Action == Request.eTaxRequestSubmit)
                    {
                        InsertElement(_requestMessage, "kec:RefSubmitID", body.RefSubmitID);
                    }
                    else if (header.Action == Request.eTaxRequestCertSubmit)
                    {
                        InsertElement(_requestMessage, "kec:BusnID", body.RequestParty.ID);
                        InsertElement(_requestMessage, "kec:BusnCrfcNo", body.RequestParty.KecRegId);
                        InsertElement(_requestMessage, "kec:FileType", body.FileType);
                    }

                    _soapBody.AppendChild(_requestMessage);
                }
            }

            return _soapBody;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // <SOAP:Envelope>
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public XmlElement GetSoapEnvelope(Header header, Body body)
        {
            XmlElement _soapEnvelope = CreateElement("SOAP:Envelope");
            {
                //-----------------------------------------------------------------------------------------------------------------
                // SoapEnvelope NameSpaces
                //-----------------------------------------------------------------------------------------------------------------
                foreach (string _key in SoapNameCollections.AllKeys)
                {
                    XmlAttribute _xmlns = SoapDoc.CreateAttribute("xmlns:" + _key);
                    {
                        _xmlns.Value = SoapNameCollections[_key];
                        _soapEnvelope.Attributes.Append(_xmlns);
                    }
                }

                //-------------------------------------------------------------------------------------------------------------
                // SoapHeader
                //-------------------------------------------------------------------------------------------------------------
                XmlElement _soapHeader = GetSoapHeader(header, body);
                _soapEnvelope.AppendChild(_soapHeader);

                //-----------------------------------------------------------------------------------------------------------------
                // SoapBody
                //-----------------------------------------------------------------------------------------------------------------
                XmlElement _soapBody = GetSoapBody(header, body);
                _soapEnvelope.AppendChild(_soapBody);
            }

            return _soapEnvelope;
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        // <SignedSoapEnvelope>
        //-------------------------------------------------------------------------------------------------------------------------//

        /// <summary>
        ///
        /// </summary>
        /// <param name="encrypted"></param>
        /// <param name="x509_certificate2"></param>
        /// <param name="header"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public XmlDocument GetSignedSoapEnvelope(byte[] encrypted, X509Certificate2 x509_certificate2, Header header, Body body)
        {
            HashAlgorithm _digester;
            AsymmetricSignatureFormatter _signer = new RSAPKCS1SignatureFormatter(x509_certificate2.PrivateKey);

            if (x509_certificate2.PrivateKey.KeySize == 2048)
            {
                _digester = new SHA256CryptoServiceProvider();
                _signer.SetHashAlgorithm("SHA256");
            }
            else
            {
                _digester = new SHA1CryptoServiceProvider();
                _signer.SetHashAlgorithm("SHA1");
            }

            header.X509Cert2 = x509_certificate2;
            if (header.Action == Request.eTaxInvoiceSubmit)
                header.DigestValue2 = _digester.ComputeHash(encrypted);

            XmlElement _envelope = GetSoapEnvelope(header, body);

            //#if DEBUG
            //            if (header.Action == Request.eTaxInvoiceSubmit)
            //            {
            //                var _savefile = Path.Combine(output_path, @"WS-Security\2. Transforms전.txt");

            //                MemoryStream _ms = new MemoryStream(Encoding.UTF8.GetBytes(_envelope.OuterXml));
            //                _ms = Convertor.SNG.CanonicalizationToStream(_ms);

            //                File.WriteAllBytes(_savefile, _ms.ToArray());
            //                _ms.Close();
            //            }
            //#endif

            //-------------------------------------------------------------------------------------------------------------------------//
            // First Reference Target
            //-------------------------------------------------------------------------------------------------------------------------//

            // <wsse:Security> remove and clone
            XmlNode _security = _envelope.SelectSingleNode("descendant::wsse:Security", SoapNamespaces);
            if (_security == null)
                throw new ProxyException("not exist <wsse:Security>");

            XmlElement _securityClone = (XmlElement)_security.Clone();
            if (RemoveElement(_envelope, "wsse:Security") == false)
                throw new ProxyException("remove error <wsse:Security>");

            // normalization <SOAP:Envelope>
            XmlElement _envelopeClone = Convertor.SNG.TransformToElement(_envelope);
            _envelopeClone = Convertor.SNG.CanonicalizationToElement(_envelopeClone);

            // <ds:Signature> remove and clone
            XmlNode _signature = _securityClone.SelectSingleNode("descendant::ds:Signature", SoapNamespaces);
            if (_signature == null)
                throw new ProxyException("not exist <ds:Signature>");

            XmlElement _signatureClone = (XmlElement)_signature.Clone();
            if (RemoveElement(_securityClone, "ds:Signature") == false)
                throw new ProxyException("remove error <ds:Signature>");

            // search <SOAP:Header> and Append <wsse:Security>
            XmlElement _header = (XmlElement)_envelopeClone.SelectSingleNode("descendant::SOAP:Header", SoapNamespaces);
            if (_header == null)
                throw new ProxyException("not exist <SOAP:Header>");

            _header.AppendChild(_header.OwnerDocument.ImportNode(_securityClone, true));

            //#if DEBUG
            //            if (header.Action == Request.eTaxInvoiceSubmit)
            //            {
            //                var _savefile = Path.Combine(output_path, @"WS-Security\3. 첫번째ReferenceTarget.txt");
            //                File.WriteAllText(_savefile, _envelopeClone.OuterXml, Encoding.UTF8);
            //            }
            //#endif

            //-------------------------------------------------------------------------------------------------------------------------//
            // Calculate first digest value
            //-------------------------------------------------------------------------------------------------------------------------//
            byte[] _1stDigestValue = _digester.ComputeHash(Encoding.UTF8.GetBytes(_envelopeClone.OuterXml));

            XmlNode _digestValue = _signatureClone.SelectSingleNode("descendant::ds:DigestValue", SoapNamespaces);
            if (_digestValue == null)
                throw new ProxyException("not exist <ds:DigestValue>");

            _digestValue.InnerText = Convert.ToBase64String(_1stDigestValue);

            //-------------------------------------------------------------------------------------------------------------------------//
            // After Transform
            //-------------------------------------------------------------------------------------------------------------------------//

            // <wsse:SecurityTokenReference> remove and clone
            XmlNode _securityTokenReference = _signatureClone.SelectSingleNode("descendant::wsse:SecurityTokenReference", SoapNamespaces);
            if (_securityTokenReference == null)
                throw new ProxyException("not exist <wsse:SecurityTokenReference>");

            XmlElement _securityTokenReferenceClone = (XmlElement)_securityTokenReference.Clone();
            if (RemoveElement(_signatureClone, "wsse:SecurityTokenReference") == false)
                throw new ProxyException("remove error <wsse:SecurityTokenReference>");

            // normalization <ds:Signature>
            XmlElement _signatureNew = Convertor.SNG.TransformToElement(_signatureClone);
            _signatureNew = Convertor.SNG.CanonicalizationToElement(_signatureNew, "");

            // search <wsse:Security> and Append <ds:Signature>
            XmlElement _securityEle = (XmlElement)_envelopeClone.SelectSingleNode("descendant::wsse:Security", SoapNamespaces);
            if (_securityEle == null)
                throw new ProxyException("not exist <wsse:Security>");

            _securityEle.AppendChild(_securityEle.OwnerDocument.ImportNode(_signatureNew, true));

            // transform <wsse:SecurityTokenReference>
            XmlElement _securityTokenReferenceNew = Convertor.SNG.TransformToElement(_securityTokenReferenceClone);

            // search <ds:KeyInfo> and Append <wsse:SecurityTokenReference>
            XmlElement _keyinfo = (XmlElement)_envelopeClone.SelectSingleNode("descendant::ds:KeyInfo", SoapNamespaces);
            if (_keyinfo == null)
                throw new ProxyException("not exist <ds:KeyInfo>");

            _keyinfo.AppendChild(_keyinfo.OwnerDocument.CreateWhitespace("\n"));
            _keyinfo.AppendChild(_keyinfo.OwnerDocument.ImportNode(_securityTokenReferenceNew, true));
            _keyinfo.AppendChild(_keyinfo.OwnerDocument.CreateWhitespace("\n"));

            //-------------------------------------------------------------------------------------------------------------------------//
            // Signature Target
            //-------------------------------------------------------------------------------------------------------------------------//

            // normalization <SOAP:Envelope>
            XmlElement _afterSign = Convertor.SNG.TransformToElement(_envelopeClone);

            //#if DEBUG
            //            if (header.Action == Request.eTaxInvoiceSubmit)
            //            {
            //                var _savefile = Path.Combine(output_path, @"WS-Security\5. Transforms후.txt");
            //                File.WriteAllText(_savefile, _afterSign.OuterXml, Encoding.UTF8);
            //            }
            //#endif

            // search <ds:SignedInfo>
            XmlElement _signedInfo = (XmlElement)_afterSign.SelectSingleNode("descendant::ds:SignedInfo", SoapNamespaces);
            if (_signedInfo == null)
                throw new ProxyException("not exist <ds:SignedInfo>");

            var _signedInfoXml = _signedInfo.OuterXml.Replace(
                                "<ds:SignedInfo xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\">",
                                "<ds:SignedInfo xmlns:SOAP=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" xmlns:kec=\"http://www.kec.or.kr/standard/Tax/\" xmlns:wsa=\"http://www.w3.org/2005/08/addressing\" xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">"
                            );

            //#if DEBUG
            //            if (header.Action == Request.eTaxInvoiceSubmit)
            //            {
            //                var _savefile = Path.Combine(output_path, @"WS-Security\6. SignatureTarget.txt");
            //                File.WriteAllText(_savefile, _signedInfoXml, Encoding.UTF8);
            //            }
            //#endif

            //-------------------------------------------------------------------------------------------------------------------------//
            // Signature
            //-------------------------------------------------------------------------------------------------------------------------//
            byte[] _signatureHash = _digester.ComputeHash(Encoding.UTF8.GetBytes(_signedInfoXml));
            byte[] _signatureSign = _signer.CreateSignature(_signatureHash);

            // search <ds:SignatureValue> and Assign SignatureValue
            XmlElement _signatureValue = (XmlElement)_afterSign.SelectSingleNode("descendant::ds:SignatureValue", SoapNamespaces);
            if (_signatureValue == null)
                throw new ProxyException("not exist <ds:SignatureValue>");

            var _signatureText = Convertor.SNG.FormatEncodedString(_signatureSign);

            _signatureValue.AppendChild(_signatureValue.OwnerDocument.CreateWhitespace("\n"));
            _signatureValue.AppendChild(_signatureValue.OwnerDocument.CreateTextNode(_signatureText));
            _signatureValue.AppendChild(_signatureValue.OwnerDocument.CreateWhitespace("\n"));

            //-------------------------------------------------------------------------------------------------------------------------//
            //
            //-------------------------------------------------------------------------------------------------------------------------//
            XmlDeclaration _declaration = _afterSign.OwnerDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
            _afterSign.OwnerDocument.PrependChild(_declaration);

            return _afterSign.OwnerDocument;
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//
    }
}