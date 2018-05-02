using System.Collections;
using System.Collections.Generic;

/* Batch.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine
{
    public class BatchGroup<TKey, TValue> : IEnumerable<TValue>
    {
        public TKey Key;
        public List<TValue> List;

        public BatchGroup(TKey key)
        {
            Key = key;
            List = new List<TValue>();
        }

        public void Add(TValue value)
        {
            List.Add(value);
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return List.GetEnumerator();
        }
    }

    public class Batch<TKey, TValue> : IEnumerable<BatchGroup<TKey, TValue>>
    {
        public int ElementCount { get; private set; }
        public int BatchCount { get; private set; }

        public List<TValue> UnoptimizedBatch
        {
            get { return unoptimizedBatch; }
        }

        List<BatchGroup<TKey, TValue>> batched;
        Queue<BatchGroup<TKey, TValue>> recycledBatches;
        List<TValue> unoptimizedBatch;

        public Batch()
        {
            batched = new List<BatchGroup<TKey, TValue>>();
            recycledBatches = new Queue<BatchGroup<TKey, TValue>>();
            unoptimizedBatch = new List<TValue>();
        }

        public void BatchItem(TKey key, TValue value, bool dontOptimize = false)
        {
            if (!dontOptimize)
            {
                ElementCount++;

                BatchGroup<TKey, TValue> group;
                if (TryGetGroup(key, out group))
                    group.List.Add(value);
                else
                {
                    BatchGroup<TKey, TValue> batch = CreateBatch(key);
                    batch.Add(value);
                    batched.Add(batch);
                }
            }
            else
                unoptimizedBatch.Add(value);
        }

        bool TryGetGroup(TKey key, out BatchGroup<TKey, TValue> group)
        {
            foreach (BatchGroup<TKey, TValue> g in batched)
                if (g.Key.Equals(key))
                {
                    group = g;
                    return true;
                }

            group = null;
            return false;
        }

        public void Clear(bool clearCache = false)
        {
            if (!clearCache)
                foreach (BatchGroup<TKey, TValue> batch in batched)
                {
                    batch.List.Clear();
                    recycledBatches.Enqueue(batch);
                }

            unoptimizedBatch.Clear();
            batched.Clear();
            ElementCount = 0;

            if (clearCache)
            {
                recycledBatches.Clear();
                BatchCount = 0;
            }
        }

        public IEnumerator<BatchGroup<TKey, TValue>> GetEnumerator()
        {
            return ((IEnumerable<BatchGroup<TKey, TValue>>)batched).GetEnumerator();
        }

        BatchGroup<TKey, TValue> CreateBatch(TKey key)
        {
            if (recycledBatches.Count > 0)
            {
                BatchGroup<TKey, TValue> group = recycledBatches.Dequeue();
                group.Key = key;
                return group;
            }
            else
            {
                BatchCount++;
                return new BatchGroup<TKey, TValue>(key);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return batched.GetEnumerator();
        }
    }
}
