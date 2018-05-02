using System;
using System.Collections.Generic;
using System.Threading;

/* ConcurrentUniqueList.cs
 * Ethan Lafrenais
*/

namespace Dash.Net.Helpers
{
    class ConcurrentUniqueList<T> : IDisposable
    {
        public int Count 
        { 
            get 
            {
                try
                {
                    // Get the count
                    _lock.EnterReadLock();
                    return list.Count;
                }
                finally
                {
                    // Release lock
                    if (_lock.IsReadLockHeld)
                        _lock.ExitReadLock();
                }
            }
        }

        readonly ReaderWriterLockSlim _lock;
        readonly List<T> list;

        public ConcurrentUniqueList()
        {
            _lock = new ReaderWriterLockSlim();
            list = new List<T>();
        }

        public T this[int index]
        {
            get
            {
                try
                {
                    // Get the value
                    _lock.EnterReadLock();
                    return list[index];
                }
                finally
                {
                    // Release lock
                    if (_lock.IsReadLockHeld)
                        _lock.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    // Set the value
                    _lock.EnterWriteLock();
                    list[index] = value;
                }
                finally
                {
                    // Release lock
                    if (_lock.IsWriteLockHeld)
                        _lock.ExitWriteLock();
                }
            }
        }

        public bool Add(T item)
        {
            try
            {
                // Don't allow multiples to be added
                if (Contains(item))
                    return false;

                // Add item
                _lock.EnterWriteLock();
                list.Add(item);
                return true;
            }
            finally
            {
                // Release lock
                if (_lock.IsWriteLockHeld)
                    _lock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            try
            {
                // Clear set
                _lock.EnterWriteLock();
                list.Clear();
            }
            finally
            {
                // Release lock
                if (_lock.IsWriteLockHeld)
                    _lock.ExitWriteLock();
            }
        }

        public bool Contains(T item)
        {
            try
            {
                // Attempt to retreive item
                _lock.EnterReadLock();
                return list.Contains(item);
            }
            finally
            {
                // Release lock
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }
        }

        public bool Remove(T item)
        {
            try
            {
                // Remove item
                _lock.EnterWriteLock();
                return list.Remove(item);
            }
            finally
            {
                // Release lock
                if (_lock.IsWriteLockHeld)
                    _lock.ExitWriteLock();
            }
        }

        public void RemoveFirst()
        {
            try
            {
                // Remove item
                _lock.EnterWriteLock();
                list.RemoveAt(0);
            }
            finally
            {
                // Release lock
                if (_lock.IsWriteLockHeld)
                    _lock.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            _lock.Dispose();
        }
    }
}
