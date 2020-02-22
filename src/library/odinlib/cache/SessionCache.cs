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

namespace OdinSdk.OdinLib.Caching
{
    /// <summary>
    ///
    /// </summary>
    public class SessionCache
    {
        #region Fields and Properties

        private string _uniqueId;

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get
            {
                return HttpRuntime.Cache[CreateKey(key)];
            }
            set
            {
                Insert(key, value);
            }
        }

        #endregion Fields and Properties

        #region Constructors

        /// <summary>
        ///
        /// </summary>
        public SessionCache()
        {
            if (HttpContext.Current == null)
            {
                Init(Guid.NewGuid().ToString());
            }
            else
            {
                Init(HttpContext.Current.Session.SessionID);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="uniqueId"></param>
        public SessionCache(string uniqueId)
        {
            Init(uniqueId);
        }

        private void Init(string uniqueId)
        {
            if (String.IsNullOrEmpty(uniqueId))
            {
                throw new ArgumentNullException("uniqueId");
            }
            _uniqueId = uniqueId;
        }

        #endregion Constructors

        #region Inserts

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public void Insert(string key, object data)
        {
            HttpRuntime.Cache.Insert(CreateKey(key), data);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="dependency"></param>
        public void Insert(string key, object data, CacheDependency dependency)
        {
            HttpRuntime.Cache.Insert(CreateKey(key), data, dependency);
        }

        //more inserts

        #endregion Inserts

        #region Private Methods

        private string CreateKey(string key)
        {
            if (String.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            return String.Format("{0}:{1}", key, _uniqueId);
        }

        #endregion Private Methods
    }
}