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
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;

namespace OdinSdk.eTaxBill.Security.Issue
{
    /// <summary>
    ///
    /// </summary>
    public class Writer
    {
        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//

        /// <summary>
        ///
        /// </summary>
        private Writer()
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//
        private static object m_syncRoot = new object();

        private readonly static Lazy<Writer> m_writer = new Lazy<Writer>(() =>
        {
            return new Writer();
        });

        /// <summary></summary>
        public static Writer SNG
        {
            get
            {
                return m_writer.Value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 전자세금계산서 1매를 생성한다.
        /// </summary>
        /// <param name="exchange_data_set">전자세금계산 신고 항목 데이터셋(0-Invoice,1-LineItem)</param>
        public Writer(DataSet exchange_data_set)
        {
            ExchangeSet = exchange_data_set;

            TaxDocument.AppendChild(TaxInvoice());
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        public DataSet ExchangeSet
        {
            get;
            set;
        }

        private DataRow InvoiceRow
        {
            get
            {
                return ExchangeSet.Tables[0].Rows[0];
            }
        }

        private DataTable ItemTable
        {
            get
            {
                return ExchangeSet.Tables[1];
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//
        private NameValueCollection m_nameCollections = null;

        /// <summary>
        ///
        /// </summary>
        public NameValueCollection TaxNameCollections
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

                            m_nameCollections.Add("", "urn:kr:or:kec:standard:Tax:ReusableAggregateBusinessInformationEntitySchemaModule:1:0");
                            m_nameCollections.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                            m_nameCollections.Add("xsi:schemaLocation", "http://www.kec.or.kr/standard/Tax/TaxInvoiceSchemaModule_1.0.xsd");
                        }
                    }
                }

                return m_nameCollections;
            }
        }

        private XmlNamespaceManager m_taxNSpaces = null;

        /// <summary>
        ///
        /// </summary>
        public XmlNamespaceManager TaxNamespaces
        {
            get
            {
                if (m_taxNSpaces == null)
                {
                    lock (m_syncRoot)
                    {
                        if (m_taxNSpaces == null)
                        {
                            m_taxNSpaces = new XmlNamespaceManager(new NameTable());

                            foreach (string _key in this.TaxNameCollections.AllKeys)
                            {
                                if (_key.IndexOf(':') < 1)
                                    m_taxNSpaces.AddNamespace(_key, this.TaxNameCollections[_key]);
                            }
                        }
                    }
                }

                return m_taxNSpaces;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private XmlDocument m_xmldoc = null;

        /// <summary>
        ///
        /// </summary>
        public XmlDocument TaxDocument
        {
            get
            {
                if (m_xmldoc == null)
                {
                    lock (m_syncRoot)
                    {
                        if (m_xmldoc == null)
                            m_xmldoc = new XmlDocument();
                    }
                }

                return m_xmldoc;
            }
            private set
            {
                m_xmldoc = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public MemoryStream TaxStream
        {
            get
            {
                return new MemoryStream(Encoding.UTF8.GetBytes(TaxDocument.OuterXml));
            }
        }

        private XmlElement CreateElement(string qualified_name)
        {
            return TaxDocument.CreateElement(qualified_name, TaxNamespaces.DefaultNamespace);
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

        private void InvoiceTextElement(XmlElement element, string qualified_name, string field_name)
        {
            TextElement(InvoiceRow, element, qualified_name, field_name);
        }

        private void InvoiceDateElement(XmlElement element, string qualified_name, string field_name, string format)
        {
            DateElement(InvoiceRow, element, qualified_name, field_name, format);
        }

        private string InvoiceDateToString(string field_name, string format)
        {
            return DateToString(InvoiceRow, field_name, format);
        }

        private bool IsInvoiceZeroOrEmpty(string field_name)
        {
            return IsZeroOrEmpty(InvoiceRow, field_name);
        }

        private void TextElement(DataRow data_row, XmlElement element, string qualified_name, string field_name)
        {
            if (data_row[field_name] != DBNull.Value)
            {
                var _fieldValue = Convert.ToString(data_row[field_name]);
                InsertElement(element, qualified_name, _fieldValue);
            }
        }

        private void DateElement(DataRow data_row, XmlElement element, string qualified_name, string field_name, string format)
        {
            if (data_row[field_name] != DBNull.Value)
            {
                var _fieldValue = DateToString(data_row, field_name, format);
                InsertElement(element, qualified_name, _fieldValue);
            }
        }

        private string DateToString(DataRow data_row, string field_name, string format)
        {
            //if (data_row[field_name] == DBNull.Value)
            //    throw new ProxyException();

            return Convert.ToDateTime(data_row[field_name]).ToString(format);
        }

        private bool IsZeroOrEmpty(DataRow data_row, string field_name)
        {
            var _result = true;

            if (data_row[field_name] != DBNull.Value)
            {
                if (Convert.ToDecimal(data_row[field_name]) != 0)
                    _result = false;
            }

            return _result;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // <ExchangedDocument>
        //-------------------------------------------------------------------------------------------------------------------------
        private XmlElement ExchangedDocument()
        {
            XmlElement _exchangedEle = CreateElement("ExchangedDocument");
            {
                // 서비스 사업자 관리번호 24자리 (0..1)
                InvoiceTextElement(_exchangedEle, "ID", "exchangeId");

                // 발행일시(전자서명 생성일시)
                InvoiceDateElement(_exchangedEle, "IssueDateTime", "issueDate", "yyyyMMddHHmmss");
            }

            return _exchangedEle;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // <TaxInvoiceDocument> - 기본정보
        //-------------------------------------------------------------------------------------------------------------------------
        private XmlElement TaxInvoiceDocument()
        {
            XmlElement _taxInvoiceEle = CreateElement("TaxInvoiceDocument");
            {
                // 승인번호 (S/24/1..1) - 작성 연월일 8자리 + 국세청 등록번호 8자리 + 발행 일련번호 8자리
                InvoiceTextElement(_taxInvoiceEle, "IssueID", "issueId");

                // 종류 (N/4/1..1)
                InvoiceTextElement(_taxInvoiceEle, "TypeCode", "typeCode");

                // 비고 (S/150/0..3)
                InvoiceTextElement(_taxInvoiceEle, "DescriptionText", "description");

                // 작성일자 (N/8/1..1)
                InvoiceDateElement(_taxInvoiceEle, "IssueDateTime", "issueDate", "yyyyMMdd");

                // 수정코드 (N/2/0..1)
                InvoiceTextElement(_taxInvoiceEle, "AmendmentStatusCode", "amendmentCode");

                // 영수/청구 구분 (N/2/0..1)
                InvoiceTextElement(_taxInvoiceEle, "PurposeCode", "purposeCode");

                // 당초승인번호 (S/24/0..1)
                InvoiceTextElement(_taxInvoiceEle, "OriginalIssueID", "refIssueId");

                // 수입 세금계산서
                XmlElement _refImportEle = CreateElement("ReferencedImportDocument");
                {
                    // 수입 신고서 번호 (S/15/0.1)
                    InvoiceTextElement(_refImportEle, "ID", "importId");

                    if (_refImportEle.HasChildNodes == true)
                    {
                        // 수입총건수 (N/6/0.1)
                        InvoiceTextElement(_refImportEle, "ItemQuantity", "importQuantity");

                        // 일괄발급
                        XmlElement _acceptablePeriod = CreateElement("AcceptablePeriod");
                        {
                            // 시작일자 (S/8/0.1) - YYYYMMDD
                            InvoiceDateElement(_acceptablePeriod, "StartDateTime", "importStart", "yyyyMMdd");

                            // 종료일자 (S/8/0.1) - YYYYMMDD
                            InvoiceDateElement(_acceptablePeriod, "EndDateTime", "importEnd", "yyyyMMdd");

                            if (_acceptablePeriod.HasChildNodes == true)
                                _refImportEle.AppendChild(_acceptablePeriod);
                        }

                        _taxInvoiceEle.AppendChild(_refImportEle);
                    }
                }
            }

            return _taxInvoiceEle;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // <TaxInvoiceTradeSettlement>
        //-------------------------------------------------------------------------------------------------------------------------
        private XmlElement TaxInvoiceTradeSettlement()
        {
            XmlElement _trade = CreateElement("TaxInvoiceTradeSettlement");
            {
                // 공급자 정보
                XmlElement _invoicer = CreateElement("InvoicerParty");
                {
                    // 사업자등록번호
                    InvoiceTextElement(_invoicer, "ID", "invoicerId");

                    if (_invoicer.HasChildNodes == true)
                    {
                        // 공급업체의 업태 (0 or 1)
                        InvoiceTextElement(_invoicer, "TypeCode", "invoicerType");

                        // 사업체명
                        InvoiceTextElement(_invoicer, "NameText", "invoicerName");

                        // 공급업체의 업종 (0 or 1)
                        InvoiceTextElement(_invoicer, "ClassificationCode", "invoicerClass");

                        XmlElement _specifiedOrg = CreateElement("SpecifiedOrganization");
                        {
                            // 공급자 종사업장 식별코드 (0 or 1) - 사업자 단위 과세제도에 따라 국세청에서 부여한 코드
                            InvoiceTextElement(_specifiedOrg, "TaxRegistrationID", "invoicerOrgId");

                            if (_specifiedOrg.HasChildNodes == true)
                                _invoicer.AppendChild(_specifiedOrg);
                        }

                        // 대표자명
                        XmlElement _specifiedPerson = CreateElement("SpecifiedPerson");
                        {
                            InvoiceTextElement(_specifiedPerson, "NameText", "invoicerPerson");

                            if (_specifiedPerson.HasChildNodes == true)
                                _invoicer.AppendChild(_specifiedPerson);
                        }

                        // 담당자
                        XmlElement _definedContact = CreateElement("DefinedContact");
                        {
                            // 공급업체 담당부서 (0 or 1)
                            InvoiceTextElement(_definedContact, "DepartmentNameText", "invoicerDepartment");

                            // 공급업체 담당자명 (0 or 1)
                            InvoiceTextElement(_definedContact, "PersonNameText", "invoicerContactor");

                            // 공급업체 담당자 전화번호 (0 or 1)
                            InvoiceTextElement(_definedContact, "TelephoneCommunication", "invoicerPhone");

                            // 공급업체 담당자 이메일 (0 or 1)
                            InvoiceTextElement(_definedContact, "URICommunication", "invoicerEMail");

                            if (_definedContact.HasChildNodes == true)
                                _invoicer.AppendChild(_definedContact);
                        }

                        // 공급업체의 주소 (0 or 1)
                        XmlElement _specifiedAddress = CreateElement("SpecifiedAddress");
                        {
                            InvoiceTextElement(_specifiedAddress, "LineOneText", "invoicerAddress");

                            if (_specifiedAddress.HasChildNodes == true)
                                _invoicer.AppendChild(_specifiedAddress);
                        }

                        _trade.AppendChild(_invoicer);
                    }
                }

                // 공급받는자 정보
                XmlElement _invoicee = CreateElement("InvoiceeParty");
                {
                    // 사업자 등록 번호
                    InvoiceTextElement(_invoicee, "ID", "invoiceeId");

                    if (_invoicee.HasChildNodes == true)
                    {
                        // 공급받는자 업태 (0 or 1)
                        InvoiceTextElement(_invoicee, "TypeCode", "invoiceeType");

                        // 상호명
                        InvoiceTextElement(_invoicee, "NameText", "invoiceeName");

                        // 공급받는자 업종 (0 or 1)
                        InvoiceTextElement(_invoicee, "ClassificationCode", "invoiceeClass");

                        XmlElement _specifiedOrg = CreateElement("SpecifiedOrganization");
                        {
                            // 공급받는자 종사업장 식별코드 (0 or 1) - 사업자 단위 과세제도에 따라 국세청에서 부여한 코드
                            InvoiceTextElement(_specifiedOrg, "TaxRegistrationID", "invoiceeOrgId");

                            // 구분코드: 01-사업자,02-주민등록,03-외국인
                            InvoiceTextElement(_specifiedOrg, "BusinessTypeCode", "invoiceeKind");

                            if (_specifiedOrg.HasChildNodes == true)
                                _invoicee.AppendChild(_specifiedOrg);
                        }

                        // 대표자명
                        XmlElement _specifiedPerson = CreateElement("SpecifiedPerson");
                        {
                            InvoiceTextElement(_specifiedPerson, "NameText", "invoiceePerson");

                            if (_specifiedPerson.HasChildNodes == true)
                                _invoicee.AppendChild(_specifiedPerson);
                        }

                        // 담당자 1
                        XmlElement _primaryContact = CreateElement("PrimaryDefinedContact");
                        {
                            // 공급받는자 담당부서1 (0 or 1)
                            InvoiceTextElement(_primaryContact, "DepartmentNameText", "invoiceeDepartment1");

                            // 공급받는자 담당자명1 (0 or 1)
                            InvoiceTextElement(_primaryContact, "PersonNameText", "invoiceeContactor1");

                            // 공급받는자 담당자 전화번호1 (0 or 1)
                            InvoiceTextElement(_primaryContact, "TelephoneCommunication", "invoiceePhone1");

                            // 공급받는자 담당자 이메일1 (0 or 1)
                            InvoiceTextElement(_primaryContact, "URICommunication", "invoiceeEMail1");

                            if (_primaryContact.HasChildNodes == true)
                                _invoicee.AppendChild(_primaryContact);
                        }

                        // 담당자 2
                        XmlElement _secondaryContact = CreateElement("SecondaryDefinedContact");
                        {
                            // 공급받는자 담당부서2 (0 or 1)
                            InvoiceTextElement(_secondaryContact, "DepartmentNameText", "invoiceeDepartment2");

                            // 공급받는자 담당자명2 (0 or 1)
                            InvoiceTextElement(_secondaryContact, "PersonNameText", "invoiceeContactor2");

                            // 공급받는자 담당자 전화번호2 (0 or 1)
                            InvoiceTextElement(_secondaryContact, "TelephoneCommunication", "invoiceePhone2");

                            // 공급받는자 담당자 이메일2 (0 or 1)
                            InvoiceTextElement(_secondaryContact, "URICommunication", "invoiceeEMail2");

                            if (_secondaryContact.HasChildNodes == true)
                                _invoicee.AppendChild(_secondaryContact);
                        }

                        // 공급받는자 주소 (0 or 1)
                        XmlElement _specifiedAddress = CreateElement("SpecifiedAddress");
                        {
                            InvoiceTextElement(_specifiedAddress, "LineOneText", "invoiceeAddress");

                            if (_specifiedAddress.HasChildNodes == true)
                                _invoicee.AppendChild(_specifiedAddress);
                        }

                        _trade.AppendChild(_invoicee);
                    }
                }

                // 수탁 사업자 정보
                XmlElement _broker = CreateElement("BrokerParty");
                {
                    // 사업자 등록 번호
                    InvoiceTextElement(_broker, "ID", "brokerId");

                    if (_broker.HasChildNodes == true)
                    {
                        // 수탁사업자 업태 (0 or 1)
                        InvoiceTextElement(_broker, "TypeCode", "brokerType");

                        // 상호명
                        InvoiceTextElement(_broker, "NameText", "brokerName");

                        // 수탁사업자 업종 (0 or 1)
                        InvoiceTextElement(_broker, "ClassificationCode", "brokerClass");

                        XmlElement _specifiedOrg = CreateElement("SpecifiedOrganization");
                        {
                            // 수탁 사업자 종사업장 식별코드 (0 or 1) - 사업자 단위 과세제도에 따라 국세청에서 부여한 코드
                            InvoiceTextElement(_specifiedOrg, "TaxRegistrationID", "brokerOrgId");

                            if (_specifiedOrg.HasChildNodes == true)
                                _broker.AppendChild(_specifiedOrg);
                        }

                        // 대표자명
                        XmlElement _specifiedPerson = CreateElement("SpecifiedPerson");
                        {
                            InvoiceTextElement(_specifiedPerson, "NameText", "brokerPerson");

                            if (_specifiedPerson.HasChildNodes == true)
                                _broker.AppendChild(_specifiedPerson);
                        }

                        // 담당자
                        XmlElement _definedContact = CreateElement("DefinedContact");
                        {
                            // 수탁사업자 담당부서 (0 or 1)
                            InvoiceTextElement(_definedContact, "DepartmentNameText", "brokerDepartment");

                            // 수탁사업자 담당자명 (0 or 1)
                            InvoiceTextElement(_definedContact, "PersonNameText", "brokerContactor");

                            // 수탁사업자 담당자 전화번호 (0 or 1)
                            InvoiceTextElement(_definedContact, "TelephoneCommunication", "brokerPhone");

                            // 수탁사업자 담당자 이메일 (0 or 1)
                            InvoiceTextElement(_definedContact, "URICommunication", "brokerEMail");

                            if (_definedContact.HasChildNodes == true)
                                _broker.AppendChild(_definedContact);
                        }

                        // 수탁사업자 주소 (0 or 1)
                        XmlElement _specifiedAddress = CreateElement("SpecifiedAddress");
                        {
                            InvoiceTextElement(_specifiedAddress, "LineOneText", "brokerAddress");

                            if (_specifiedAddress.HasChildNodes == true)
                                _broker.AppendChild(_specifiedAddress);
                        }

                        _trade.AppendChild(_broker);
                    }
                }

                // 결제방법별 금액
                XmlElement _paymentMeans;
                {
                    // 현금
                    _paymentMeans = CreateElement("SpecifiedPaymentMeans");
                    {
                        // 결제방법 코드 (0 or 1)
                        InsertElement(_paymentMeans, "TypeCode", "10");

                        // 결제방법별 금액 (0 or 1)
                        InvoiceTextElement(_paymentMeans, "PaidAmount", "paidCash");

                        if (_paymentMeans.HasChildNodes == true)
                            _trade.AppendChild(_paymentMeans);
                    }

                    // 수표
                    _paymentMeans = CreateElement("SpecifiedPaymentMeans");
                    {
                        // 결제방법 코드 (0 or 1)
                        InsertElement(_paymentMeans, "TypeCode", "20");

                        // 결제방법별 금액 (0 or 1)
                        InvoiceTextElement(_paymentMeans, "PaidAmount", "paidCheck");

                        if (_paymentMeans.HasChildNodes == true)
                            _trade.AppendChild(_paymentMeans);
                    }

                    // 어음
                    _paymentMeans = CreateElement("SpecifiedPaymentMeans");
                    {
                        // 결제방법 코드 (0 or 1)
                        InsertElement(_paymentMeans, "TypeCode", "30");

                        // 결제방법별 금액 (0 or 1)
                        InvoiceTextElement(_paymentMeans, "PaidAmount", "paidNote");

                        if (_paymentMeans.HasChildNodes == true)
                            _trade.AppendChild(_paymentMeans);
                    }

                    // 외상
                    _paymentMeans = CreateElement("SpecifiedPaymentMeans");
                    {
                        // 결제방법 코드 (0 or 1)
                        InsertElement(_paymentMeans, "TypeCode", "40");

                        // 결제방법별 금액 (0 or 1)
                        InvoiceTextElement(_paymentMeans, "PaidAmount", "paidCredit");

                        if (_paymentMeans.HasChildNodes == true)
                            _trade.AppendChild(_paymentMeans);
                    }
                }

                // Summary
                XmlElement _summation = CreateElement("SpecifiedMonetarySummation");
                {
                    // 공급가액합계
                    InvoiceTextElement(_summation, "ChargeTotalAmount", "chargeTotal");

                    // 세액 합계 (0 or 1)
                    InvoiceTextElement(_summation, "TaxTotalAmount", "taxTotal");

                    // 총금액
                    InvoiceTextElement(_summation, "GrandTotalAmount", "grandTotal");

                    if (_summation.HasChildNodes == true)
                        _trade.AppendChild(_summation);
                }
            }

            return _trade;
        }

        private XmlElement TaxInvoiceTradeLineItem(DataRow item_row)
        {
            XmlElement _lineitem = CreateElement("TaxInvoiceTradeLineItem");
            {
                // 물품 일련번호 (0 or 1)
                TextElement(item_row, _lineitem, "SequenceNumeric", "seqNo");

                // 물품과 관련된 자유기술문 (0 or 1)
                TextElement(item_row, _lineitem, "DescriptionText", "description");

                // 물품 공급 가액 (0 or 1)
                TextElement(item_row, _lineitem, "InvoiceAmount", "invoiceAmount");

                // 물품 수량 (0 or 1)
                TextElement(item_row, _lineitem, "ChargeableUnitQuantity", "quantity");

                // 물품에 대한 규격 (0 or 1)
                TextElement(item_row, _lineitem, "InformationText", "information");

                // 물품명 (0 or 1)
                TextElement(item_row, _lineitem, "NameText", "itemName");

                // 물품 공급일자 (0 or 1)
                DateElement(item_row, _lineitem, "PurchaseExpiryDateTime", "purchaseDate", "yyyyMMdd");

                // 물품 세액 (0 or 1)
                XmlElement _totalTax = CreateElement("TotalTax");
                {
                    TextElement(item_row, _totalTax, "CalculatedAmount", "taxAmount");

                    if (_totalTax.HasChildNodes == true)
                        _lineitem.AppendChild(_totalTax);
                }

                // 물품 단가 (0 or 1)
                XmlElement _unitPrice = CreateElement("UnitPrice");
                {
                    TextElement(item_row, _unitPrice, "UnitAmount", "unitPrice");

                    if (_unitPrice.HasChildNodes == true)
                        _lineitem.AppendChild(_unitPrice);
                }
            }

            return _lineitem;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // <TaxInvoice>
        //-------------------------------------------------------------------------------------------------------------------------
        private XmlElement TaxInvoice()
        {
            XmlElement _taxInvoice = CreateElement("TaxInvoice");
            {
                //-----------------------------------------------------------------------------------------------------------------
                // <TaxInvoice NameSpaces and Attributes>
                //-----------------------------------------------------------------------------------------------------------------
                foreach (string _key in TaxNameCollections.AllKeys)
                {
                    if (_key.IndexOf(':') < 1)
                    {
                        var _name = (String.IsNullOrEmpty(_key) == true) ? "xmlns" : "xmlns:" + _key;

                        XmlAttribute _xmlns = TaxDocument.CreateAttribute(_name);
                        {
                            _xmlns.Value = TaxNameCollections[_key];
                            _taxInvoice.Attributes.Append(_xmlns);
                        }
                    }
                    else
                    {
                        string[] _names = _key.Split(':');

                        XmlAttribute _attribute = TaxDocument.CreateAttribute(_names[0], _names[1], TaxNameCollections[_names[0]]);
                        {
                            _attribute.Value = TaxNamespaces.DefaultNamespace + " " + TaxNameCollections[_key];
                            _taxInvoice.Attributes.Append(_attribute);
                        }
                    }
                }

                //-----------------------------------------------------------------------------------------------------------------
                // <xsd:element name="ExchangedDocument" type="ExchangedDocumentType"/>
                //-----------------------------------------------------------------------------------------------------------------
                XmlElement _exchange = ExchangedDocument();
                _taxInvoice.AppendChild(_exchange);

                //-----------------------------------------------------------------------------------------------------------------
                // <TaxInvoiceDocument>
                //-----------------------------------------------------------------------------------------------------------------
                XmlElement _invoice = TaxInvoiceDocument();
                _taxInvoice.AppendChild(_invoice);

                //-----------------------------------------------------------------------------------------------------------------
                // <TaxInvoiceTradeSettlement>
                //-----------------------------------------------------------------------------------------------------------------
                XmlElement _trade = TaxInvoiceTradeSettlement();
                _taxInvoice.AppendChild(_trade);

                //-----------------------------------------------------------------------------------------------------------------
                // <TaxInvoiceTradeLineItem>
                //-----------------------------------------------------------------------------------------------------------------
                for (int i = 0; i < ItemTable.Rows.Count; i++)
                {
                    XmlElement _lineItem = TaxInvoiceTradeLineItem(ItemTable.Rows[i]);
                    _taxInvoice.AppendChild(_lineItem);
                }
            }

            return _taxInvoice;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}