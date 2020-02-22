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
using System.Diagnostics;
using System.IO;
using System.Text;

namespace OdinSdk.OdinLib.Logging
{
    /// <summary>
    ///
    /// </summary>
    public class OFileLog
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 파일로그 객체 생성자, 직접 directory를 지정 해주어야 합니다.
        /// </summary>
        public OFileLog()
        {
        }

        /// <summary>
        /// 파일로그 객체 생성자
        /// </summary>
        /// <param name="directory_name">로그를 저장 할 폴더 위치</param>
        public OFileLog(string directory_name)
        {
            m_directory = directory_name;
        }

        /// <summary>
        /// 파일로그 객체 생성자
        /// </summary>
        /// <param name="directory_name">로그를 저장 할 폴더 위치</param>
        /// <param name="product_id">솔루션명</param>
        public OFileLog(string directory_name, string product_id)
        {
            m_directory = directory_name;
            m_product_id = product_id;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private string m_directory = "";

        /// <summary>
        ///
        /// </summary>
        public string Directory
        {
            get
            {
                if (String.IsNullOrEmpty(m_directory) == true || m_directory.Substring(1, 1) != ":")
                    m_directory = CfgHelper.SNG.GetWorkingFolder("LogFiles");

                return m_directory;
            }
            set
            {
                m_directory = value;
            }
        }

        private string m_product_id = "";

        /// <summary>
        ///
        /// </summary>
        public string ProductId
        {
            get
            {
                return m_product_id;
            }
            set
            {
                m_product_id = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        protected string DirectionToPrefix(string direction)
        {
            var _prefix = "log";
            {
                direction = direction.ToUpper();

                if (direction == "I")
                    _prefix = "imp";
                else if (direction == "E")
                    _prefix = "exc";
                else if (direction == "O")
                    _prefix = "exp";
                else if (direction == "S")
                    _prefix = "svc";
            }

            return _prefix;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected string GetDirectory()
        {
            var _result = Directory;

            if (String.IsNullOrEmpty(ProductId) == true)
                _result = Path.Combine(_result, "default");
            else
                _result = Path.Combine(_result, ProductId);

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected string CheckDirectory()
        {
            var _directory = GetDirectory();

            if (System.IO.Directory.Exists(_directory) == false)
                System.IO.Directory.CreateDirectory(_directory);

            return _directory;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="log_write_time"></param>
        /// <param name="direction"></param>
        /// <param name="message"></param>
        protected void FileWrite(string sender, DateTime log_write_time, string direction, string message)
        {
            FileStream _fs = null;

            try
            {
                var _file = Path.Combine(
                    CheckDirectory(),
                    String.Format("{0}_{1}_{2}.log", sender, DirectionToPrefix(direction), log_write_time.ToString("yyyyMMdd"))
                    );

                _fs = new FileStream(_file, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                using (StreamWriter _sw = new StreamWriter(_fs, Encoding.UTF8))
                {
                    _sw.WriteLine(String.Format("[{0}] {1}", log_write_time.ToLongTimeString(), message));
                    _sw.Flush();
                }
            }
            catch (Exception ex)
            {
                OEventLog.SNG.WriteEntry(String.Format("'{0}', {1}", message, ex.Message), EventLogEntryType.Error);
            }
            finally
            {
                if (_fs != null)
                    _fs.Close();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        public virtual void WriteLog(string sender, string message)
        {
            WriteLog(sender, "L", message);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="direction"></param>
        /// <param name="message"></param>
        public virtual void WriteLog(string sender, string direction, string message)
        {
            WriteLog(sender, CUnixTime.UtcNow, direction, message);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="log_write_time"></param>
        /// <param name="direction"></param>
        /// <param name="message"></param>
        public virtual void WriteLog(string sender, DateTime log_write_time, string direction, string message)
        {
            FileWrite(sender, log_write_time, direction, message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}