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
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Reflection;
using System.Text;

//#pragma warning disable 1589, 1591

namespace OpenTax.Engine.Library.Data
{
    internal class TableDef
    {
        public string TableName;
        public string PrimaryKey;
    }

    public static class LinqToTable
    {
        private static Type GetCoreType(Type type)
        {
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return Nullable.GetUnderlyingType(type);
            else
                return type;
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> p_source)
        {
            DataTable _result = new DataTable();

            PropertyInfo[] _properties = typeof(T).GetProperties();
            foreach (var _property in _properties)
            {
                _result.Columns.Add(_property.Name, GetCoreType(_property.PropertyType));
            }

            foreach (var _item in p_source)
            {
                DataRow _row = _result.NewRow();

                foreach (var _property in _properties)
                {
                    object _o = _property.GetValue(_item, null);
                    if (_o == null)
                        _o = DBNull.Value;

                    _row[_property.Name] = _o;
                }

                _result.Rows.Add(_row);
            }

            return _result;
        }

        internal static Dictionary<Type, TableDef> __TableDefCache = new Dictionary<Type, TableDef>();

        public static void DeleteByPK<TS, TK>(TK p_primaryKey, DataContext p_dataContext)
                where TS : class
        {
            Table<TS> _table = p_dataContext.GetTable<TS>();
            TableDef _tableDef = GetTableDef<TS>();

            p_dataContext.ExecuteCommand(
                "DELETE FROM [" + _tableDef.TableName + "] WHERE [" + _tableDef.PrimaryKey + "] = {0}",
                p_primaryKey
                );
        }

        public static void DeleteByPK<TS, TK>(TK[] p_primaryKeys, DataContext p_dataContext)
                where TS : class
        {
            Table<TS> _table = p_dataContext.GetTable<TS>();
            TableDef _tableDef = GetTableDef<TS>();

            var _buffer = new StringBuilder();
            _buffer
                .Append("DELETE FROM [")
                .Append(_tableDef.TableName)
                .Append("] WHERE [")
                .Append(_tableDef.PrimaryKey)
                .Append("] IN (");

            for (int i = 0; i < p_primaryKeys.Length; i++)
                _buffer
                    .Append('\'')
                    .Append(p_primaryKeys[i].ToString())
                    .Append('\'')
                    .Append(',');

            _buffer.Length--;
            _buffer.Append(')');

            p_dataContext.ExecuteCommand(_buffer.ToString());
        }

        internal static TableDef GetTableDef<TEntity>()
                where TEntity : class
        {
            var _result = new TableDef();

            Type _entityType = typeof(TEntity);
            lock (__TableDefCache)
            {
                if (!__TableDefCache.ContainsKey(_entityType))
                {
                    object[] _attributes = _entityType.GetCustomAttributes(typeof(TableAttribute), true);

                    var _tableName = (_attributes[0] as TableAttribute).Name;
                    if (_tableName.StartsWith("dbo."))
                        _tableName = _tableName.Substring("dbo.".Length);

                    var _primaryKey = "ID";

                    // Find the property which is the primary key so that we can find the
                    // primary key field name in database
                    foreach (PropertyInfo _property in _entityType.GetProperties())
                    {
                        object[] _columnAtts = _property.GetCustomAttributes(typeof(ColumnAttribute), true);
                        if (_columnAtts.Length > 0)
                        {
                            ColumnAttribute _columnAtt = _columnAtts[0] as ColumnAttribute;
                            if (_columnAtt.IsPrimaryKey)
                                _primaryKey = _columnAtt.Storage.TrimStart('_');
                        }
                    }

                    _result = new TableDef
                    {
                        TableName = _tableName,
                        PrimaryKey = _primaryKey
                    };

                    __TableDefCache.Add(_entityType, _result);
                }
                else
                {
                    _result = __TableDefCache[_entityType];
                }
            }

            return _result;
        }
    }
}