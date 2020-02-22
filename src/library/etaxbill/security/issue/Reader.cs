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

using OdinSdk.eTaxBill.Security.Signature;
using OdinSdk.eTaxBill.Utility;
using System;
using System.Data;
using System.Xml;
using System.Xml.XPath;

namespace OdinSdk.eTaxBill.Security.Issue
{
    /// <summary>
    ///
    /// </summary>
    public class Reader
    {
        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        private Reader()
        {
            this.InvoiceXml = new XmlDocument();
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//
        private readonly static Lazy<Reader> m_reader = new Lazy<Reader>(() =>
        {
            return new Reader();
        });

        /// <summary></summary>
        public static Reader SNG
        {
            get
            {
                return m_reader.Value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// XML 문서를 INVOICE/LINEITEM DataSet으로 변환 한다.
        /// </summary>
        /// <param name="invoice_xml">전자세금계산서</param>
        public Reader(XmlDocument invoice_xml)
        {
            this.InvoiceXml = invoice_xml;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private DataSet m_invoiceSet = null;

        private DataSet InvoiceSet
        {
            get
            {
                if (m_invoiceSet == null)
                    m_invoiceSet = Schema.SNG.GetTaxSchema();

                return m_invoiceSet;
            }
        }

        private DataTable m_invoiceTbl = null;

        private DataTable InvoiceTbl
        {
            get
            {
                if (m_invoiceTbl == null)
                    m_invoiceTbl = this.InvoiceSet.Tables["TB_eTAX_INVOICE"];

                return m_invoiceTbl;
            }
        }

        private DataTable m_lineitemTbl = null;

        private DataTable LineItemTbl
        {
            get
            {
                if (m_lineitemTbl == null)
                    m_lineitemTbl = this.InvoiceSet.Tables["TB_eTAX_LINEITEM"];

                return m_lineitemTbl;
            }
        }

        private DataTable m_purchaseTbl = null;

        private DataTable PurchaseTbl
        {
            get
            {
                if (m_purchaseTbl == null)
                    m_purchaseTbl = this.InvoiceSet.Tables["TB_eTAX_PURCHASE"];

                return m_purchaseTbl;
            }
        }

        private DataTable m_puritemTbl = null;

        private DataTable PurchaseItemTbl
        {
            get
            {
                if (m_puritemTbl == null)
                    m_puritemTbl = this.InvoiceSet.Tables["TB_eTAX_PURITEM"];

                return m_puritemTbl;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private XmlDocument InvoiceXml
        {
            get;
            set;
        }

        private XmlNamespaceManager m_xmlNsMgr = null;

        private XmlNamespaceManager XmlNsMgr
        {
            get
            {
                if (m_xmlNsMgr == null)
                {
                    m_xmlNsMgr = new XmlNamespaceManager(this.InvoiceXml.NameTable);
                    m_xmlNsMgr.AddNamespace("etax", XSignature.SNG.SignNameCollections[""]);
                }

                return m_xmlNsMgr;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // <ExchangedDocument>
        //-------------------------------------------------------------------------------------------------------------------------
        private void AssignFieldValue(DataRow invoice_row, string name, string value, string format)
        {
            if (invoice_row.Table.Columns.IndexOf(name) >= 0)
            {
                if (invoice_row.Table.Columns[name].DataType == typeof(DateTime))
                {
                    invoice_row[name] = DateTime.ParseExact(value, format, System.Globalization.CultureInfo.CurrentCulture);
                }
                else if (invoice_row.Table.Columns[name].DataType == typeof(Decimal))
                {
                    if (String.IsNullOrEmpty(value) == false)
                        invoice_row[name] = Decimal.Parse(value);
                }
                else
                {
                    invoice_row[name] = value;
                }
            }
        }

        private void ExchangedDocument(DataRow invoice_row)
        {
            XPathExpression _xexpr = XPathExpression.Compile("//etax:ExchangedDocument");
            _xexpr.SetContext(this.XmlNsMgr);

            XPathNavigator _nav = this.InvoiceXml.CreateNavigator().SelectSingleNode(_xexpr);
            if (_nav.MoveToChild(XPathNodeType.Element) == true)
            {
                do
                {
                    var _format = "";

                    var _name = _nav.Name;
                    {
                        if (_name == "ID")
                            _name = "exchangeId";
                        else if (_name == "IssueDateTime")
                        {
                            _name = "exchangeDate";
                            _format = "yyyyMMddHHmmss";
                        }
                    }

                    this.AssignFieldValue(invoice_row, _name, _nav.Value, _format);
                }
                while (_nav.MoveToNext(XPathNodeType.Element));
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // <TaxInvoiceDocument> - 기본정보
        //-------------------------------------------------------------------------------------------------------------------------
        private void TaxInvoiceDocument(DataRow invoice_row)
        {
            XPathExpression _xexpr = XPathExpression.Compile("//etax:TaxInvoiceDocument");
            _xexpr.SetContext(this.XmlNsMgr);

            XPathNavigator _nav = this.InvoiceXml.CreateNavigator().SelectSingleNode(_xexpr);
            if (_nav.MoveToChild(XPathNodeType.Element) == true)
            {
                do
                {
                    var _format = "";

                    var _name = _nav.Name;
                    {
                        if (_name == "IssueID")
                            _name = "issueId";
                        else if (_name == "TypeCode")
                            _name = "typeCode";
                        else if (_name == "DescriptionText")
                            _name = "description";
                        else if (_name == "IssueDateTime")
                        {
                            _name = "issueDate";
                            _format = "yyyyMMdd";
                        }
                        else if (_name == "AmendmentStatusCode")
                            _name = "amendmentCode";
                        else if (_name == "PurposeCode")
                            _name = "purposeCode";
                        else if (_name == "ReferencedImportDocument" && _nav.HasChildren == true)
                        {
                            XPathNodeIterator _iter1 = _nav.SelectChildren(XPathNodeType.Element);
                            while (_iter1.MoveNext() == true)
                            {
                                _format = "";

                                _name = _iter1.Current.Name;
                                if (_name == "ID")
                                    _name = "importId";
                                else if (_name == "ItemQuantity")
                                    _name = "importQuantity";
                                else if (_name == "AcceptablePeriod" && _iter1.Current.HasChildren == true)
                                {
                                    XPathNodeIterator _iter2 = _iter1.Current.SelectChildren(XPathNodeType.Element);
                                    while (_iter2.MoveNext() == true)
                                    {
                                        _format = "yyyyMMdd";

                                        _name = _iter2.Current.Name;
                                        if (_name == "StartDateTime")
                                            _name = "importStart";
                                        else if (_name == "EndDateTime")
                                            _name = "importEnd";

                                        this.AssignFieldValue(invoice_row, _name, _iter2.Current.Value, _format);
                                    }

                                    _name = "";
                                }

                                this.AssignFieldValue(invoice_row, _name, _iter1.Current.Value, _format);
                            }

                            _name = "";
                        }
                    }

                    this.AssignFieldValue(invoice_row, _name, _nav.Value, _format);
                }
                while (_nav.MoveToNext(XPathNodeType.Element));
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // <TaxInvoiceTradeSettlement>
        //-------------------------------------------------------------------------------------------------------------------------
        private void TaxInvoiceTradeSettlement(DataRow invoice_row)
        {
            XPathExpression _xexpr = XPathExpression.Compile("//etax:TaxInvoiceTradeSettlement");
            _xexpr.SetContext(this.XmlNsMgr);

            XPathNavigator _nav = this.InvoiceXml.CreateNavigator().SelectSingleNode(_xexpr);
            if (_nav.MoveToChild(XPathNodeType.Element) == true)
            {
                do
                {
                    var _format = "";

                    var _name = _nav.Name;
                    if (_name == "InvoicerParty" && _nav.HasChildren == true)
                    {
                        XPathNodeIterator _iter1 = _nav.SelectChildren(XPathNodeType.Element);
                        while (_iter1.MoveNext() == true)
                        {
                            _format = "";

                            _name = _iter1.Current.Name;
                            if (_name == "ID")
                                _name = "invoicerId";
                            else if (_name == "TypeCode")
                                _name = "invoicerType";
                            else if (_name == "NameText")
                                _name = "invoicerName";
                            else if (_name == "ClassificationCode")
                                _name = "invoicerClass";
                            else if (_name == "SpecifiedOrganization" && _iter1.Current.HasChildren == true)
                            {
                                XPathNodeIterator _iter2 = _iter1.Current.SelectChildren(XPathNodeType.Element);
                                while (_iter2.MoveNext() == true)
                                {
                                    _name = _iter2.Current.Name;
                                    if (_name == "TaxRegistrationID")
                                        _name = "invoicerOrgId";

                                    this.AssignFieldValue(invoice_row, _name, _iter2.Current.Value, _format);
                                }

                                _name = "";
                            }
                            else if (_name == "SpecifiedPerson" && _iter1.Current.HasChildren == true)
                            {
                                XPathNodeIterator _iter2 = _iter1.Current.SelectChildren(XPathNodeType.Element);
                                while (_iter2.MoveNext() == true)
                                {
                                    _name = _iter2.Current.Name;
                                    if (_name == "NameText")
                                        _name = "invoicerPerson";

                                    this.AssignFieldValue(invoice_row, _name, _iter2.Current.Value, _format);
                                }

                                _name = "";
                            }
                            else if (_name == "DefinedContact" && _iter1.Current.HasChildren == true)
                            {
                                XPathNodeIterator _iter2 = _iter1.Current.SelectChildren(XPathNodeType.Element);
                                while (_iter2.MoveNext() == true)
                                {
                                    _name = _iter2.Current.Name;
                                    if (_name == "DepartmentNameText")
                                        _name = "invoicerDepartment";
                                    else if (_name == "PersonNameText")
                                        _name = "invoicerContactor";
                                    else if (_name == "TelephoneCommunication")
                                        _name = "invoicerPhone";
                                    else if (_name == "URICommunication")
                                        _name = "invoicerEMail";

                                    this.AssignFieldValue(invoice_row, _name, _iter2.Current.Value, _format);
                                }

                                _name = "";
                            }
                            else if (_name == "SpecifiedAddress" && _iter1.Current.HasChildren == true)
                            {
                                XPathNodeIterator _iter2 = _iter1.Current.SelectChildren(XPathNodeType.Element);
                                while (_iter2.MoveNext() == true)
                                {
                                    _name = _iter2.Current.Name;
                                    if (_name == "LineOneText")
                                        _name = "invoicerAddress";

                                    this.AssignFieldValue(invoice_row, _name, _iter2.Current.Value, _format);
                                }

                                _name = "";
                            }

                            this.AssignFieldValue(invoice_row, _name, _iter1.Current.Value, _format);
                        }

                        _name = "";
                    }
                    else if (_name == "InvoiceeParty" && _nav.HasChildren == true)
                    {
                        XPathNodeIterator _iter1 = _nav.SelectChildren(XPathNodeType.Element);
                        while (_iter1.MoveNext() == true)
                        {
                            _format = "";

                            _name = _iter1.Current.Name;
                            if (_name == "ID")
                                _name = "invoiceeId";
                            else if (_name == "TypeCode")
                                _name = "invoiceeType";
                            else if (_name == "NameText")
                                _name = "invoiceeName";
                            else if (_name == "ClassificationCode")
                                _name = "invoiceeClass";
                            else if (_name == "SpecifiedOrganization" && _iter1.Current.HasChildren == true)
                            {
                                XPathNodeIterator _iter2 = _iter1.Current.SelectChildren(XPathNodeType.Element);
                                while (_iter2.MoveNext() == true)
                                {
                                    _name = _iter2.Current.Name;
                                    if (_name == "TaxRegistrationID")
                                        _name = "invoiceeOrgId";
                                    else if (_name == "BusinessTypeCode")
                                        _name = "invoiceeKind";

                                    this.AssignFieldValue(invoice_row, _name, _iter2.Current.Value, _format);
                                }

                                _name = "";
                            }
                            else if (_name == "SpecifiedPerson" && _iter1.Current.HasChildren == true)
                            {
                                XPathNodeIterator _iter2 = _iter1.Current.SelectChildren(XPathNodeType.Element);
                                while (_iter2.MoveNext() == true)
                                {
                                    _name = _iter2.Current.Name;
                                    if (_name == "NameText")
                                        _name = "invoiceePerson";

                                    this.AssignFieldValue(invoice_row, _name, _iter2.Current.Value, _format);
                                }

                                _name = "";
                            }
                            else if (_name == "PrimaryDefinedContact" && _iter1.Current.HasChildren == true)
                            {
                                XPathNodeIterator _iter2 = _iter1.Current.SelectChildren(XPathNodeType.Element);
                                while (_iter2.MoveNext() == true)
                                {
                                    _name = _iter2.Current.Name;
                                    if (_name == "DepartmentNameText")
                                        _name = "invoiceeDepartment1";
                                    else if (_name == "PersonNameText")
                                        _name = "invoiceeContactor1";
                                    else if (_name == "TelephoneCommunication")
                                        _name = "invoiceePhone1";
                                    else if (_name == "URICommunication")
                                        _name = "invoiceeEMail1";

                                    this.AssignFieldValue(invoice_row, _name, _iter2.Current.Value, _format);
                                }

                                _name = "";
                            }
                            else if (_name == "SecondaryDefinedContact" && _iter1.Current.HasChildren == true)
                            {
                                XPathNodeIterator _iter2 = _iter1.Current.SelectChildren(XPathNodeType.Element);
                                while (_iter2.MoveNext() == true)
                                {
                                    _name = _iter2.Current.Name;
                                    if (_name == "DepartmentNameText")
                                        _name = "invoiceeDepartment2";
                                    else if (_name == "PersonNameText")
                                        _name = "invoiceeContactor2";
                                    else if (_name == "TelephoneCommunication")
                                        _name = "invoiceePhone2";
                                    else if (_name == "URICommunication")
                                        _name = "invoiceeEMail2";

                                    this.AssignFieldValue(invoice_row, _name, _iter2.Current.Value, _format);
                                }

                                _name = "";
                            }
                            else if (_name == "SpecifiedAddress" && _iter1.Current.HasChildren == true)
                            {
                                XPathNodeIterator _iter2 = _iter1.Current.SelectChildren(XPathNodeType.Element);
                                while (_iter2.MoveNext() == true)
                                {
                                    _name = _iter2.Current.Name;
                                    if (_name == "LineOneText")
                                        _name = "invoiceeAddress";

                                    this.AssignFieldValue(invoice_row, _name, _iter2.Current.Value, _format);
                                }

                                _name = "";
                            }

                            this.AssignFieldValue(invoice_row, _name, _iter1.Current.Value, _format);
                        }

                        _name = "";
                    }
                    else if (_name == "BrokerParty" && _nav.HasChildren == true)
                    {
                        XPathNodeIterator _iter1 = _nav.SelectChildren(XPathNodeType.Element);
                        while (_iter1.MoveNext() == true)
                        {
                            _format = "";

                            _name = _iter1.Current.Name;
                            if (_name == "ID")
                                _name = "brokerId";
                            else if (_name == "TypeCode")
                                _name = "brokerType";
                            else if (_name == "NameText")
                                _name = "brokerName";
                            else if (_name == "ClassificationCode")
                                _name = "brokerClass";
                            else if (_name == "SpecifiedOrganization" && _iter1.Current.HasChildren == true)
                            {
                                XPathNodeIterator _iter2 = _iter1.Current.SelectChildren(XPathNodeType.Element);
                                while (_iter2.MoveNext() == true)
                                {
                                    _name = _iter2.Current.Name;
                                    if (_name == "TaxRegistrationID")
                                        _name = "brokerOrgId";

                                    this.AssignFieldValue(invoice_row, _name, _iter2.Current.Value, _format);
                                }

                                _name = "";
                            }
                            else if (_name == "SpecifiedPerson" && _iter1.Current.HasChildren == true)
                            {
                                XPathNodeIterator _iter2 = _iter1.Current.SelectChildren(XPathNodeType.Element);
                                while (_iter2.MoveNext() == true)
                                {
                                    _name = _iter2.Current.Name;
                                    if (_name == "NameText")
                                        _name = "brokerPerson";

                                    this.AssignFieldValue(invoice_row, _name, _iter2.Current.Value, _format);
                                }

                                _name = "";
                            }
                            else if (_name == "DefinedContact" && _iter1.Current.HasChildren == true)
                            {
                                XPathNodeIterator _iter2 = _iter1.Current.SelectChildren(XPathNodeType.Element);
                                while (_iter2.MoveNext() == true)
                                {
                                    _name = _iter2.Current.Name;
                                    if (_name == "DepartmentNameText")
                                        _name = "brokerDepartment";
                                    else if (_name == "PersonNameText")
                                        _name = "brokerContactor";
                                    else if (_name == "TelephoneCommunication")
                                        _name = "brokerPhone";
                                    else if (_name == "URICommunication")
                                        _name = "brokerEMail";

                                    this.AssignFieldValue(invoice_row, _name, _iter2.Current.Value, _format);
                                }

                                _name = "";
                            }
                            else if (_name == "SpecifiedAddress" && _iter1.Current.HasChildren == true)
                            {
                                XPathNodeIterator _iter2 = _iter1.Current.SelectChildren(XPathNodeType.Element);
                                while (_iter2.MoveNext() == true)
                                {
                                    _name = _iter2.Current.Name;
                                    if (_name == "LineOneText")
                                        _name = "brokerAddress";

                                    this.AssignFieldValue(invoice_row, _name, _iter2.Current.Value, _format);
                                }

                                _name = "";
                            }

                            this.AssignFieldValue(invoice_row, _name, _iter1.Current.Value, _format);
                        }

                        _name = "";
                    }
                    else if (_name == "SpecifiedPaymentMeans" && _nav.HasChildren == true)
                    {
                        var _typeCode = "";
                        var _paidAmount = "";

                        XPathNodeIterator _iter1 = _nav.SelectChildren(XPathNodeType.Element);
                        while (_iter1.MoveNext() == true)
                        {
                            _name = _iter1.Current.Name;
                            if (_name == "TypeCode")
                                _typeCode = _iter1.Current.Value;
                            else if (_name == "PaidAmount")
                                _paidAmount = _iter1.Current.Value;
                        }

                        if (_typeCode == "10")
                            _name = "paidCash";
                        else if (_typeCode == "20")
                            _name = "paidCheck";
                        else if (_typeCode == "30")
                            _name = "paidNote";
                        else if (_typeCode == "40")
                            _name = "paidCredit";

                        this.AssignFieldValue(invoice_row, _name, _paidAmount, _format);
                        _name = "";
                    }
                    else if (_name == "SpecifiedMonetarySummation" && _nav.HasChildren == true)
                    {
                        XPathNodeIterator _iter1 = _nav.SelectChildren(XPathNodeType.Element);
                        while (_iter1.MoveNext() == true)
                        {
                            _format = "";

                            _name = _iter1.Current.Name;
                            if (_name == "ChargeTotalAmount")
                                _name = "chargeTotal";
                            else if (_name == "TaxTotalAmount")
                                _name = "taxTotal";
                            else if (_name == "GrandTotalAmount")
                                _name = "grandTotal";

                            this.AssignFieldValue(invoice_row, _name, _iter1.Current.Value, _format);
                        }

                        _name = "";
                    }

                    this.AssignFieldValue(invoice_row, _name, _nav.Value, _format);
                }
                while (_nav.MoveToNext(XPathNodeType.Element));
            }
        }

        private void TaxInvoiceTradeLineItem(DataTable line_item_table, string issue_id)
        {
            XPathExpression _xexpr = XPathExpression.Compile("//etax:TaxInvoiceTradeLineItem");
            _xexpr.SetContext(this.XmlNsMgr);

            XPathNavigator _nav = this.InvoiceXml.CreateNavigator().SelectSingleNode(_xexpr);

            do
            {
                if (_nav.HasChildren == true)
                {
                    DataRow _lineitemRow = line_item_table.NewRow();

                    foreach (DataColumn _dc in line_item_table.Columns)
                    {
                        if (_dc.AllowDBNull == false)
                        {
                            if (_dc.DataType == typeof(Decimal))
                                _lineitemRow[_dc.ColumnName] = 0;
                            else if (_dc.DataType == typeof(String))
                                _lineitemRow[_dc.ColumnName] = "";
                        }
                    }

                    _lineitemRow["issueId"] = issue_id;

                    XPathNodeIterator _iter1 = _nav.SelectChildren(XPathNodeType.Element);
                    while (_iter1.MoveNext() == true)
                    {
                        var _format = "";

                        var _name = _iter1.Current.Name;
                        {
                            if (_name == "SequenceNumeric")
                                _name = "seqNo";
                            else if (_name == "DescriptionText")
                                _name = "description";
                            else if (_name == "InvoiceAmount")
                                _name = "invoiceAmount";
                            else if (_name == "ChargeableUnitQuantity")
                                _name = "quantity";
                            else if (_name == "InformationText")
                                _name = "information";
                            else if (_name == "NameText")
                                _name = "itemName";
                            else if (_name == "PurchaseExpiryDateTime")
                            {
                                _name = "purchaseDate";
                                _format = "yyyyMMdd";
                            }
                            else if (_name == "TotalTax" && _iter1.Current.HasChildren == true)
                            {
                                XPathNodeIterator _iter2 = _iter1.Current.SelectChildren(XPathNodeType.Element);
                                while (_iter2.MoveNext() == true)
                                {
                                    _format = "";

                                    _name = _iter2.Current.Name;
                                    if (_name == "CalculatedAmount")
                                        _name = "taxAmount";

                                    this.AssignFieldValue(_lineitemRow, _name, _iter2.Current.Value, _format);
                                }

                                _name = "";
                            }
                            else if (_name == "UnitPrice" && _iter1.Current.HasChildren == true)
                            {
                                XPathNodeIterator _iter2 = _iter1.Current.SelectChildren(XPathNodeType.Element);
                                while (_iter2.MoveNext() == true)
                                {
                                    _format = "";

                                    _name = _iter2.Current.Name;
                                    if (_name == "UnitAmount")
                                        _name = "unitPrice";

                                    this.AssignFieldValue(_lineitemRow, _name, _iter2.Current.Value, _format);
                                }

                                _name = "";
                            }
                        }

                        this.AssignFieldValue(_lineitemRow, _name, _iter1.Current.Value, _format);
                    }

                    foreach (DataColumn _dc in line_item_table.Columns)
                    {
                        if (_dc.AllowDBNull == false)
                        {
                            if (_lineitemRow[_dc.ColumnName] == DBNull.Value)
                                throw new ProxyException(String.Format("'{0}' is null in the lineitem table.", _dc.ColumnName));
                        }
                    }

                    line_item_table.Rows.Add(_lineitemRow);
                }
            } while (_nav.MoveToNext(XPathNodeType.Element));
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // <TaxInvoice>
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public DataSet TaxInvoice()
        {
            DataRow _invoiceRow = this.InvoiceTbl.NewRow();
            {
                foreach (DataColumn _dc in this.InvoiceTbl.Columns)
                {
                    if (_dc.AllowDBNull == false)
                    {
                        if (_dc.DataType == typeof(Decimal))
                            _invoiceRow[_dc.ColumnName] = 0;
                        else if (_dc.DataType == typeof(String))
                            _invoiceRow[_dc.ColumnName] = "";
                    }
                }

                this.ExchangedDocument(_invoiceRow);
                this.TaxInvoiceDocument(_invoiceRow);
                this.TaxInvoiceTradeSettlement(_invoiceRow);

                _invoiceRow["isIssued"] = "T";

                foreach (DataColumn _dc in this.InvoiceTbl.Columns)
                {
                    if (_dc.AllowDBNull == false)
                    {
                        if (_invoiceRow[_dc.ColumnName] == DBNull.Value)
                            throw new ProxyException(String.Format("'{0}' is null in the invoice table.", _dc.ColumnName));
                    }
                }

                this.InvoiceTbl.Rows.Add(_invoiceRow);
            }

            var _issueId = Convert.ToString(_invoiceRow["issueId"]);
            this.TaxInvoiceTradeLineItem(this.LineItemTbl, _issueId);

            return this.InvoiceSet;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // <TaxPurchase>
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public DataSet TaxPurchase()
        {
            DataRow _purchaseRow = this.PurchaseTbl.NewRow();
            {
                foreach (DataColumn _dc in this.PurchaseTbl.Columns)
                {
                    if (_dc.AllowDBNull == false)
                    {
                        if (_dc.DataType == typeof(Decimal))
                            _purchaseRow[_dc.ColumnName] = 0;
                        else if (_dc.DataType == typeof(String))
                            _purchaseRow[_dc.ColumnName] = "";
                    }
                }

                this.ExchangedDocument(_purchaseRow);
                this.TaxInvoiceDocument(_purchaseRow);
                this.TaxInvoiceTradeSettlement(_purchaseRow);

                _purchaseRow["creator"] = "M";          // default creator is receiving mail.

                foreach (DataColumn _dc in this.PurchaseTbl.Columns)
                {
                    if (_dc.AllowDBNull == false)
                    {
                        if (_purchaseRow[_dc.ColumnName] == DBNull.Value)
                            throw new ProxyException(String.Format("'{0}' is null in the purchase table.", _dc.ColumnName));
                    }
                }

                this.PurchaseTbl.Rows.Add(_purchaseRow);
            }

            var _issueId = Convert.ToString(_purchaseRow["issueId"]);
            this.TaxInvoiceTradeLineItem(this.PurchaseItemTbl, _issueId);

            return this.InvoiceSet;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}