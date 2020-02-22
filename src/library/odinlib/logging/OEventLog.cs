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

namespace OdinSdk.OdinLib.Logging
{
    /// <summary>
    ///
    /// </summary>
    public class OEventLog
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        public OEventLog()
        {
            m_eventLog = new Lazy<EventLog>(GetEventLog);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="event_source"></param>
        public OEventLog(string event_source)
            : this()
        {
            EventSource = event_source;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private static readonly Lazy<OEventLog> m_lzyHelper = new Lazy<OEventLog>(() =>
        {
            return new OEventLog();
        });

        /// <summary>
        ///
        /// </summary>
        public static OEventLog SNG
        {
            get
            {
                return m_lzyHelper.Value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private EventLog GetEventLog()
        {
            if (!EventLog.SourceExists(EventSource))
                EventLog.CreateEventSource(EventSource, LogName);

            return new EventLog
            {
                Source = EventSource
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // CreateEventSource를 호출할 때 logname이 null 이거나 빈 문자열("")이면 로그는 기본적으로 응용 프로그램
        // 로그가 됩니다. 로컬 컴퓨터에 로그가 없으면 사용자 지정 로그가 만들어지고 응용 프로그램이 해당 로그의
        // Source로 등록됩니다.
        //-----------------------------------------------------------------------------------------------------------------------------
        private string m_eventSrc = "";

        /// <summary>
        ///
        /// </summary>
        public string EventSource
        {
            get
            {
                if (String.IsNullOrEmpty(m_eventSrc) == true)
                    m_eventSrc = "OpenETaxBill_Lib_Event_Src";

                return m_eventSrc;
            }
            set
            {
                m_eventSrc = value;
            }
        }

        private string m_logname = "";

        /// <summary>
        ///
        /// </summary>
        public string LogName
        {
            get
            {
                if (String.IsNullOrEmpty(m_logname) == true)
                    m_logname = "OdinSoft Applicaiotn Event";

                return m_logname;
            }
            set
            {
                m_logname = value;
            }
        }

        private Lazy<EventLog> m_eventLog;

        private EventLog EventLogger
        {
            get
            {
                return m_eventLog.Value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // Write event's log to Application event view
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        public void WriteEntry(string message)
        {
            EventLogger.WriteEntry(message);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        /// <param name="db_type"></param>
        public void WriteEntry(string message, EventLogEntryType db_type)
        {
            EventLogger.WriteEntry(message, db_type);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="previous_method"></param>
        /// <param name="event_message"></param>
        /// <param name="expire"></param>
        public void WriteEntry(System.Reflection.MethodBase previous_method, string event_message, string expire)
        {
            WriteEntry(
                CUnixTime.UtcNow,
                String.Format("{0}.{1}(): {2}", previous_method.ReflectedType.FullName, previous_method.Name, event_message),
                expire
                );
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="occure_time"></param>
        /// <param name="event_message"></param>
        /// <param name="expire"></param>
        public void WriteEntry(DateTime occure_time, string event_message, string expire)
        {
            var _today = (occure_time - TimeSpan.FromDays(3)).ToString("yyyyMMdd");
            if (expire == "live" || _today.CompareTo(expire) <= 0)
            {
                WriteEntry(
                    String.Format("[{0:yyyy/MM/dd} {1:HH:mm:ss:fffffff}] {2}\r\n", occure_time, occure_time, event_message)
                    );
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}