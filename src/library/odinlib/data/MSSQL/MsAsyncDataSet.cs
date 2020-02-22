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

using OdinSdk.OdinLib.Configuration;
using System;
using System.Data;
using System.IO;
using System.Text;

//#pragma warning disable 1589, 1591

namespace OdinSdk.OdinLib.Data.MSSQL
{
    public class MsAsyncDataSet
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        public MsAsyncDataSet(string file_path, string file_name, int rows_per_page, string extension)
        {
            if (rows_per_page > 0)
                RowsPerPage = rows_per_page;

            if (String.IsNullOrEmpty(file_path) == true)
                file_path = Path.Combine(CfgHelper.SNG.GetWorkingFolder("AsyncDS"), c_rootpath);

            m_filepath = file_path;
            if (Directory.Exists(m_filepath) == false)
                Directory.CreateDirectory(m_filepath);

            m_filename = file_name;
            m_extension = extension;
            m_fullpath = Path.Combine(m_filepath, m_filename);

            m_data_helper = new Lazy<MsDataHelper>(GetDataHelper);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private MsDataHelper GetDataHelper()
        {
            return new MsDataHelper();
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private const string c_rootpath = "AsyncDataSet";

        private const string c_schemaext = "xsd";
        private const string c_tempraryext = "tmp";

        private string m_fullpath = "";

        public string FullPath
        {
            get
            {
                return m_fullpath;
            }
        }

        private string m_filepath = "";

        public string FilePath
        {
            get
            {
                return m_filepath;
            }
        }

        private string m_filename = "";

        public string FileName
        {
            get
            {
                return m_filename;
            }
        }

        private string m_extension = "";

        public string Extension
        {
            get
            {
                return m_extension;
            }
        }

        private Lazy<int> m_rowsperpage = new Lazy<int>(() => 128);

        public int RowsPerPage
        {
            get
            {
                return m_rowsperpage.Value;
            }
            set
            {
                m_rowsperpage = new Lazy<int>(() =>
                {
                    return value;
                });
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private Lazy<MsDataHelper> m_data_helper;

        private OdinSdk.OdinLib.Data.MSSQL.MsDataHelper DatHelper
        {
            get
            {
                return m_data_helper.Value;
            }
        }

        private Lazy<System.Data.DataSet> m_datStore = new Lazy<DataSet>();

        private System.Data.DataSet DatStore
        {
            get
            {
                return m_datStore.Value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private string _topCommandTextBuild(string sql_text)
        {
            var _result = "";
            var _tops = 0;

            string[] _words = sql_text.Split(' ', '\t', '\r', '\n');
            for (int i = 0; i < _words.Length; i++)
            {
                _result += _words[i] + ' ';
                if (_tops < 1 && _words[i].ToLower() == "select")
                {
                    _result += String.Format("top {0} ", RowsPerPage);
                    _tops++;
                }
            }

            return _result;
        }

        private string _recordCountCommandTextBuild(string command_text)
        {
            var _result = "";
            var _norec = 0;

            string[] _words = command_text.Split(' ', '\t', '\r', '\n');
            for (int i = 0; i < _words.Length; i++)
            {
                if (_norec == 0)
                {
                    if (_words[i].ToLower() == "select")
                    {
                        _result += _words[i] + " count(*) as norec ";
                        _norec++;
                    }
                }
                else
                    if (_norec == 1)
                {
                    if (_words[i].ToLower() == "from")
                    {
                        _result += _words[i] + ' ';
                        _norec++;
                    }
                }
                else
                        if (_norec > 1)
                {
                    _result += _words[i] + ' ';
                    _norec++;
                }
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private void _fillBinarySet(string connection_string, string command_text, MsDatParameters db_params)
        {
            DatStore.Merge(DatHelper.SelectDataSet(connection_string, command_text, db_params));
        }

        private int _lengthBinarySet()
        {
            var _result = 0;

            if (DatStore.Tables.Count > 0)
            {
                foreach (DataColumn _dc in DatStore.Tables[0].Columns)
                {
                    _result += sizeof(Boolean);

                    switch (_dc.DataType.ToString())
                    {
                        case "System.String":
                            _result += _dc.MaxLength * 2;
                            break;

                        case "System.DateTime":
                            _result += sizeof(Int64);
                            break;

                        case "System.Int16":
                            _result += sizeof(Int16);
                            break;

                        case "System.Int32":
                            _result += sizeof(Int32);
                            break;

                        case "System.Int64":
                            _result += sizeof(Int64);
                            break;

                        case "System.Decimal":
                            _result += sizeof(Double);
                            break;

                        case "System.Single":
                            _result += sizeof(Single);
                            break;

                        case "System.Double":
                            _result += sizeof(Double);
                            break;

                        case "System.UInt16":
                            _result += sizeof(UInt16);
                            break;

                        case "System.UInt32":
                            _result += sizeof(UInt32);
                            break;

                        case "System.UInt64":
                            _result += sizeof(UInt64);
                            break;

                        case "System.Char":
                            _result += sizeof(Char);
                            break;

                        case "System.Boolean":
                            _result += sizeof(Boolean);
                            break;
                    }
                }
            }

            return _result;
        }

        private bool _writeBinarySet()
        {
            var _result = false;

            if (DatStore.Tables.Count > 0 && DatStore.Tables[0].Rows.Count > 0)
            {
                _result = true;

                DataTable _dt = DatStore.Tables[0];
                _dt.WriteXmlSchema(Path.ChangeExtension(m_fullpath, c_schemaext));

                var _tempfile = Path.ChangeExtension(m_fullpath, c_tempraryext);
                FileStream _wfs = new FileStream(_tempfile, FileMode.Create, FileAccess.Write);

                for (int _row = 0; _row < _dt.Rows.Count; _row++)
                {
                    var _dr = _dt.Rows[_row];

                    for (int _col = 0; _col < _dt.Columns.Count; _col++)
                    {
                        DataColumn _dc = _dt.Columns[_col];

                        byte[] _bytes = null;
                        var _isnull = true;
                        object _value = null;

                        if (_dr[_col].GetType() != typeof(System.DBNull))
                        {
                            _isnull = false;
                            _value = _dr[_col];
                        }

                        switch (_dc.DataType.ToString())
                        {
                            case "System.String":
                                var _vstr = "";
                                if (_isnull == false)
                                    _vstr = (System.String)_value;

                                _bytes = Encoding.Unicode.GetBytes(_vstr.PadRight(_dc.MaxLength));
                                break;

                            case "System.DateTime":
                                DateTime _vday = DateTime.MinValue;
                                if (_isnull == false)
                                    _vday = (System.DateTime)_value;

                                _bytes = BitConverter.GetBytes(_vday.Ticks);
                                break;

                            case "System.Int16":
                                Int16 _int16 = Int16.MinValue;
                                if (_isnull == false)
                                    _int16 = (System.Int16)_value;

                                _bytes = BitConverter.GetBytes(_int16);
                                break;

                            case "System.Int32":
                                Int32 _int32 = Int32.MinValue;
                                if (_isnull == false)
                                    _int32 = (System.Int32)_value;

                                _bytes = BitConverter.GetBytes(_int32);
                                break;

                            case "System.Int64":
                                Int64 _int64 = Int64.MinValue;
                                if (_isnull == false)
                                    _int64 = (System.Int64)_value;

                                _bytes = BitConverter.GetBytes(_int64);
                                break;

                            case "System.Decimal":
                                Decimal _dcml = Decimal.MinValue;
                                if (_isnull == false)
                                    _dcml = (System.Decimal)_value;

                                _bytes = BitConverter.GetBytes(Convert.ToDouble(_dcml));
                                break;

                            case "System.Single":
                                Single _sngl = Single.MinValue;
                                if (_isnull == false)
                                    _sngl = (System.Single)_value;

                                _bytes = BitConverter.GetBytes(_sngl);
                                break;

                            case "System.Double":
                                Double _dble = Double.MinValue;
                                if (_isnull == false)
                                    _dble = (System.Double)_value;

                                _bytes = BitConverter.GetBytes(_dble);
                                break;

                            case "System.UInt16":
                                UInt16 _ui16 = UInt16.MinValue;
                                if (_isnull == false)
                                    _ui16 = (System.UInt16)_value;

                                _bytes = BitConverter.GetBytes(_ui16);
                                break;

                            case "System.UInt32":
                                UInt32 _ui32 = UInt32.MinValue;
                                if (_isnull == false)
                                    _ui32 = (System.UInt32)_value;

                                _bytes = BitConverter.GetBytes(_ui32);
                                break;

                            case "System.UInt64":
                                UInt64 _ui64 = UInt64.MinValue;
                                if (_isnull == false)
                                    _ui64 = (System.UInt64)_value;

                                _bytes = BitConverter.GetBytes(_ui64);
                                break;

                            case "System.Char":
                                Char _char = Char.MinValue;
                                if (_isnull == false)
                                    _char = (System.Char)_value;

                                _bytes = BitConverter.GetBytes(_char);
                                break;

                            case "System.Boolean":
                                Boolean _bool = false;
                                if (_isnull == false)
                                    _bool = (System.Boolean)_value;

                                _bytes = BitConverter.GetBytes(_bool);
                                break;
                        }

                        _wfs.Write(BitConverter.GetBytes(_isnull), 0, sizeof(bool));
                        _wfs.Write(_bytes, 0, _bytes.Length);
                    }

                    _wfs.Flush();
                }

                _wfs.Close();

                File.Delete(m_fullpath);
                File.Move(_tempfile, m_fullpath);
            }

            return _result;
        }

        private void _readBinarySet(int page_number, out int no_reccord)
        {
            no_reccord = 0;

            if (File.Exists(m_fullpath) == false)
            {
                DatStore.Tables.Clear();
                return;
            }

            if (DatStore.Tables.Count == 0)
            {
                var _schemafile = Path.ChangeExtension(m_fullpath, c_schemaext);
                if (File.Exists(_schemafile) == false)
                {
                    return;
                }

                DatStore.ReadXmlSchema(_schemafile);
            }

            if (DatStore.Tables.Count > 0)
            {
                var _reclen = _lengthBinarySet();
                FileStream _rfs = new FileStream(m_fullpath, FileMode.Open, FileAccess.Read);

                no_reccord = (int)_rfs.Length / _reclen;

                var _lastpage = (int)((_rfs.Length - 1) / (long)(_reclen * RowsPerPage) + 1);
                if (page_number < 1)
                    page_number = 1;
                else if (page_number > _lastpage)
                    page_number = _lastpage;

                _rfs.Seek((page_number - 1) * _reclen * RowsPerPage, SeekOrigin.Begin);

                DataTable _dt = DatStore.Tables[0];
                _dt.Rows.Clear();

                for (int _row = 0; _row < RowsPerPage; _row++)
                {
                    var _dr = _dt.NewRow();
                    var _modified = true;

                    for (int _col = 0; _col < _dt.Columns.Count; _col++)
                    {
                        DataColumn _dc = _dt.Columns[_col];

                        byte[] _bytes = new byte[sizeof(Boolean)];
                        if (_rfs.Read(_bytes, 0, _bytes.Length) <= 0)
                        {
                            _modified = false;
                            break;
                        }

                        var _isnull = BitConverter.ToBoolean(_bytes, 0);
                        object _value = System.DBNull.Value;

                        switch (_dc.DataType.ToString())
                        {
                            case "System.String":
                                _bytes = new byte[_dc.MaxLength * 2];
                                if (_rfs.Read(_bytes, 0, _bytes.Length) > 0)
                                {
                                    if (_isnull == false)
                                        _value = (object)Encoding.Unicode.GetString(_bytes, 0, _bytes.Length).Trim();
                                }
                                break;

                            case "System.DateTime":
                                _bytes = new byte[sizeof(Int64)];
                                if (_rfs.Read(_bytes, 0, _bytes.Length) > 0)
                                {
                                    if (_isnull == false)
                                        _value = (object)(new DateTime(BitConverter.ToInt64(_bytes, 0)));
                                }
                                break;

                            case "System.Int16":
                                _bytes = new byte[sizeof(Int16)];
                                if (_rfs.Read(_bytes, 0, _bytes.Length) > 0)
                                {
                                    if (_isnull == false)
                                        _value = (object)BitConverter.ToInt16(_bytes, 0);
                                }
                                break;

                            case "System.Int32":
                                _bytes = new byte[sizeof(Int32)];
                                if (_rfs.Read(_bytes, 0, _bytes.Length) > 0)
                                {
                                    if (_isnull == false)
                                        _value = (object)BitConverter.ToInt32(_bytes, 0);
                                }
                                break;

                            case "System.Int64":
                                _bytes = new byte[sizeof(Int64)];
                                if (_rfs.Read(_bytes, 0, _bytes.Length) > 0)
                                {
                                    if (_isnull == false)
                                        _value = (object)BitConverter.ToInt64(_bytes, 0);
                                }
                                break;

                            case "System.Decimal":
                                _bytes = new byte[sizeof(Double)];
                                if (_rfs.Read(_bytes, 0, _bytes.Length) > 0)
                                {
                                    if (_isnull == false)
                                        _value = (object)Convert.ToDecimal(BitConverter.ToDouble(_bytes, 0));
                                }
                                break;

                            case "System.Single":
                                _bytes = new byte[sizeof(Single)];
                                if (_rfs.Read(_bytes, 0, _bytes.Length) > 0)
                                {
                                    if (_isnull == false)
                                        _value = (object)BitConverter.ToSingle(_bytes, 0);
                                }
                                break;

                            case "System.Double":
                                _bytes = new byte[sizeof(Double)];
                                if (_rfs.Read(_bytes, 0, _bytes.Length) > 0)
                                {
                                    if (_isnull == false)
                                        _value = (object)BitConverter.ToDouble(_bytes, 0);
                                }
                                break;

                            case "System.UInt16":
                                _bytes = new byte[sizeof(UInt16)];
                                if (_rfs.Read(_bytes, 0, _bytes.Length) > 0)
                                {
                                    if (_isnull == false)
                                        _value = (object)BitConverter.ToUInt16(_bytes, 0);
                                }
                                break;

                            case "System.UInt32":
                                _bytes = new byte[sizeof(UInt32)];
                                if (_rfs.Read(_bytes, 0, _bytes.Length) > 0)
                                {
                                    if (_isnull == false)
                                        _value = (object)BitConverter.ToUInt32(_bytes, 0);
                                }
                                break;

                            case "System.UInt64":
                                _bytes = new byte[sizeof(UInt64)];
                                if (_rfs.Read(_bytes, 0, _bytes.Length) > 0)
                                {
                                    if (_isnull == false)
                                        _value = (object)BitConverter.ToUInt64(_bytes, 0);
                                }
                                break;

                            case "System.Char":
                                _bytes = new byte[sizeof(Char)];
                                if (_rfs.Read(_bytes, 0, _bytes.Length) > 0)
                                {
                                    if (_isnull == false)
                                        _value = (object)BitConverter.ToChar(_bytes, 0);
                                }
                                break;

                            case "System.Boolean":
                                _bytes = new byte[sizeof(Boolean)];
                                if (_rfs.Read(_bytes, 0, _bytes.Length) > 0)
                                {
                                    if (_isnull == false)
                                        _value = (object)BitConverter.ToBoolean(_bytes, 0);
                                }
                                break;
                        }

                        _dr[_col] = _value;
                    }

                    if (_modified == false)
                        break;

                    _dt.Rows.Add(_dr);
                }

                _rfs.Close();
            }
        }

        private void _cleanBinarySet()
        {
            string[] _filelist = Directory.GetFiles(m_filepath, "*." + m_extension);
            foreach (string _filename in _filelist)
            {
                TimeSpan _timediff = CUnixTime.UtcNow.TimeOfDay - Directory.GetCreationTime(_filename).TimeOfDay;
                if (_timediff.TotalMinutes > 30)
                {
                    File.Delete(_filename);
                    File.Delete(Path.ChangeExtension(_filename, c_schemaext));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        public DataSet SelectTopSet(string connection_string, string command_text, MsDatParameters db_params)
        {
            _fillBinarySet(connection_string, _topCommandTextBuild(command_text), db_params);
            return DatStore;
        }

        public int RecordCount(string connection_string, string command_text, MsDatParameters db_params)
        {
            _fillBinarySet(connection_string, _recordCountCommandTextBuild(command_text), db_params);
            return (int)DatStore.Tables[0].Rows[0][0];
        }

        public DataSet WriteBinary(string connection_string, string command_text, MsDatParameters db_params, out int no_reccord)
        {
            _cleanBinarySet();
            _fillBinarySet(connection_string, command_text, db_params);

            no_reccord = 0;
            if (_writeBinarySet() == true)
                _readBinarySet(1, out no_reccord);

            return DatStore;
        }

        public DataSet ReadBinary(int page_number, out int no_reccord)
        {
            _readBinarySet(page_number, out no_reccord);
            return DatStore;
        }

        public void CleanBinary()
        {
            _cleanBinarySet();
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}