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
using OdinSdk.eTaxBill.Security.Mime;
using System;
using System.IO;
using System.Net;

//#pragma warning disable 1591

namespace OdinSdk.eTaxBill.Security.Notice
{
    /// <summary>
    ///
    /// </summary>
    public class Request
    {
        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//

        /// <summary>
        ///
        /// </summary>
        private Request()
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//
        private static volatile Request m_instance = null;

        private static object m_syncRoot = new object();

        /// <summary></summary>
        public static Request SNG
        {
            get
            {
                if (m_instance == null)
                {
                    lock (m_syncRoot)
                    {
                        if (m_instance == null)
                        {
                            m_instance = new Request();
                        }
                    }
                }

                return m_instance;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//
        public const string eTaxInvoiceSubmit = "http://www.kec.or.kr/standard/Tax/TaxInvoiceSubmit";           // 전자세금계산서 전송(OpenETaxBill -> NTS)

        public const string eTaxInvoiceRecvAck = "http://www.kec.or.kr/standard/Tax/TaxInvoiceRecvAck";         // 전자세금계산서 수신확인(OpenETaxBill <- NTS)

        public const string eTaxResultSubmit = "http://www.kec.or.kr/standard/Tax/ResultsSubmit";               // 전자세금계산서 처리결과 전송(OpenETaxBill <- NTS)
        public const string eTaxResultRecvAck = "http://www.kec.or.kr/standard/Tax/ResultsRecvAck";             // 처리결과에 대한 수신 확인(OpenETaxBill -> NTS)

        public const string eTaxRequestSubmit = "http://www.kec.or.kr/standard/Tax/ResultsReqSubmit";           // 처리결과 전송요청(OpenETaxBill -> NTS)
        public const string eTaxRequestRecvAck = "http://www.kec.or.kr/standard/Tax/ResultsReqRecvAck";         // 처리결과 전송요청에 대한 수신확인(OpenETaxBill <- NTS)

        public const string OperationType_InvoiceSubmit = "01";                                                 // 전자세금계산서 제출 업무(OpenETaxBill -> NTS)
        public const string OperationType_ResultSubmit = "02";                                                  // 처리결과 전송 업무(OpenETaxBill <- NTS)
        public const string OperationType_RequestSubmit = "03";                                                 // 처리결과 요청 업무(OpenETaxBill -> NTS)

        public const string MessageType_Request = "01";                                                         // 요청(Request)
        public const string MessageType_Response = "02";                                                        // 응답(Response)

        public const string eTaxRequestCertSubmit = "http://webservice.esero.go.kr/services/RequestCert";       // 공개키 전송요청(OpenETaxBill -> NTS)
        public const string eTaxRequestCertRecvAck = "http://webservice.esero.go.kr/services/RequestRecvCert";  // 공개키 전송요청에 대한 수신확인(OpenETaxBill <- NTS)

        public const string FileType_ZIP = "1";                                                                 // .zip Deflate 알고리즘을 이용한 압축
        public const string FileType_JAR = "2";                                                                 // .jar archiving
        public const string FileType_TAR = "3";                                                                 // .tar.gz unix tar로 archiving 후 gnu zip을 이용하여 압축

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//

        /// <summary>
        ///
        /// </summary>
        /// <param name="soap_part"></param>
        /// <param name="encrypted"></param>
        /// <param name="reference_id"></param>
        /// <param name="request_uri"></param>
        /// <returns></returns>
        public MimeContent TaxInvoiceSubmit(byte[] soap_part, byte[] encrypted, string reference_id, string request_uri)
        {
            MimeContent _result = new MimeContent();

            try
            {
                MimePart _soapPart = new MimePart()
                {
                    ContentType = "text/xml",
                    CharSet = "UTF-8",
                    ContentId = "<" + Convertor.SNG.GetRandHexString(32) + ">",
                    Content = soap_part
                };

                MimePart _attachment = new MimePart()
                {
                    ContentType = "application/octet-stream",
                    ContentId = reference_id,
                    Content = encrypted
                };

                MimeContent _mimeContent = new MimeContent()
                {
                    Boundary = "----=_Part_"
                             + Convertor.SNG.GetRandNumString(5) + "_"
                             + Convertor.SNG.GetRandNumString(8) + "."
                             + Convertor.SNG.GetRandNumString(13)
                };

                _mimeContent.Parts.Add(_soapPart);
                _mimeContent.Parts.Add(_attachment);
                _mimeContent.SetAsStartPart(_soapPart);

                HttpWebRequest _request = (HttpWebRequest)WebRequest.Create(request_uri);
                {
                    byte[] _reqBuffer = (new MimeParser()).SerializeMimeContent(_mimeContent);

                    _request.Method = "POST";
                    _request.ContentType = _mimeContent.ContentType;
                    _request.Headers.Add("SOAPAction", Request.eTaxInvoiceSubmit);
                    _request.ContentLength = _reqBuffer.Length;

                    Stream _reqStream = _request.GetRequestStream();
                    {
                        _reqStream.Write(_reqBuffer, 0, _reqBuffer.Length);
                        _reqStream.Flush();
                    }
                }

                _result = this.GetResponseMime(_request);
            }
            catch (Exception ex)
            {
                _result.ErrorMessage = "TaxInvoiceSubmit -> " + ex.Message;
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="soap_part"></param>
        /// <param name="request_uri"></param>
        /// <returns></returns>
        public MimeContent TaxRequestSubmit(byte[] soap_part, string request_uri)
        {
            MimeContent _result = new MimeContent();

            try
            {
                MimePart _soapPart = new MimePart()
                {
                    ContentType = "text/xml",
                    CharSet = "UTF-8",
                    ContentId = "<" + Convertor.SNG.GetRandHexString(32) + ">",
                    Content = soap_part
                };

                MimeContent _mimeContent = new MimeContent()
                {
                    Boundary = "----=_Part_"
                             + Convertor.SNG.GetRandNumString(5) + "_"
                             + Convertor.SNG.GetRandNumString(8) + "."
                             + Convertor.SNG.GetRandNumString(13)
                };

                _mimeContent.Parts.Add(_soapPart);
                _mimeContent.SetAsStartPart(_soapPart);

                HttpWebRequest _request = (HttpWebRequest)WebRequest.Create(request_uri);
                {
                    byte[] _reqBuffer = (new MimeParser()).SerializeMimeContent(_mimeContent);

                    _request.Method = "POST";
                    _request.ContentType = _mimeContent.ContentType;
                    _request.Headers.Add("SOAPAction", Request.eTaxRequestSubmit);
                    _request.ContentLength = _reqBuffer.Length;

                    Stream _reqStream = _request.GetRequestStream();
                    {
                        _reqStream.Write(_reqBuffer, 0, _reqBuffer.Length);
                        _reqStream.Flush();
                    }
                }

                _result = this.GetResponseMime(_request);
            }
            catch (Exception ex)
            {
                _result.ErrorMessage = "TaxRequestSubmit -> " + ex.Message;
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="soap_part"></param>
        /// <param name="request_uri"></param>
        /// <returns></returns>
        public MimeContent TaxRequestCertSubmit(byte[] soap_part, string request_uri)
        {
            MimeContent _result = new MimeContent();

            try
            {
                MimePart _soapPart = new MimePart()
                {
                    ContentType = "text/xml",
                    CharSet = "UTF-8",
                    ContentId = "<" + Convertor.SNG.GetRandHexString(32) + ">",
                    Content = soap_part
                };

                MimeContent _mimeContent = new MimeContent()
                {
                    Boundary = "----=_Part_"
                             + Convertor.SNG.GetRandNumString(5) + "_"
                             + Convertor.SNG.GetRandNumString(8) + "."
                             + Convertor.SNG.GetRandNumString(13)
                };

                _mimeContent.Parts.Add(_soapPart);
                _mimeContent.SetAsStartPart(_soapPart);

                HttpWebRequest _request = (HttpWebRequest)WebRequest.Create(request_uri);
                {
                    byte[] _reqBuffer = (new MimeParser()).SerializeMimeContent(_mimeContent);

                    _request.Method = "POST";
                    _request.ContentType = _mimeContent.ContentType;
                    _request.Headers.Add("SOAPAction", Request.eTaxRequestCertSubmit);
                    _request.ContentLength = _reqBuffer.Length;

                    Stream _reqStream = _request.GetRequestStream();
                    {
                        _reqStream.Write(_reqBuffer, 0, _reqBuffer.Length);
                        _reqStream.Flush();
                    }
                }

                _result = this.GetResponseMime(_request);
            }
            catch (Exception ex)
            {
                _result.ErrorMessage = "TaxRequestCertSubmit -> " + ex.Message;
            }

            return _result;
        }

        private MimeContent GetResponseMime(HttpWebRequest request)
        {
            MimeContent _result = new MimeContent();

            try
            {
                MemoryStream _builder = new MemoryStream();

                WebResponse _response = request.GetResponse();
                {
                    Stream _rspStream = _response.GetResponseStream();
                    {
                        Byte[] _rspBuffer = new Byte[8192];
                        var _count = _rspStream.Read(_rspBuffer, 0, _rspBuffer.Length);

                        while (_count > 0)
                        {
                            _builder.Write(_rspBuffer, 0, _count);

                            _count = _rspStream.Read(_rspBuffer, 0, _rspBuffer.Length);
                        }

                        _rspStream.Close();
                    }
                }

                _result = (new MimeParser()).DeserializeMimeContent(_response.ContentType, _builder.ToArray());
                _result.StatusCode = 0;
            }
            catch (Exception ex)
            {
                _result.ErrorMessage = "GetResponseMime -> " + ex.Message;
            }

            return _result;
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//
    }
}