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

namespace OdinSdk.OdinLib.Caching
{
    /// <summary>
    /// This class contains all data important to define an item stored in the cache. It holds both the key and
    /// value specified by the user, as well as housekeeping information used internally by this block. It is public,
    /// rather than internal, to allow block extenders access to it inside their own implementations of IBackingStore.
    /// </summary>
    public class CacheItem
    {
        // User-provided data
        private string key;

        private object data;

        // Internal housekeeping information
        private DateTime lastAccessedTime;

        private bool eligibleForScavenging;

        /// <summary>
        /// Constructs a fully formed CacheItem.
        /// </summary>
        /// <param name="key">Key identifying this CacheItem</param>
        /// <param name="value">Value to be stored. May be null.</param>
        public CacheItem(string key, object value)
        {
            Initialize(key, value);

            TouchedByUserAction(false);
        }

        /// <summary>
        /// Constructs a fully formed CacheItem. This constructor is to be used when restoring an existing
        /// CacheItem from the backing store. As such, it does not generate its own Guid for this instance,
        /// but allows the guid to be passed in, as read from the backing store.
        /// </summary>
        /// <param name="lastAccessedTime">Time this CacheItem last accessed by user.</param>
        /// <param name="key">Key provided  by the user for this cache item. May not be null.</param>
        /// <param name="value">Value to be stored. May be null.</param>
        public CacheItem(DateTime lastAccessedTime, string key, object value)
        {
            Initialize(key, value);

            TouchedByUserAction(false, lastAccessedTime);
        }

        /// <summary>
        /// Replaces the internals of the current cache item with the given new values. This is strictly used in the Cache
        /// class when adding a new item into the cache. By replacing the item's contents, rather than replacing the item
        /// itself, it allows us to keep a single reference in the cache, simplifying locking.
        /// </summary>
        /// <param name="cacheItemData">Value to be stored. May be null.</param>
        internal void Replace(object cacheItemData)
        {
            Initialize(this.key, cacheItemData);
            TouchedByUserAction(false);
        }

        /// <summary>
        /// Returns the last accessed time.
        /// </summary>
        /// <value>
        /// Gets the last accessed time.
        /// </value>
        /// <remarks>
        /// The set is present for testing purposes only. Should not be called by application code
        /// </remarks>
        public DateTime LastAccessedTime
        {
            get
            {
                return lastAccessedTime;
            }
        }

        /// <summary>
        /// Intended to be used internally only. The value should be true when an item is eligible to be expired.
        /// </summary>
        public bool WillBeExpired
        {
            get;
            set;
        }

        /// <summary>
        /// Intended to be used internally only. The value should be true when an item is eligible for scavenging.
        /// </summary>
        public bool EligibleForScavenging
        {
            get
            {
                return eligibleForScavenging;// && ScavengingPriority != CacheItemPriority.NotRemovable;
            }
        }

        /// <summary>
        /// Returns the cached value of this CacheItem
        /// </summary>
        public object Value
        {
            get
            {
                return data;
            }
        }

        /// <summary>
        /// Returns the key associated with this CacheItem
        /// </summary>
        public string Key
        {
            get
            {
                return key;
            }
        }

        /// <summary>
        /// Evaluates all cacheItemExpirations associated with this cache item to determine if it
        /// should be considered expired. Evaluation stops as soon as any expiration returns true.
        /// </summary>
        /// <returns>True if item should be considered expired, according to policies
        /// defined in this item's cacheItemExpirations.</returns>
        public bool HasExpired()
        {
            //foreach (ICacheItemExpiration expiration in expirations)
            //{
            //    if (expiration.HasExpired())
            //    {
            //        return true;
            //    }
            //}

            return false;
        }

        /// <summary>
        /// Intended to be used internally only. This method is called whenever a CacheItem is touched through the action of a user. It
        /// prevents this CacheItem from being expired or scavenged during an in-progress expiration or scavenging process. It has no effect
        /// on subsequent expiration or scavenging processes.
        /// </summary>
        public void TouchedByUserAction(bool objectRemovedFromCache)
        {
            TouchedByUserAction(objectRemovedFromCache, CUnixTime.UtcNow);
        }

        /// <summary>
        /// Intended to be used internally only. This method is called whenever a CacheItem is touched through the action of a user. It
        /// prevents this CacheItem from being expired or scavenged during an in-progress expiration or scavenging process. It has no effect
        /// on subsequent expiration or scavenging processes.
        /// </summary>
        internal void TouchedByUserAction(bool objectRemovedFromCache, DateTime timestamp)
        {
            lastAccessedTime = timestamp;
            eligibleForScavenging = false;

            WillBeExpired = objectRemovedFromCache ? false : false;
        }

        /// <summary>
        /// Makes the cache item eligible for scavenging.
        /// </summary>
        public void MakeEligibleForScavenging()
        {
            eligibleForScavenging = true;
        }

        /// <summary>
        /// Makes the cache item not eligible for scavenging.
        /// </summary>
        public void MakeNotEligibleForScavenging()
        {
            eligibleForScavenging = false;
        }

        private void Initialize(string cacheItemKey, object cacheItemData)
        {
            key = cacheItemKey;
            data = cacheItemData;
        }

        internal void SetLastAccessedTime(DateTime specificAccessedTime)
        {
            lastAccessedTime = specificAccessedTime;
        }
    }
}