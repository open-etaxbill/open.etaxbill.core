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

//#pragma warning disable 1589, 1591

namespace OdinSdk.OdinLib.Data.MSSQL
{
    [Serializable]
    public class MsDatCommand
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        public MsDatCommand()
        {
        }

        public MsDatCommand(string sql_text)
        {
            _bldCommand("", sql_text, null);
        }

        public MsDatCommand(string sql_text, MsDatParameters db_parms)
        {
            _bldCommand("", sql_text, db_parms);
        }

        public MsDatCommand(string name, string sql_text, MsDatParameters db_parms)
        {
            _bldCommand(name, sql_text, db_parms);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private string _splitTableName(string sql_text)
        {
            var _result = "";

            string[] _words = sql_text.Split(' ', '\t', '\r', '\n');
            for (int i = 0; i < _words.Length; i++)
            {
                if (_words[i].ToLower() == "from")
                {
                    for (int j = i + 1; j < _words.Length; j++)
                    {
                        if (_words[j] != "")
                        {
                            _result = _words[j];
                            break;
                        }
                    }

                    break;
                }
            }

            return _result;
        }

        private void _bldCommand(string name, string sql_text, MsDatParameters db_parms)
        {
            if (String.IsNullOrEmpty(name) == true)
            {
                Name = _splitTableName(sql_text);
            }
            else
            {
                Name = name;
            }

            Text = sql_text;

            var _dbps = new MsDatParameters();
            if (db_parms != null)
            {
                foreach (MsDatParameter _dbp in db_parms)
                    _dbps.Add(_dbp.Name, _dbp.FieldType, _dbp.Type, _dbp.Direction, _dbp.Value);
            }

            Value = _dbps;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private string m_name = "";

        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        private string m_text = "";

        public string Text
        {
            get
            {
                return m_text;
            }
            set
            {
                m_text = value;
            }
        }

        private MsDatParameters m_parms = null;

        public MsDatParameters Value
        {
            get
            {
                return m_parms;
            }
            set
            {
                m_parms = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}