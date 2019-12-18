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

using OdinSdk.BaseLib.Security;
using System;
using System.Data;

namespace OpenTax.Engine.Library.Data.MSSQL
{
    /// <summary>
    ///
    /// </summary>
    public static class MsDataArgs
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //multi task crash string c#
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_xmlarg"></param>
        /// <returns></returns>
        public static MsDatParameters ToArguments(this String p_xmlarg)
        {
            var _result = new MsDatParameters();
            _result.PutXmlString(p_xmlarg);
            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_dbps"></param>
        /// <returns></returns>
        public static string ToXmlString(this MsDatParameters p_dbps)
        {
            return ((MsDatParameters)p_dbps.Clone()).GetXmlString();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_package"></param>
        /// <returns></returns>
        public static MsDatParameters ToParameters(this XmlPackage p_package)
        {
            var _result = new MsDatParameters();

            _result.PutXmlPackage(p_package);

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_drow"></param>
        /// <param name="p_columnName"></param>
        /// <returns></returns>
        public static bool ToBoolean(this DataRow p_drow, string p_columnName)
        {
            var _result = false;

            if (p_drow != null)
            {
                if (p_drow.Table.Columns.Contains(p_columnName) == true)
                {
                    object _value = p_drow[p_columnName];

                    if (_value is Boolean)
                        _result = (bool)_value;
                    else if (_value is String)
                        _result = _value.ToString() == "T";
                }
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_drowView"></param>
        /// <param name="p_columnName"></param>
        /// <returns></returns>
        public static bool ToBoolean(this DataRowView p_drowView, string p_columnName)
        {
            var _result = false;

            if (p_drowView != null)
            {
                if (p_drowView.Row.Table.Columns.Contains(p_columnName) == true)
                {
                    object _value = p_drowView[p_columnName];

                    if (_value is Boolean)
                        _result = (bool)_value;
                    else if (_value is String)
                        _result = _value.ToString() == "T";
                }
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_drow"></param>
        /// <param name="p_columnName"></param>
        /// <returns></returns>
        public static decimal ToDecimal(this DataRow p_drow, string p_columnName)
        {
            decimal _result = 0;

            if (p_drow != null)
            {
                if (p_drow.Table.Columns.Contains(p_columnName) == true)
                {
                    object _value = p_drow[p_columnName];

                    if (_value is Decimal)
                        _result = (decimal)_value;
                    else if (_value is Int16 || _value is Int32 || _value is Int64)
                        _result = Convert.ToDecimal(_value);
                    else if (_value is String)
                        _result = Convert.ToDecimal(_value);
                }
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_dbps"></param>
        /// <param name="p_kindOfPacking"></param>
        /// <returns></returns>
        public static XmlPackage ToXmlPackage(this MsDatParameters p_dbps, MKindOfPacking p_kindOfPacking = MKindOfPacking.Encrypted)
        {
            return ((MsDatParameters)p_dbps.Clone()).GetXmlPackage(p_kindOfPacking);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_dbp"></param>
        /// <returns></returns>
        public static bool ToBoolean(this MsDatParameter p_dbp)
        {
            var _result = false;

            if (p_dbp != null && p_dbp.Value != null)
            {
                if (p_dbp.Value is Boolean)
                    _result = (bool)p_dbp.Value;
                else if (p_dbp.Value is String && p_dbp.Type == SqlDbType.NVarChar)
                    _result = p_dbp.Value.ToString() == "T";
            }

            return _result;
        }

        /// <summary>
        /// 만약 string 값을 decimal로 변환 하려고 하는 경우, SqlDbType이 Decimal이어야 합니다.
        /// </summary>
        /// <param name="p_dbp"></param>
        /// <returns></returns>
        public static decimal ToDecimal(this MsDatParameter p_dbp)
        {
            decimal _result = 0;

            if (p_dbp != null && p_dbp.Value != null)
            {
                if (p_dbp.Value is Decimal)
                    _result = (decimal)p_dbp.Value;
                else if (p_dbp.Value is Int16 || p_dbp.Value is Int32 || p_dbp.Value is Int64)
                    _result = Convert.ToDecimal(p_dbp.Value);
                else if (p_dbp.Value is String && p_dbp.Type == SqlDbType.Decimal)
                    _result = Convert.ToDecimal(p_dbp.Value);
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_dbp"></param>
        /// <returns></returns>
        public static Int32 ToInt32(this MsDatParameter p_dbp)
        {
            Int32 _result = 0;

            if (p_dbp != null && p_dbp.Value != null)
            {
                if (p_dbp.Value is Int32)
                    _result = (Int32)p_dbp.Value;
                else if (p_dbp.Value is Int16 || p_dbp.Value is Int32 || p_dbp.Value is Int64)
                    _result = Convert.ToInt32(p_dbp.Value);
                else if (p_dbp.Value is String && p_dbp.Type == SqlDbType.Decimal)
                    _result = Convert.ToInt32(p_dbp.Value);
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_dbp"></param>
        /// <returns></returns>
        public static Double ToDouble(this MsDatParameter p_dbp)
        {
            Double _result = 0;

            if (p_dbp != null && p_dbp.Value != null)
            {
                if (p_dbp.Value is Double)
                    _result = (Double)p_dbp.Value;
                else
                    _result = Convert.ToDouble(p_dbp.Value);
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_dbp"></param>
        /// <returns></returns>
        public static Int64 ToInt64(this MsDatParameter p_dbp)
        {
            Int64 _result = 0;

            if (p_dbp != null && p_dbp.Value != null)
            {
                if (p_dbp.Value is Int64)
                    _result = (Int64)p_dbp.Value;
                else if (p_dbp.Value is Int16 || p_dbp.Value is Int32 || p_dbp.Value is Int64)
                    _result = Convert.ToInt64(p_dbp.Value);
                else if (p_dbp.Value is String && p_dbp.Type == SqlDbType.Decimal)
                    _result = Convert.ToInt64(p_dbp.Value);
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_dbp"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this MsDatParameter p_dbp)
        {
            DateTime _result = DateTime.MinValue;

            if (p_dbp != null && p_dbp.Value != null)
            {
                if (p_dbp.Value is DateTime)
                    _result = (DateTime)p_dbp.Value;
                else if (p_dbp.Value is String && p_dbp.Type == SqlDbType.DateTime)
                    _result = Convert.ToDateTime(p_dbp.Value);
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}