using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

/* ConcurrentHashSet.cs
 * Ethan Lafrenais
*/

namespace Dash.Net.Helpers
{
    class ConcurrentHashSet<T> : IDisposable, IEnumerable<T>, IEnumerable
    {
        public int Count
        {
            get
            {
                try
                {
                    // Get the count
                    _lock.EnterReadLock();
                    return set.Count;
                }
                finally
                {
                    // Release lock
                    if (_lock.IsReadLockHeld)
                        _lock.ExitReadLock();
                }
            }
        }

        internal readonly ReaderWriterLockSlim _lock;
        readonly HashSet<T> set;

        public ConcurrentHashSet()
        {
            _lock = new ReaderWriterLockSlim();
            set = new HashSet<T>();
        }

        public bool Add(T item)
        {
            try
            {
                _lock.EnterWriteLock();
                return set.Add(item);
            }
            finally
            {
                if (_lock.IsWriteLockHeld)
                    _lock.ExitWriteLock();
            }
        }

        public bool Remove(T item)
        {
            try
            {
                _lock.EnterWriteLock();
                return set.Remove(item);
            }
            finally
            {
                if (_lock.IsWriteLockHeld)
                    _lock.ExitWriteLock();
            }
        }

        public bool Contains(T item)
        {
            try
            {
                
                _lock.EnterReadLock();
                return set.Contains(item);
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }
        }

        public void Clear()
        {
            try
            {
                _lock.EnterWriteLock();
                set.Clear();
            }
            finally
            {
                if (_lock.IsWriteLockHeld)
                    _lock.ExitWriteLock();
            }
        }

        public T[] ToArray()
        {
            T[] arr;

            try
            {
                _lock.EnterReadLock();
                arr = new T[set.Count];
                set.CopyTo(arr);
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }

            return arr;
        }

        public T[] ToArrayAndClear()
        {
            T[] arr;

            try
            {
                _lock.EnterWriteLock();
                arr = new T[set.Count];
                set.CopyTo(arr);
                set.Clear();
            }
            finally
            {
                if (_lock.IsWriteLockHeld)
                    _lock.ExitWriteLock();
            }

            return arr;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return set.GetEnumerator();
        }

        public void Dispose()
        {
            _lock.Dispose();
        }
    }
}
