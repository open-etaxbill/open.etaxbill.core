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
using System.Collections;
using System.Threading;

namespace OdinSdk.OdinLib.Caching
{
    /// <summary>
    /// The real worker of the block. The Cache class is the traffic cop that prevents
    /// resource contention among the different threads in the system. It also will act
    /// as the remoting gateway when that feature is added to the cache.
    /// </summary>
    public class Cache : IDisposable
    {
        private readonly Hashtable __InMemoryCache;

        private const string __AddInProgressFlag = "Dummy variable used to flag temp cache item added during Add";

        /// <summary>
        /// Initialzie a new instance of a <see cref="Cache"/> class with a backing store, and scavenging policy.
        /// </summary>
        public Cache()
        {
            __InMemoryCache = Hashtable.Synchronized(new Hashtable());
        }

        /// <summary>
        /// Gets the count of <see cref="CacheItem"/> objects.
        /// </summary>
        /// <value>
        /// The count of <see cref="CacheItem"/> objects.
        /// </value>
        public int Count
        {
            get
            {
                return __InMemoryCache.Count;
            }
        }

        /// <summary>
        /// Gets the current cache.
        /// </summary>
        /// <returns>
        /// The current cache.
        /// </returns>
        public Hashtable CurrentCacheState
        {
            get
            {
                return (Hashtable)__InMemoryCache.Clone();
            }
        }

        /// <summary>
		/// delete the current cache.
        /// </summary>
        public void Clear()
        {
            lock (__InMemoryCache.SyncRoot)
            {
                __InMemoryCache.Clear();
            }
        }

        /// <summary>
        /// Determines if a particular key is contained in the cache.
        /// </summary>
        /// <param name="indexer">The key to locate.</param>
        /// <returns>
        /// <see langword="true"/> if the key is contained in the cache; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Contains(string indexer)
        {
            indexer = indexer.ToLower();

            ValidateKey(indexer);
            return __InMemoryCache.Contains(indexer);
        }

        /// <summary>
        /// Add a new keyed object to the cache.
        /// </summary>
        /// <param name="indexer">The key of the object.</param>
        /// <param name="object_value">The object to add.</param>
        public void Add(string indexer, object object_value)
        {
            indexer = indexer.ToLower();

            ValidateKey(indexer);

            CacheItem _cacheItemBeforeLock = null;
            var _lockWasSuccessful = false;

            do
            {
                lock (__InMemoryCache.SyncRoot)
                {
                    if (__InMemoryCache.Contains(indexer) == false)
                    {
                        _cacheItemBeforeLock = new CacheItem(indexer, __AddInProgressFlag);
                        __InMemoryCache[indexer] = _cacheItemBeforeLock;
                    }
                    else
                    {
                        _cacheItemBeforeLock = (CacheItem)__InMemoryCache[indexer];
                    }

                    _lockWasSuccessful = Monitor.TryEnter(_cacheItemBeforeLock);
                }

                if (_lockWasSuccessful == false)
                {
                    Thread.Sleep(0);
                }
            } while (_lockWasSuccessful == false);

            try
            {
                _cacheItemBeforeLock.TouchedByUserAction(true);

                CacheItem _newCacheItem = new CacheItem(indexer, object_value);
                try
                {
                    _cacheItemBeforeLock.Replace(object_value);
                    __InMemoryCache[indexer] = _cacheItemBeforeLock;
                }
                catch (Exception ex)
                {
                    __InMemoryCache.Remove(indexer);
                    throw ex;
                }
            }
            finally
            {
                Monitor.Exit(_cacheItemBeforeLock);
            }
        }

        /// <summary>
        /// Remove an item from the cache by key.
        /// </summary>
        /// <param name="indexer">The key of the item to remove.</param>
        public void Remove(string indexer)
        {
            indexer = indexer.ToLower();

            ValidateKey(indexer);

            CacheItem _cacheItemBeforeLock = null;
            bool _lockWasSuccessful;

            do
            {
                lock (__InMemoryCache.SyncRoot)
                {
                    _cacheItemBeforeLock = (CacheItem)__InMemoryCache[indexer];

                    if (IsObjectInCache(_cacheItemBeforeLock))
                    {
                        return;
                    }

                    _lockWasSuccessful = Monitor.TryEnter(_cacheItemBeforeLock);
                }

                if (_lockWasSuccessful == false)
                {
                    Thread.Sleep(0);
                }
            } while (_lockWasSuccessful == false);

            try
            {
                _cacheItemBeforeLock.TouchedByUserAction(true);

                __InMemoryCache.Remove(indexer);
            }
            finally
            {
                Monitor.Exit(_cacheItemBeforeLock);
            }
        }

        /// <summary>
        /// Removes an item from the cache.
        /// </summary>
        /// <param name="indexer">The key to remove.</param>
        /// <remarks>
        /// This seemingly redundant method is here to be called through the ICacheOperations
        /// interface. I put this in place to break any dependency from any other class onto
        /// the Cache class
        /// </remarks>
        public void RemoveItemFromCache(string indexer)
        {
            Remove(indexer);
        }

        /// <summary>
        /// Get the object from the cache for the key.
        /// </summary>
        /// <param name="indexer">
        /// The key whose value to get.
        /// </param>
        /// <returns>
        /// The value associated with the specified key.
        /// </returns>
        public object GetData(string indexer)
        {
            indexer = indexer.ToLower();

            ValidateKey(indexer);

            CacheItem _cacheItemBeforeLock = null;
            var _lockWasSuccessful = false;

            do
            {
                lock (__InMemoryCache.SyncRoot)
                {
                    _cacheItemBeforeLock = (CacheItem)__InMemoryCache[indexer];
                    if (IsObjectInCache(_cacheItemBeforeLock))
                    {
                        return null;
                    }

                    _lockWasSuccessful = Monitor.TryEnter(_cacheItemBeforeLock);
                }

                if (_lockWasSuccessful == false)
                {
                    Thread.Sleep(0);
                }
            } while (_lockWasSuccessful == false);

            try
            {
                if (_cacheItemBeforeLock.HasExpired())
                {
                    __InMemoryCache.Remove(indexer);
                    return null;
                }

                _cacheItemBeforeLock.TouchedByUserAction(false);

                return _cacheItemBeforeLock.Value;
            }
            finally
            {
                Monitor.Exit(_cacheItemBeforeLock);
            }
        }

        /// <summary>
        /// Flush the cache.
        /// </summary>
        /// <remarks>
        /// There may still be thread safety issues in this class with respect to cacheItemExpirations
        /// and scavenging, but I really doubt that either of those will be happening while
        /// a Flush is in progress. It seems that the most likely scenario for a flush
        /// to be called is at the very start of a program, or when absolutely nothing else
        /// is going on. Calling flush in the middle of an application would seem to be
        /// an "interesting" thing to do in normal circumstances.
        /// </remarks>
        public void Flush()
        {
        RestartFlushAlgorithm:

            lock (__InMemoryCache.SyncRoot)
            {
                foreach (string _key in __InMemoryCache.Keys)
                {
                    var _lockWasSuccessful = false;
                    CacheItem _itemToRemove = (CacheItem)__InMemoryCache[_key];

                    try
                    {
                        if (_lockWasSuccessful = Monitor.TryEnter(_itemToRemove))
                        {
                            _itemToRemove.TouchedByUserAction(true);
                        }
                        else
                        {
                            goto RestartFlushAlgorithm;
                        }
                    }
                    finally
                    {
                        if (_lockWasSuccessful)
                            Monitor.Exit(_itemToRemove);
                    }
                }

                var _countBeforeFlushing = __InMemoryCache.Count;

                __InMemoryCache.Clear();
            }
        }

        private static void ValidateKey(string indexer)
        {
            if (String.IsNullOrEmpty(indexer))
            {
                throw new ArgumentException("Parameter name cannot be null or an empty String.", "key");
            }
        }

        private static bool IsObjectInCache(CacheItem cache_item_before_lock)
        {
            return cache_item_before_lock == null || Object.ReferenceEquals(cache_item_before_lock.Value, __AddInProgressFlag);
        }

        #region IDisposable Members

        /// <summary>
        ///
        /// </summary>
        private bool IsDisposed
        {
            get;
            set;
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> if disposing; otherwise, <see langword="false"/>.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed == false)
            {
                if (disposing == true)
                {
                    // Dispose managed resources.
                    this.Clear();
                }

                // Dispose unmanaged resources.

                // Note disposing has been done.
                IsDisposed = true;
            }
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        ~Cache()
        {
            Dispose(false);
        }

        #endregion IDisposable Members
    }
}