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

using OdinSdk.OdinLib.Security;
using System;
using System.Data;

namespace OdinSdk.OdinLib.Data.MSSQL
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
        /// <param name="xml_params"></param>
        /// <returns></returns>
        public static MsDatParameters ToArguments(this String xml_params)
        {
            var _result = new MsDatParameters();
            _result.PutXmlString(xml_params);
            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="db_params"></param>
        /// <returns></returns>
        public static string ToXmlString(this MsDatParameters db_params)
        {
            return ((MsDatParameters)db_params.Clone()).GetXmlString();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public static MsDatParameters ToParameters(this XmlPackage package)
        {
            var _result = new MsDatParameters();

            _result.PutXmlPackage(package);

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="data_row"></param>
        /// <param name="column_name"></param>
        /// <returns></returns>
        public static bool ToBoolean(this DataRow data_row, string column_name)
        {
            var _result = false;

            if (data_row != null)
            {
                if (data_row.Table.Columns.Contains(column_name) == true)
                {
                    object _value = data_row[column_name];

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
        /// <param name="data_row_view"></param>
        /// <param name="column_name"></param>
        /// <returns></returns>
        public static bool ToBoolean(this DataRowView data_row_view, string column_name)
        {
            var _result = false;

            if (data_row_view != null)
            {
                if (data_row_view.Row.Table.Columns.Contains(column_name) == true)
                {
                    object _value = data_row_view[column_name];

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
        /// <param name="data_row"></param>
        /// <param name="column_name"></param>
        /// <returns></returns>
        public static decimal ToDecimal(this DataRow data_row, string column_name)
        {
            decimal _result = 0;

            if (data_row != null)
            {
                if (data_row.Table.Columns.Contains(column_name) == true)
                {
                    object _value = data_row[column_name];

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
        /// <param name="db_params"></param>
        /// <param name="kind_of_packing"></param>
        /// <returns></returns>
        public static XmlPackage ToXmlPackage(this MsDatParameters db_params, MKindOfPacking kind_of_packing = MKindOfPacking.Encrypted)
        {
            return ((MsDatParameters)db_params.Clone()).GetXmlPackage(kind_of_packing);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="db_param"></param>
        /// <returns></returns>
        public static bool ToBoolean(this MsDatParameter db_param)
        {
            var _result = false;

            if (db_param != null && db_param.Value != null)
            {
                if (db_param.Value is Boolean)
                    _result = (bool)db_param.Value;
                else if (db_param.Value is String && db_param.Type == SqlDbType.NVarChar)
                    _result = db_param.Value.ToString() == "T";
            }

            return _result;
        }

        /// <summary>
        /// 만약 string 값을 decimal로 변환 하려고 하는 경우, SqlDbType이 Decimal이어야 합니다.
        /// </summary>
        /// <param name="db_param"></param>
        /// <returns></returns>
        public static decimal ToDecimal(this MsDatParameter db_param)
        {
            decimal _result = 0;

            if (db_param != null && db_param.Value != null)
            {
                if (db_param.Value is Decimal)
                    _result = (decimal)db_param.Value;
                else if (db_param.Value is Int16 || db_param.Value is Int32 || db_param.Value is Int64)
                    _result = Convert.ToDecimal(db_param.Value);
                else if (db_param.Value is String && db_param.Type == SqlDbType.Decimal)
                    _result = Convert.ToDecimal(db_param.Value);
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="db_param"></param>
        /// <returns></returns>
        public static Int32 ToInt32(this MsDatParameter db_param)
        {
            Int32 _result = 0;

            if (db_param != null && db_param.Value != null)
            {
                if (db_param.Value is Int32)
                    _result = (Int32)db_param.Value;
                else if (db_param.Value is Int16 || db_param.Value is Int32 || db_param.Value is Int64)
                    _result = Convert.ToInt32(db_param.Value);
                else if (db_param.Value is String && db_param.Type == SqlDbType.Decimal)
                    _result = Convert.ToInt32(db_param.Value);
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="db_param"></param>
        /// <returns></returns>
        public static Double ToDouble(this MsDatParameter db_param)
        {
            Double _result = 0;

            if (db_param != null && db_param.Value != null)
            {
                if (db_param.Value is Double)
                    _result = (Double)db_param.Value;
                else
                    _result = Convert.ToDouble(db_param.Value);
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="db_param"></param>
        /// <returns></returns>
        public static Int64 ToInt64(this MsDatParameter db_param)
        {
            Int64 _result = 0;

            if (db_param != null && db_param.Value != null)
            {
                if (db_param.Value is Int64)
                    _result = (Int64)db_param.Value;
                else if (db_param.Value is Int16 || db_param.Value is Int32 || db_param.Value is Int64)
                    _result = Convert.ToInt64(db_param.Value);
                else if (db_param.Value is String && db_param.Type == SqlDbType.Decimal)
                    _result = Convert.ToInt64(db_param.Value);
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="db_param"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this MsDatParameter db_param)
        {
            DateTime _result = DateTime.MinValue;

            if (db_param != null && db_param.Value != null)
            {
                if (db_param.Value is DateTime)
                    _result = (DateTime)db_param.Value;
                else if (db_param.Value is String && db_param.Type == SqlDbType.DateTime)
                    _result = Convert.ToDateTime(db_param.Value);
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}