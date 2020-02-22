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
using System.Data;
using System.IO;
using System.Text;

namespace OdinSdk.eTaxBill.Security.Issue
{
    /// <summary>
    ///
    /// </summary>
    public class Schema
    {
        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        private Schema()
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//
        private readonly static Lazy<Schema> m_schema = new Lazy<Schema>(() =>
        {
            return new Schema();
        });

        /// <summary></summary>
        public static Schema SNG
        {
            get
            {
                return m_schema.Value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public DataSet GetTaxSchema()
        {
            DataSet _result = new DataSet();

            using (MemoryStream _ms = new MemoryStream())
            {
                var _schema = OdinSdk.eTaxBill.Properties.Resources.taxSchema;
                byte[] _schemaBytes = Encoding.UTF8.GetBytes(_schema);

                _ms.Write(_schemaBytes, 0, _schemaBytes.Length);
                _ms.Seek(0, SeekOrigin.Begin);

                _result.ReadXmlSchema(_ms);
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="table_name"></param>
        /// <returns></returns>
        public DataTable GetTaxDataTable(string table_name)
        {
            DataSet _result = new DataSet(table_name);

            foreach (DataTable _table in this.GetTaxSchema().Tables)
            {
                if (_table.TableName == table_name)
                {
                    _result.Merge(_table);
                    break;
                }
            }

            return _result.Tables[table_name];
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="tax_data_set"></param>
        /// <param name="table_name"></param>
        /// <param name="remain_columns"></param>
        /// <returns></returns>
        public DataTable GetTaxModifiedDataTable(DataSet tax_data_set, string table_name, string[] remain_columns)
        {
            DataTable _target = tax_data_set.Tables[table_name];

            while (true)
            {
                var _nocols = 0;

                foreach (DataColumn _dc in _target.Columns)
                {
                    var _remain = false;

                    foreach (string _name in remain_columns)
                    {
                        if (_dc.ColumnName == _name)
                        {
                            _remain = true;
                            break;
                        }
                    }

                    if (_remain == false)
                    {
                        _target.Columns.Remove(_dc);
                        _nocols++;

                        break;
                    }
                }

                if (_nocols == 0)
                    break;
            }

            return _target;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}