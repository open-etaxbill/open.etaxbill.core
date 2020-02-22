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
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;

namespace OdinSdk.eTaxBill.Security.Issue
{
    /// <summary>
    ///
    /// </summary>
    public class Validator
    {
        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        private Validator()
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//
        private readonly static Lazy<Validator> m_validator = new Lazy<Validator>(() =>
        {
            return new Validator();
        });

        /// <summary></summary>
        public static Validator SNG
        {
            get
            {
                return m_validator.Value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 검증 결과 저장 문자열
        //-------------------------------------------------------------------------------------------------------------------------
        private string m_checkResult = "";

        /// <summary>
        /// Property
        /// </summary>
        public string Result
        {
            get
            {
                return this.m_checkResult;
            }
        }

        private XmlReaderSettings m_readerSettings = null;

        /// <summary>
        ///
        /// </summary>
        public XmlReaderSettings ReaderSettings
        {
            get
            {
                if (m_readerSettings == null)
                {
                    m_readerSettings = new XmlReaderSettings();
                    m_readerSettings.ConformanceLevel = ConformanceLevel.Auto;

                    //m_readerSettings.ValidationType = ValidationType.Schema;
                    //m_readerSettings.ProhibitDtd = false;

                    //m_readerSettings.Schemas.Add("urn:kr:or:kec:standard:Tax:ReusableAggregateBusinessInformationEntitySchemaModule:1:0", "http://www.kec.or.kr/standard/Tax/TaxInvoiceSchemaModule_1.0.xsd");
                    //m_readerSettings.Schemas.Add("http://www.w3.org/2000/09/xmldsig#", "http://www.w3.org/TR/2002/REC-xmldsig-core-20020212/xmldsig-core-schema.xsd");

                    m_readerSettings.ValidationEventHandler += new ValidationEventHandler(XmlRead_ValidationEventHandler);
                }

                return m_readerSettings;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 생성된 전자세금계산서 XML 문서의 스키마를 검증하는 함수
        /// </summary>
        /// <param name="xml_string">검증할 XML 파일 경로</param>
        public string DoValidation(string xml_string)
        {
            return this.DoValidation(
                new MemoryStream(Encoding.UTF8.GetBytes(xml_string))
                );
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public string DoValidation(Stream stream)
        {
            var _result = "";

            try
            {
                this.m_checkResult = "";

                XmlReader _reader = XmlReader.Create(stream, this.ReaderSettings);
                while (_reader.Read())
                    ;
            }
            catch (Exception ex)
            {
                this.m_checkResult += ex.Message + Environment.NewLine;
            }
            finally
            {
                _result = this.Result;
            }

            return _result;
        }

        private void XmlRead_ValidationEventHandler(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
                this.m_checkResult += "WARNING: ";
            else if (args.Severity == XmlSeverityType.Error)
                this.m_checkResult += "ERROR: ";

            this.m_checkResult += args.Exception.Message + args.Exception.LineNumber + Environment.NewLine;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="content"></param>
        /// <param name="signatureBytes"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public bool VerifySignature(byte[] content, byte[] signatureBytes, AsymmetricAlgorithm publicKey)
        {
            var _result = false;

            try
            {
                HashAlgorithm _digester;

                RSAPKCS1SignatureDeformatter _deformatter = new RSAPKCS1SignatureDeformatter(publicKey);
                if (publicKey.KeySize == 2048)
                {
                    _digester = new SHA256CryptoServiceProvider();
                    _deformatter.SetHashAlgorithm("SHA256");
                }
                else
                {
                    _digester = new SHA1CryptoServiceProvider();
                    _deformatter.SetHashAlgorithm("SHA1");
                }

                byte[] _digestValue = _digester.ComputeHash(content);
                _result = _deformatter.VerifySignature(_digestValue, signatureBytes);
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return _result;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="invoice_data_set"></param>
        /// <returns></returns>
        public int CheckInvoiceDataTable(DataSet invoice_data_set)
        {
            return this.CheckInvoiceDataTable(
                    invoice_data_set.Tables["TB_eTAX_INVOICE"], invoice_data_set.Tables["TB_eTAX_LINEITEM"]
                );
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="invoice_table"></param>
        /// <param name="line_item_table"></param>
        /// <returns></returns>
        public int CheckInvoiceDataTable(DataTable invoice_table, DataTable line_item_table)
        {
            var _result = 0;

            foreach (DataRow _dr in invoice_table.Rows)
            {
                var _filterExpression = String.Format("issueId='{0}'", _dr["issueId"]);

                var _rowError = this.CheckInvoiceDataTable(_dr, line_item_table.Select(_filterExpression));
                if (String.IsNullOrEmpty(_rowError) == false)
                {
                    _dr.RowError = _rowError;
                    _result++;
                }
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="invoice_row"></param>
        /// <param name="line_item_rows"></param>
        /// <returns></returns>
        public string CheckInvoiceDataTable(DataRow invoice_row, DataRow[] line_item_rows)
        {
            var _result = "";

            try
            {
                var _issueId = Convert.ToString(invoice_row["issueId"]);
                if (String.IsNullOrEmpty(_issueId) == false)
                {
                    DateTime _issueDate = Convert.ToDateTime(invoice_row["issueDate"]);

                    if (_issueId.Substring(0, 8) != _issueDate.ToString("yyyyMMdd"))
                        _result += "INV100: '승인번호(issueId)'의 첫 8자리가 작성일자(issueDate)가 아닙니다." + "\n";
                }
                else
                {
                    _result += "INV102: '승인번호(issueId)'에는 값이 있어야 합니다." + "\n";
                }

                var _typeCode = Convert.ToString(invoice_row["typeCode"]);
                if (String.IsNullOrEmpty(_typeCode) == false)
                {
                    if (_typeCode.Substring(0, 2) == "02" || _typeCode.Substring(0, 2) == "04")
                    {
                        if (String.IsNullOrEmpty(Convert.ToString(invoice_row["amendmentCode"])) == true)
                            _result += "INV110: 수정세금계산서 또는 수정계산서인 경우에는 '수정코드(amendmentCode)'에 값이 있어야 합니다." + "\n";
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(Convert.ToString(invoice_row["amendmentCode"])) == false)
                            _result += "INV112: 세금계산서 또는 계산서인 경우에는 '수정코드(amendmentCode)'에 값이 없어야 합니다." + "\n";
                    }

                    if (_typeCode != "0104" && _typeCode != "0204" && _typeCode != "0304" && _typeCode != "0404")
                    {
                        if (String.IsNullOrEmpty(Convert.ToString(invoice_row["purposeCode"])) == true)
                            _result += "INV114: 수입세금계산서 또는 수입계산서가 아닌 경우에는 '영수/청구구분(purposeCode)'에 값이 있어야 합니다." + "\n";
                    }
                }
                else
                {
                    _result += "INV116: '세금계산서종류(typeCode)'에는 값이 있어야 합니다." + "\n";
                }

                var _description = Convert.ToString(invoice_row["description"]);

                var _invoicerId = Convert.ToString(invoice_row["invoicerId"]);
                if (String.IsNullOrEmpty(_invoicerId) == false)
                {
                    if (_invoicerId.Length != 10)
                        _result += "INV120: '공급사업자(invoicerId)'는 사업자번호인 10자리만 가능 합니다." + "\n";

                    if (String.IsNullOrEmpty(Convert.ToString(invoice_row["invoicerName"])) == true)
                        _result += "INV122: '공급사업자명(invoicerName)'에 값이 있어애 합니다." + "\n";

                    if (String.IsNullOrEmpty(Convert.ToString(invoice_row["invoicerPerson"])) == true)
                        _result += "INV124: '공급사업자대표자명(invoicerPerson)'에 값이 있어야 합니다." + "\n";
                }
                else
                {
                    _result += "INV126: '공급사업자(invoicerId)'에는 값이 있어야 합니다." + "\n";
                }

                var _invoiceeId = Convert.ToString(invoice_row["invoiceeId"]);
                if (String.IsNullOrEmpty(_invoiceeId) == false)
                {
                    if (_invoiceeId.Length != 10 && _invoiceeId.Length != 13)
                        _result += "INV130: '공급받는사업자(invoiceeId)'의 문자 길이가 10자리 또는 13자리가 아닙니다." + "\n";

                    if (String.IsNullOrEmpty(Convert.ToString(invoice_row["invoiceeName"])) == true)
                        _result += "INV132: '공급받는자명(invoiceeName)'에 값이 있어야 합니다." + "\n";

                    if (String.IsNullOrEmpty(Convert.ToString(invoice_row["invoiceePerson"])) == true)
                        _result += "INV134: '공급받는대표자명(invoiceePerson)'에 값이 있어야 합니다." + "\n";

                    var _invoiceeKind = Convert.ToString(invoice_row["invoiceeKind"]);
                    if (String.IsNullOrEmpty(_invoiceeKind) == false)
                    {
                        if (_invoiceeKind == "01")
                        {
                            if (_invoiceeId.Length != 10)
                                _result += "INV136: '공급받는사업자(invoiceeId)'는 '구분코드(invoiceeKind)'가 '01'인 경우 10자리의 값을 가져야 합니다." + "\n";
                        }
                        else if (_invoiceeId.Length != 13)
                        {
                            _result += "INV138: '공급받는사업자(invoiceeId)'는 '구분코드(invoiceeKind)'가 '01'이 아닌 경우 13자리의 값을 가져야 합니다." + "\n";
                        }

                        if (_invoiceeKind == "03")
                        {
                            if (_invoiceeId != "9999999999999")
                                _result += "INV140: '사업자등록구분(invoiceeKind)'이 '03-외국인'인 경우 '공급받는사업자(invoiceeId)'는 '9999999999999'로 기재 하여야 합니다." + "\n";

                            if (String.IsNullOrEmpty(_description) == true)
                                _result += "INV142: '사업자등록구분(invoiceeKind)'이 '03-외국인'인 경우 비고란에 외국인 등록번호 또는 여권번호를 기재 하여야 합니다." + "\n";
                        }
                    }
                    else
                    {
                        _result += "INV144: '사업자등록구분(invoiceeKind)'에는 값이 있어야 합니다." + "\n";
                    }

                    var _invoiceeEMail1 = Convert.ToString(invoice_row["invoiceeEMail1"]);
                    if (String.IsNullOrEmpty(_invoiceeEMail1) == true)
                        _result += "INV146: '공급받는자메일주소(invoiceeEMail1)'에는 값이 있어야 합니다." + "\n";
                }
                else
                {
                    _result += "INV148: '공급받는사업자(invoiceeId)'에는 값이 있어야 합니다." + "\n";
                }

                var _brokerId = Convert.ToString(invoice_row["brokerId"]);
                if (_typeCode == "0103" || _typeCode == "0203" || _typeCode == "0303" || _typeCode == "0403" ||
                    _typeCode == "0105" || _typeCode == "0205" || _typeCode == "0305" || _typeCode == "0405")
                {
                    if (String.IsNullOrEmpty(_brokerId) == true)
                        _result += "INV150: '세금계산서종류(typeCode)'가 위수탁인 경우 '수탁사업자(brokerId)'에 값이 있어야 합니다." + "\n";

                    if (_brokerId.Length != 10)
                        _result += "INV152: '수탁사업자(brokerId)'는 사업자번호인 10자리만 가능 합니다." + "\n";

                    if (String.IsNullOrEmpty(Convert.ToString(invoice_row["brokerName"])) == true)
                        _result += "INV154: '수탁사업자명(brokerName)'에 값이 있어야 합니다." + "\n";

                    if (String.IsNullOrEmpty(Convert.ToString(invoice_row["brokerPerson"])) == true)
                        _result += "INV156: '수탁사업자대표자명(brokerPerson)'에 값이 있어야 합니다." + "\n";
                }
                else
                {
                    if (String.IsNullOrEmpty(_brokerId) == false)
                        _result += "INV158: '세금계산서종류(typeCode)'가 위수탁 또는 위수탁영세가 아닌 경우 '수탁사업자(brokerId)'에 값이 없어야 합니다." + "\n";
                }

                Decimal _chargeTotal = Convert.ToDecimal(invoice_row["chargeTotal"]);
                Decimal _taxTotal = Convert.ToDecimal(invoice_row["taxTotal"]);
                Decimal _grandTotal = Convert.ToDecimal(invoice_row["grandTotal"]);
                if (_grandTotal == (_chargeTotal + _taxTotal))
                {
                    if (Decimal.Truncate(_chargeTotal) != _chargeTotal)
                        _result += "INV200: '공급가액합계(chargeTotal)'는 원 단위 이하 금액을 허용하지 않습니다." + "\n";

                    if (Decimal.Truncate(_taxTotal) != _taxTotal)
                        _result += "INV202: '세액합계(taxTotal)'는 원 단위 이하 금액을 허용하지 않습니다." + "\n";

                    if (Decimal.Truncate(_grandTotal) != _grandTotal)
                        _result += "INV204: '총금액(grandTotal)'은 원 단위 이하 금액을 허용하지 않습니다." + "\n";

                    if (_typeCode.Substring(0, 2) == "01" || _typeCode.Substring(0, 2) == "02")
                    {
                        if (!(_typeCode.Substring(2, 2) == "02" || _typeCode.Substring(2, 2) == "05") && (Decimal.Truncate(_chargeTotal / 10) != 0 && _taxTotal == 0))
                            _result += "INV206: 세금계산서인 경우에는 '세액합계(taxTotal)'에 값이 있어야 합니다." + "\n";
                    }
                    else
                    {
                        if (_taxTotal != 0)
                            _result += "INV208: 계산서인 경우에는 '세액합계(taxTotal)'에 값이 없어야 합니다." + "\n";
                    }

                    Decimal _paidCash = Convert.ToDecimal(invoice_row["paidCash"]);
                    Decimal _paidCheck = Convert.ToDecimal(invoice_row["paidCheck"]);
                    Decimal _paidNote = Convert.ToDecimal(invoice_row["paidNote"]);
                    Decimal _paidCredit = Convert.ToDecimal(invoice_row["paidCredit"]);

                    if (_grandTotal != (_paidCash + _paidCheck + _paidNote + _paidCredit))
                        _result += "INV210: '총금액(grandTotal)은 결제방법['현금(paidCash)','수표(paidCheck),'어음(paidNote)','외상(paidCredit)'] 별 합계와 동일해야 합니다." + "\n";
                }
                else
                {
                    _result += "INV212: '총금액(grandTotal)'이 '공급가액합계(chargeTotal)'와 '세액합계(taxTotal)'을 더한 값이 아닙니다." + "\n";
                }

                if (line_item_rows.Length > 0)
                {
                    Decimal _chargeSum = 0;
                    Decimal _taxSum = 0;

                    var _counter = 0;

                    foreach (DataRow _dr in line_item_rows)
                    {
                        Decimal _quantity = Convert.ToDecimal(_dr["quantity"]);
                        if (Decimal.Truncate(_quantity * 100) / 100 != _quantity)
                        {
                            _result += "INV300: '수량(quantity)'은 원 단위 이하 2자리 까지 가능 합니다." + "\n";
                            break;
                        }

                        Decimal _unitPrice = Convert.ToDecimal(_dr["unitPrice"]);
                        if (Decimal.Truncate(_unitPrice * 100) / 100 != _unitPrice)
                        {
                            _result += "INV302: '단가(unitPrice)'는 원 단위 이하 2자리 까지 가능 합니다." + "\n";
                            break;
                        }

                        Decimal _invoiceAmount = Convert.ToDecimal(_dr["invoiceAmount"]);
                        if (Decimal.Truncate(_invoiceAmount) != _invoiceAmount)
                        {
                            _result += "INV304: '공급가액(invoiceAmount)'은 원 단위 이하 금액을 허용하지 않습니다." + "\n";
                            break;
                        }

                        Decimal _taxAmount = Convert.ToDecimal(_dr["taxAmount"]);
                        if (Decimal.Truncate(_taxAmount) != _taxAmount)
                        {
                            _result += "INV306: '세액(taxAmount)'은 원 단위 이하 금액을 허용하지 않습니다." + "\n";
                            break;
                        }

                        _chargeSum += _invoiceAmount;
                        _taxSum += _taxAmount;

                        _counter++;
                    }

                    if (String.IsNullOrEmpty(_result) == true)
                    {
                        if (_chargeTotal != _chargeSum)
                            _result += "INV308: '공급가액합계(chargeTotal)'는 '상품정보(TradeLineItem)'의 '공급가액(invoiceAmount)'을 합산한 값과 같아야 합니다." + "\n";

                        if (_taxTotal != _taxSum)
                            _result += "INV310: '세액합계(taxTotal)'는 '상품정보(TradeLineItem)'의 '세액(taxAmount)'을 합산한 값과 같아야 합니다." + "\n";
                    }
                }
                else
                {
                    _result += "INV312: '상품정보(TradeLineItem)'는 1개 이상의 레코드를 가지고 있어야 합니다." + "\n";
                }
            }
            catch (Exception ex)
            {
                _result += ex.Message + Environment.NewLine;
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="error_message"></param>
        /// <returns></returns>
        public NameValueCollection GetCollectionFromResult(string error_message)
        {
            NameValueCollection _result = new NameValueCollection();

            string[] _lines = error_message.Split('\n');

            foreach (string _line in _lines)
            {
                if (String.IsNullOrEmpty(_line) == true)
                    continue;

                string[] _cols = _line.Split(':');

                if (_cols.Length != 2)
                {
                    var _value = _result["INV999"];
                    if (String.IsNullOrEmpty(_value) == true)
                        _result.Add("INV999", _line);
                    else
                        _result["INV999"] = String.Format("{0}, {1}", _value, _line);
                }
                else
                {
                    _result.Add(_cols[0], _cols[1].Trim());
                }
            }

            return _result;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        ///// <summary>
        ///// 사업자등록번호 및 주민번호의 적합성을 검증한다.
        ///// </summary>
        ///// <param name="customer_id">사업자번호 또는 주민번호</param>
        ///// <returns></returns>
        //public bool CheckCustomerId(string customer_id)
        //{
        //    var _result = false;

        //    string _customerid = customer_id.Trim();

        //    if (_customerid.Length == 10 || _customerid.Length == 13)
        //    {
        //        if (_customerid.Length == 13)
        //            _result = VerifySSN(_customerid);
        //        else
        //            _result = VerifyBSN(_customerid);
        //    }

        //    return _result;
        //}

        /// <summary>
        /// 주민번호 검증 함수
        /// </summary>
        /// <param name="social_security_no">주민번호</param>
        /// <returns></returns>
        public bool VerifySSN(string social_security_no)
        {
            var _result = true;

            var _socialNo = social_security_no.Trim();

            // 외국인인 경우 그냥 Pass한다.
            while (_socialNo != "9999999999999")
            {
                _result = false;

                // 정규식 패턴 문자열입니다. 6자리의 정수 + [1, 2, 3, 4 중 택 1] + 6자리의 정수
                var _pattern = @"\d{6}[1234]\d{6}";

                // 입력 내역과 정규식 패턴이 일치하면 이 조건문을 통과합니다.
                if (!Regex.Match(_socialNo, _pattern, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace).Success)
                    break;

                // 20세기 출생자와 21세기 출생자를 구분합니다.
                var _birthYear = ('2' >= _socialNo[6]) ? "19" : "20";

                // 연도 두 자리를 추출하여 추가합니다.
                _birthYear += _socialNo.Substring(0, 2);

                // 월 단위 두 자리를 추출합니다.
                var _birthMonth = _socialNo.Substring(2, 2);

                // 일 단위 두 자리를 추출합니다.
                var _birthDate = _socialNo.Substring(4, 2);

                try
                {
                    // 정수로 변환을 시도합니다. 예외가 생기면 catch 블럭으로 이동됩니다.
                    var _bYear = int.Parse(_birthYear);
                    var _bMonth = int.Parse(_birthMonth);
                    var _bDate = int.Parse(_birthDate);

                    // 20세기보다 이전연도, 21세기보다 이후연도,
                    // 월 표기 수가 1보다 작은 값, 월 표기 수가 12보다 큰 값,
                    // 일 표기 수가 1보다 작은 값, 일 표기 수가 12보다 큰 값에 해당되면
                    // catch 블럭으로 이동됩니다.
                    if (_bYear < 1900 || _bYear > 2100 || _bMonth < 1 || _bMonth > 12 || _bDate < 1 || _bDate > 31)
                        break;
                }
                catch
                {
                    break;
                }

                // 고유 알고리즘입니다.
                int[] _buffer = new int[13];
                for (int i = 0; i < _buffer.Length; i++)
                    _buffer[i] = Int32.Parse(_socialNo[i].ToString());

                var _summary = 0;

                int[] _multipliers = new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 2, 3, 4, 5 };
                for (int i = 0; i < 12; i++)
                    _summary += (_buffer[i] *= _multipliers[i]);

                _result = !((11 - (_summary % 11)) % 10 != _buffer[12]);
                break;
            }

            return _result;
        }

        /// <summary>
        /// 사업자등록번호 검증 함수
        /// </summary>
        /// <param name="business_no">사업자번호</param>
        /// <returns></returns>
        public bool VerifyBSN(string business_no)
        {
            var _result = false;

            try
            {
                var _checker = 0;

                _checker += int.Parse(business_no.Substring(0, 1)) * 1;
                _checker += int.Parse(business_no.Substring(1, 1)) * 3;
                _checker += int.Parse(business_no.Substring(2, 1)) * 7;
                _checker += int.Parse(business_no.Substring(3, 1)) * 1;
                _checker += int.Parse(business_no.Substring(4, 1)) * 3;
                _checker += int.Parse(business_no.Substring(5, 1)) * 7;
                _checker += int.Parse(business_no.Substring(6, 1)) * 1;
                _checker += int.Parse(business_no.Substring(7, 1)) * 3;
                _checker += int.Parse(business_no.Substring(8, 1)) * 5 / 10;
                _checker += int.Parse(business_no.Substring(8, 1)) * 5 % 10;
                _checker += int.Parse(business_no.Substring(9, 1));

                if (_checker % 10 == 0)
                    _result = true;
            }
            catch
            {
                _result = false;
            }

            return _result;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}