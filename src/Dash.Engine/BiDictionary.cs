using System;
using System.Collections.Generic;

/*  BiDictionary.cs
    From: http://stackoverflow.com/questions/268321/bidirectional-1-to-1-dictionary-in-c-sharp
*/

namespace Dash.Engine
{
    /// <summary>
    /// This is a dictionary guaranteed to have only one of each value and key. 
    /// It may be searched either by TFirst or by TSecond, giving a unique answer because it is 1 to 1.
    /// </summary>
    /// <typeparam name="TFirst">The type of the "key"</typeparam>
    /// <typeparam name="TSecond">The type of the "value"</typeparam>
    public class BiDictionary<TFirst, TSecond>
    {
        public ICollection<TFirst> Keys { get { return firstToSecond.Keys; } }
        public ICollection<TSecond> Values { get { return firstToSecond.Values; } }
        public ICollection<KeyValuePair<TFirst, TSecond>> Pairs { get { return firstToSecond; } }

        IDictionary<TFirst, TSecond> firstToSecond = new Dictionary<TFirst, TSecond>();
        IDictionary<TSecond, TFirst> secondToFirst = new Dictionary<TSecond, TFirst>();

        #region Exception throwing methods

        /// <summary>
        /// Tries to add the pair to the dictionary.
        /// Throws an exception if either element is already in the dictionary
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        public void Add(TFirst first, TSecond second)
        {
            if (firstToSecond.ContainsKey(first) || secondToFirst.ContainsKey(second))
                throw new ArgumentException("Duplicate first or second");

            firstToSecond.Add(first, second);
            secondToFirst.Add(second, first);
        }

        /// <summary>
        /// Find the TSecond corresponding to the TFirst first
        /// Throws an exception if first is not in the dictionary.
        /// </summary>
        /// <param name="first">the key to search for</param>
        /// <returns>the value corresponding to first</returns>
        public TSecond Get(TFirst first)
        {
            TSecond second;
            if (!firstToSecond.TryGetValue(first, out second))
                throw new ArgumentException("first");

            return second;
        }

        /// <summary>
        /// Find the TFirst corresponing to the Second second.
        /// Throws an exception if second is not in the dictionary.
        /// </summary>
        /// <param name="second">the key to search for</param>
        /// <returns>the value corresponding to second</returns>
        public TFirst Get(TSecond second)
        {
            TFirst first;
            if (!secondToFirst.TryGetValue(second, out first))
                throw new ArgumentException("second");

            return first;
        }


        /// <summary>
        /// Remove the record containing first.
        /// If first is not in the dictionary, throws an Exception.
        /// </summary>
        /// <param name="first">the key of the record to delete</param>
        public void Remove(TFirst first)
        {
            TSecond second;
            if (!firstToSecond.TryGetValue(first, out second))
                throw new ArgumentException("first");

            firstToSecond.Remove(first);
            secondToFirst.Remove(second);
        }

        /// <summary>
        /// Remove the record containing second.
        /// If second is not in the dictionary, throws an Exception.
        /// </summary>
        /// <param name="second">the key of the record to delete</param>
        public void Remove(TSecond second)
        {
            TFirst first;
            if (!secondToFirst.TryGetValue(second, out first))
                throw new ArgumentException("second");

            secondToFirst.Remove(second);
            firstToSecond.Remove(first);
        }

        #endregion

        #region Try methods

        /// <summary>
        /// Tries to add the pair to the dictionary.
        /// Returns false if either element is already in the dictionary        
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns>true if successfully added, false if either element are already in the dictionary</returns>
        public Boolean TryAdd(TFirst first, TSecond second)
        {
            if (firstToSecond.ContainsKey(first) || secondToFirst.ContainsKey(second))
                return false;

            firstToSecond.Add(first, second);
            secondToFirst.Add(second, first);
            return true;
        }


        /// <summary>
        /// Find the TSecond corresponding to the TFirst first.
        /// Returns false if first is not in the dictionary.
        /// </summary>
        /// <param name="first">the key to search for</param>
        /// <param name="second">the corresponding value</param>
        /// <returns>true if first is in the dictionary, false otherwise</returns>
        public Boolean TryGetValue(TFirst first, out TSecond second)
        {
            return firstToSecond.TryGetValue(first, out second);
        }

        /// <summary>
        /// Find the TFirst corresponding to the TSecond second.
        /// Returns false if second is not in the dictionary.
        /// </summary>
        /// <param name="second">the key to search for</param>
        /// <param name="first">the corresponding value</param>
        /// <returns>true if second is in the dictionary, false otherwise</returns>
        public Boolean TryGetValue(TSecond second, out TFirst first)
        {
            return secondToFirst.TryGetValue(second, out first);
        }

        /// <summary>
        /// Remove the record containing first, if there is one.
        /// </summary>
        /// <param name="first"></param>
        /// <returns> If first is not in the dictionary, returns false, otherwise true</returns>
        public Boolean TryRemove(TFirst first)
        {
            TSecond second;
            if (!firstToSecond.TryGetValue(first, out second))
                return false;

            firstToSecond.Remove(first);
            secondToFirst.Remove(second);
            return true;
        }

        /// <summary>
        /// Remove the record containing second, if there is one.
        /// </summary>
        /// <param name="second"></param>
        /// <returns> If second is not in the dictionary, returns false, otherwise true</returns>
        public Boolean TryRemove(TSecond second)
        {
            TFirst first;
            if (!secondToFirst.TryGetValue(second, out first))
                return false;

            secondToFirst.Remove(second);
            firstToSecond.Remove(first);
            return true;
        }

        #endregion

        public TSecond this[TFirst key]
        {
            get { return firstToSecond[key]; }
            set
            {
                firstToSecond[key] = value;
                secondToFirst[value] = key;
            }
        }

        public TFirst this[TSecond key]
        {
            get { return secondToFirst[key]; }
            set
            {
                secondToFirst[key] = value;
                firstToSecond[value] = key;
            }
        }

        /// <summary>
        /// The number of pairs stored in the dictionary
        /// </summary>
        public Int32 Count
        {
            get { return firstToSecond.Count; }
        }

        /// <summary>
        /// Removes all items from the dictionary.
        /// </summary>
        public void Clear()
        {
            firstToSecond.Clear();
            secondToFirst.Clear();
        }
    }
}