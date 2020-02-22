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
using System.Collections;
using System.Text.RegularExpressions;

namespace OdinSdk.OdinLib.Data.MSSQL
{
    /// <summary>
    /// 쿼리문장을 각 DBMS에 맞게 변환합니다.
    /// 주어지는 원시 쿼리문은 MSSQL에서 사용하는 쿼리문이며
    /// 예외적으로 문자열 연결은 MySQL의 CONCAT을 사용합니다.
    /// </summary>
    public class MsSqlConverter
    {
        //**********************************************************************************************************//
        //
        //**********************************************************************************************************//
        private MsSqlConverter()
        {
        }

        //**********************************************************************************************************//
        //
        //**********************************************************************************************************//
        private static readonly Lazy<MsSqlConverter> m_lzyHelper = new Lazy<MsSqlConverter>(() =>
        {
            return new MsSqlConverter();
        });

        /// <summary></summary>
        public static MsSqlConverter SNG
        {
            get
            {
                return m_lzyHelper.Value;
            }
        }

        //**********************************************************************************************************//
        //
        //**********************************************************************************************************//

        /// <summary>
        /// 쿼리문장을 MySQL에 사용할 수 있도록 변환합니다.
        /// </summary>
        /// <param name="query_string"></param>
        /// <returns></returns>
        public string MS2MySQL(string query_string)
        {
            var _sql = query_string;

            // 00. newid() --> uuid()
            _sql = Regex.Replace(_sql, "newid\\(\\)", "uuid()", RegexOptions.IgnoreCase);

            // 01. isnull --> ifnull
            _sql = Regex.Replace(_sql, "isnull\\(", "ifnull(", RegexOptions.IgnoreCase);

            // 02. 날짜포멧 변환
            // CONVERT(nvarchar(10), alarmstartdate, 121)  ==> date_format(alarmstartdate, '%Y-%m-%d')
            _sql = Regex.Replace(_sql,
                "convert\\(nvarchar\\([0-9]*\\)\\s*,\\s*(?<var>[\\w\\.\\(\\)]+)\\s*,\\s*121\\)",
                "date_format(${var}, '%Y-%m-%d')", RegexOptions.IgnoreCase);

            // 03. getdate() --> now()
            _sql = Regex.Replace(_sql, "getdate\\(\\)", "now()", RegexOptions.IgnoreCase);

            // 04. datalength --> length
            _sql = Regex.Replace(_sql, "datalength\\(", "length(", RegexOptions.IgnoreCase);

            // 05. len --> length
            _sql = Regex.Replace(_sql, "len\\(", "length(", RegexOptions.IgnoreCase);

            // 06. WITH(ROWLOCK)
            _sql = Regex.Replace(_sql, "\\s*with\\s*\\(rowlock\\)\\s*", " ", RegexOptions.IgnoreCase);

            // 07. 암호화 함수 처리
            _sql = Regex.Replace(_sql, "pwdencrypt\\(", "password(", RegexOptions.IgnoreCase);

            _sql = Regex.Replace(_sql,
                "pwdcompare\\(\\s*(?<pstr>[@\\w'.]+)\\s*,\\s*(?<pcol>[\\w'.]+)\\s*\\)",
                "password(${pstr}) = ${pcol}", RegexOptions.IgnoreCase);

            // 08. Exec ==> call
            if (Regex.IsMatch(_sql, "exec\\s+", RegexOptions.IgnoreCase) == true)
            {
                //_sql = ConvertExec2Call(_sql);
                _sql = Regex.Replace(_sql,
                    "exec(?<funname>\\s+\\w*\\s*)(?<param>([\\w@]*,\\s*|[\\w@]*,|[\\w@]*)*)",
                    "call${funname}(${param})", RegexOptions.IgnoreCase);
            }

            // 09. 변수에 값 할당 문제 @var = column ==> column INTO @var
            if (Regex.IsMatch(_sql, "(@\\w+)\\s*=\\s*(\\w+)") == true)
            {
                _sql = Regex.Replace(_sql,
                    "(?<var>@\\w+)\\s*=\\s*(?<col>\\w+)",
                    "${col} INTO ${var}", RegexOptions.IgnoreCase);

                // SET으로 시작하는 변수값 할당문은 다시 원래대로 돌려준다.
                _sql = Regex.Replace(_sql,
                    "set\\s+(?<var>\\w*)\\s{1}into\\s{1}(?<col>@\\w*)",
                    "SET ${col} = ${var}", RegexOptions.IgnoreCase);
            }

            // 10. Convert(int, var) ==> Cast(var as signed)
            _sql = Regex.Replace(_sql,
                "convert\\(\\s*int,\\s*(?<var>(\\w*)|(\\w+\\([\\w\\s,]+\\)))\\)",
                "cast(${var} as signed)", RegexOptions.IgnoreCase);

            // 11. 임시테이블 처리
            if (Regex.IsMatch(_sql, "select\\s+[@\\n\\w\\d.\\s,()'*-]+into\\s+#\\w+", RegexOptions.IgnoreCase) == true)
            {
                _sql = Regex.Replace(_sql,
                    "select\\s+(?<col>[@\\n\\w\\d.\\s,()'*-]+)into\\s+#(?<var>\\w+)",
                    "drop table if exists temp_${var};create temporary table temp_${var} select ${col}", RegexOptions.IgnoreCase);

                _sql = Regex.Replace(_sql,
                    "#(?<var>\\w+)",
                    "temp_${var}", RegexOptions.IgnoreCase);
            }

            // 12. Top --> Limit
            if (Regex.IsMatch(_sql, "top\\s+", RegexOptions.IgnoreCase) == true)
            {
                _sql = ConvertTop2Limit(_sql);
            }

            // Mysql 예약어 `으로 치환
            _sql = Regex.Replace(_sql,
                "\\[(?<word>\\w+)\\]",
                "`${word}`", RegexOptions.IgnoreCase);

            return _sql;
        }

        /// <summary>
        /// top을 limit로 변환합니다.
        /// </summary>
        /// <param name="sql_string"></param>
        /// <returns></returns>
        private string ConvertTop2Limit(string sql_string)
        {
            var _result = "";
            ArrayList _top = new ArrayList();

            string[] _words = sql_string.Split(' ', '\t', '\r', '\n');

            for (int i = 0; i < _words.Length; i++)
            {
                if (_words[i].Length > 0)
                {
                    // ( 이 문자열에 포함되어 있으면 빈문자열을 ArrayList에 채움
                    if (_words[i].Contains("(") == true)
                    {
                        for (int j = 0; j < _words[i].Split('(').Length - 1; j++)
                        {
                            _top.Add("");
                        }
                    }

                    // ) 이 문자열에 포함되어 있으면 ArrayList에서 하나씩 버림
                    // 버릴 때 빈문자열이 아니면 limit 가 들어가야 하는 곳임
                    if (_words[i].Contains(")") == true)
                    {
                        var _temp = "";
                        string[] _tempwords = _words[i].Split(')');

                        for (int j = 0; j < _tempwords.Length; j++)
                        {
                            if (j < _tempwords.Length - 1)
                            {
                                if (_top[_top.Count - 1].ToString() != "")
                                {
                                    _temp += String.Format("{0} limit {1})", _tempwords[j], _top[_top.Count - 1]);
                                    _top.RemoveAt(_top.Count - 1);
                                }
                                else
                                {
                                    _temp += _tempwords[j] + ")";
                                }

                                _top.RemoveAt(_top.Count - 1);
                            }
                            else
                            {
                                _temp += _tempwords[j] + ")";
                            }
                        }

                        _words[i] = _temp.Substring(0, _temp.Length - 1);
                    }

                    // ;이 문자열에 포함되어 있다면 문장의 끝이라는 의미
                    if (_words[i].Contains(";") == true)
                    {
                        if (_top.Count > 0)
                        {
                            _words[i] = _words[i].Substring(0, _words[i].IndexOf(";"))
                                        + " limit " + _top[_top.Count - 1]
                                        + _words[i].Substring(_words[i].IndexOf(";"));
                        }

                        _top.Clear();
                    }

                    if (_words[i].ToLower() == "top")
                    {
                        while (i < _words.Length)
                        {
                            i++;

                            if (_words[i].Length > 0)
                            {
                                _top.Add(_words[i]);
                                break;
                            }
                        }
                    }
                    else
                    {
                        _result += _words[i] + " ";
                    }
                }
            }

            if (_top.Count > 0)
            {
                _result += "limit " + _top[_top.Count - 1];
            }

            return _result;
        }

        //**********************************************************************************************************//
        //
        //**********************************************************************************************************//

        /// <summary>
        /// 쿼리문장을 MSSQL에 사용할 수 있도록 변환합니다.
        /// </summary>
        /// <param name="query_string"></param>
        /// <returns></returns>
        public string MY2MSSQL(string query_string)
        {
            var _sql = query_string;

            // 01. CONCAT을 +로 연결해서 변환
            if (Regex.IsMatch(_sql, "concat\\(", RegexOptions.IgnoreCase) == true)
            {
                _sql = ConvertConcat2Plus(_sql);
            }

            return _sql;
        }

        /// <summary>
        /// CONCAT문을 MSSQL 문장에 맞게 변환합니다.
        /// </summary>
        /// <param name="sql_string"></param>
        /// <returns></returns>
        private string ConvertConcat2Plus(string sql_string)
        {
            // CONCAT으로 문장을 나눈다.
            // 첫번째 문장은 CONCAT이 시작되는 문장이므로 그냥 두면 된다.
            // 두번째 문장부터는 CONCAT이후의 문장이므로 하나씩 파싱해야 한다.
            // CONCAT 이후에 "("의 갯수와 ")"의 갯수를 계산해서 CONCAT안의 내용을 추출한다.
            // CONCAT안의 내용은 ,로 구분되는데 ' 따옴표로 둘러싸여 있는 ,는 구분자가 아니라 그냥 문자열이다.
            // 구분자 ,로 문장을 나눈다.
            // 나눠진 문장을 + 로 연결한다.

            var _result = "";
            string[] _temp = Regex.Split(sql_string, "concat\\(", RegexOptions.IgnoreCase);

            for (int i = 0; i < _temp.Length; i++)
            {
                if (i > 0)
                {
                    var _endck = 1;
                    var _singleQuotationCk = false;
                    var _prevStr = "";
                    var _prevPoint = 0;

                    for (int j = 0; j < _temp[i].Length; j++)
                    {
                        switch (_temp[i][j])
                        {
                            case ')':
                                if (_singleQuotationCk == false)
                                {
                                    _endck--;
                                }
                                break;

                            case '(':
                                if (_singleQuotationCk == false)
                                {
                                    _endck++;
                                }
                                break;

                            case '\'':
                                if (_singleQuotationCk == false)
                                {
                                    _singleQuotationCk = true;
                                }
                                else
                                {
                                    _singleQuotationCk = false;
                                }
                                break;

                            case ',':
                                if (_singleQuotationCk == false)
                                {
                                    if (_endck == 1)
                                    {
                                        if (_prevPoint > 0)
                                        {
                                            _prevStr += "+";
                                        }

                                        _prevStr += String.Format("convert(nvarchar(4000), {0})", _temp[i].Substring(_prevPoint, j - _prevPoint));
                                        _prevPoint = j + 1;
                                    }
                                }
                                break;
                        }

                        if (_endck == 0)
                        {
                            // CONCAT 함수가 닫혔으므로 내용을 정리해서 _result에 담아줍니다.
                            if (_prevPoint > 0)
                            {
                                _prevStr += "+";
                            }

                            _prevStr += String.Format("convert(nvarchar(4000), {0})", _temp[i].Substring(_prevPoint, j - _prevPoint));
                            _result += String.Format("{0}{1}", _prevStr, _temp[i].Substring(j + 1));
                            break;
                        }
                        else if (j == _temp[i].Length - 1)
                        {
                            // CONCAT 함수가 닫히지 않고 새로운 CONCAT이 시작 됩니다.
                            if (_prevPoint > 0)
                            {
                                _prevStr += "+";
                            }

                            _prevStr += String.Format("convert(nvarchar(4000), {0}(", _temp[i].Substring(_prevPoint, j - _prevPoint));
                            _result += String.Format("{0}{1}", _prevStr, _temp[i].Substring(j + 1));
                            break;
                        }
                    }
                }
                else
                {
                    // 첫번째 문장은 그냥 지나갑니다.
                    _result += _temp[i];
                }
            }

            return _result;
        }

        //**********************************************************************************************************//
        //
        //**********************************************************************************************************//
    }
}