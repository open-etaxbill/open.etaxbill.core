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

using System.Configuration;

//#pragma warning disable 1589, 1591

namespace OdinSdk.OdinLib.Configuration
{
    /// <summary>
    ///
    /// </summary>
    public class MyAppService
    {
        public const string DefaultSectionName = "MyAppService";

        private static string __sectionName = null;

        /// <summary>
        ///
        /// </summary>
        public static string SectionName
        {
            get
            {
                if (__sectionName == null)
                    __sectionName = DefaultSectionName;

                return __sectionName;
            }
            set
            {
                if (__sectionName.Equals(value) == false)
                {
                    __sectionValue = null;
                    __sectionName = value;
                }
            }
        }

        private static ServiceSection __sectionValue = null;

        /// <summary>
        ///
        /// </summary>
        public static ServiceSection Value
        {
            get
            {
                if (__sectionValue == null)
                    __sectionValue = ConfigurationManager.GetSection(SectionName) as ServiceSection;

                return __sectionValue;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static HostElement GetHostByName(string name)
        {
            return Value.servers[name];
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static RedisElement GetRedisByName(string name)
        {
            return Value.redises[name];
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static QueueElement GetQueueByName(string name)
        {
            return Value.queues[name];
        }
    }
}