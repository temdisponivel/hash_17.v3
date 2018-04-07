using System;
using System.Collections;
using System.Collections.Generic;
using SimpleCollections.Lists;
using SimpleCollections.Util;

#if USE_UNITY
using UnityEngine;
#endif

namespace SimpleCollections.Hash
{
    /// <summary>
    /// A simple hash set.
    /// This is a very lightweight implementation of a hash set, also known as dictionary.
    /// Use SSet class to perform operations on it.
    /// The performance of this class will be similiar (if not better) than the C#'s default Dictionary.
    /// </summary>
    public class SimpleSet<TKey> : IEnumerable<TKey>
#if USE_UNITY
        , ISerializationCallbackReceiver
#endif
    {
        public int Count;
        public int Capacity;

#if USE_UNITY
        [SerializeField]
#endif
        internal SetEntry<TKey>[] Entries;

        internal IEqualityComparer<TKey> Comparer;
        internal SimpleList<SetEntry<TKey>> EntryPool;
        internal SimpleSetEnumerator<TKey> Enumerator;

        public IEnumerator<TKey> GetEnumerator()
        {
            return SSet.GetEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

#if USE_UNITY
        public void OnBeforeSerialize()
        {
            // DO nothing
        }

        public void OnAfterDeserialize()
        {
            for (int i = 0; i < Entries.Length; i++)
            {
                var entry = Entries[i];
                SSet.Add(this, entry.Key);
            }
            Capacity = Entries.Length;
        }
#endif
    }

    /// <summary>
    /// Represents a entry on the simple set.
    /// </summary>
#if USE_UNITY
    [Serializable]
#endif
    internal class SetEntry<TKey>
    {
        public TKey Key;
        [NonSerialized] public SetEntry<TKey> Next;
        [NonSerialized] public int HashCode;
    }

    /// <summary>
    /// An enumerator used to iterate over the simple set.
    /// </summary>
    /// <typeparam name="<TKey>"></typeparam>
    /// <typeparam name=""></typeparam>
    internal class SimpleSetEnumerator<TKey> : IEnumerator<TKey>
    {
        internal SimpleSet<TKey> Set;
        internal int CurrentIndex;
        internal SetEntry<TKey> CurrentEntry;

        public void Dispose()
        {
            CurrentEntry = null;
        }

        public bool MoveNext()
        {
            MoveToNext();
            return CurrentEntry != null;
        }

        public void Reset()
        {
            CurrentIndex = 0;
            CurrentEntry = null;
        }

        public TKey Current
        {
            get { return CurrentEntry.Key; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        private void MoveToNext()
        {
            if (CurrentEntry != null)
            {
                CurrentEntry = CurrentEntry.Next;
                if (CurrentEntry == null)
                    CurrentIndex++;
                else
                    return;
            }

            for (; CurrentIndex < Set.Capacity; CurrentIndex++)
            {
                CurrentEntry = Set.Entries[CurrentIndex];
                if (CurrentEntry != null)
                    break;
            }
        }
    }

    /// <summary>
    /// Performs operations on a FakeHashSet
    /// </summary>
    public static class SSet
    {
        /// <summary>
        /// Creates a fake hash set with the given capacity.
        /// This will also create an pool of SetEntry (the internal class used to store elements
        /// on the array) to reduce garbage generation and improve performance.
        /// If preWarmPool is true, this will pre-instantiate the elements on the pool.
        /// If preWarmPool is false, this will only create a SimpleSet of size 'size' and
        /// pool removed elements from the set for later reuse.
        /// </summary>
        public static SimpleSet<TKey> Create<TKey>(int size, bool preWarmPool)
        {
            var Set = new SimpleSet<TKey>();
            Set.Count = 0;
            Set.Capacity = GetNextLength(size);
            Set.Entries = new SetEntry<TKey>[Set.Capacity];
            Set.Comparer = EqualityComparer<TKey>.Default;
            Set.Enumerator = new SimpleSetEnumerator<TKey>();
            Set.Enumerator.Set = Set;

            Set.EntryPool = SList.Create<SetEntry<TKey>>(size);
            if (preWarmPool)
            {
                for (int i = 0; i < size; i++)
                    Set.EntryPool[i] = new SetEntry<TKey>();
            }

            return Set;
        }

        /// <summary>
        /// set the comparer to use when getting the hash code.
        /// If comparer is null, the current comparer is not changed.
        /// </summary>
        public static void SetComparer<TKey>(SimpleSet<TKey> Set, IEqualityComparer<TKey> comparer)
        {
            if (comparer != null)
                Set.Comparer = comparer;
        }

        /// <summary>
        /// Adds the value to the set using the key.
        /// If replace is true and the key is already present on the set, we will replace the value,
        /// if false, we will return false to signal the failure.
        /// </summary>
        public static void Add<TKey>(SimpleSet<TKey> Set, TKey key)
        {
            var hash = Set.Comparer.GetHashCode(key);
            var index = (hash & int.MaxValue) % Set.Capacity;

            var inserted = false;
            var entry = Set.Entries[index];
            if (entry != null)
            {
                // If we find a entry with the given key, validate if it's the same key
                // this is needed because two keys can produce the same hash
                if (entry.HashCode != hash && !Set.Comparer.Equals(key, entry.Key))
                {
                    // It's a collision, create an entry and add to the linked list of collisions
                    var newEntry = CreateEntry(Set, key, hash);

                    // Find the last element of the collision chain
                    while (entry.Next != null)
                        entry = entry.Next;

                    entry.Next = newEntry;

                    inserted = true;

                    if (Set.Count >= Set.Capacity)
                        Expand(Set);
                }
            }
            // Did not found, create and add an entry
            else
            {
                var newEntry = CreateEntry(Set, key, hash);

                if (Set.Count >= Set.Capacity)
                    Expand(Set);

                Set.Entries[index] = newEntry;
                inserted = true;
            }

            if (inserted)
                Set.Count++;
        }

        /// <summary>
        /// Creates a new set entry with the given values.
        /// </summary>
        private static SetEntry<TKey> CreateEntry<TKey>(SimpleSet<TKey> Set, TKey key, int hash)
        {
            SetEntry<TKey> newEntry;
            if (Set.EntryPool.Count > 0)
            {
                newEntry = Set.EntryPool[Set.EntryPool.Count - 1];
                SList.RemoveLast(Set.EntryPool); // remove last for better perfomance
            }
            else
                newEntry = new SetEntry<TKey>();

            newEntry.Key = key;
            newEntry.HashCode = hash;
            return newEntry;
        }

        /// <summary>
        /// Returns true if the given key is present on the set.
        /// </summary>
        public static bool Contains<TKey>(SimpleSet<TKey> Set, TKey key)
        {
            var hash = Set.Comparer.GetHashCode(key);
            var index = (hash & int.MaxValue) % Set.Capacity;

            var entry = Set.Entries[index];
            if (entry != null)
            {
                if (entry.HashCode == hash && Set.Comparer.Equals(key, entry.Key))
                    return true;
                else
                {
                    while (entry.Next != null)
                    {
                        entry = entry.Next;
                        if (entry.HashCode == hash && Set.Comparer.Equals(entry.Key, key))
                            return true;
                    }

                    return false;
                }
            }
            else
                return false;
        }

        /// <summary>
        /// Removes the given item that corresponds to the given key.
        /// </summary>
        public static void Remove<TKey>(SimpleSet<TKey> Set, TKey key)
        {
            var hash = Set.Comparer.GetHashCode(key);
            var index = (hash & int.MaxValue) % Set.Capacity;

            var entry = Set.Entries[index];
            if (entry == null)
                return;

            if (entry.HashCode == hash && Set.Comparer.Equals(entry.Key, key))
            {
                Set.Entries[index] = null;
                Set.Count--;

                entry.Key = default(TKey);
                entry.HashCode = -1;
                SList.Add(Set.EntryPool, entry);
            }
            else
            {
                while (entry.Next != null)
                {
                    if (entry.Next.HashCode == hash && Set.Comparer.Equals(entry.Next.Key, key))
                    {
                        var removed = entry.Next;
                        entry.Next = entry.Next.Next;

                        removed.Key = default(TKey);
                        removed.HashCode = -1;
                        SList.Add(Set.EntryPool, removed);

                        Set.Count--;
                    }
                    entry = entry.Next;
                }
            }
        }

        /// <summary>
        /// Expands the set to the next good capacity.
        /// This will expand to the capacity GetNextLength(set.Capacity).
        /// See: GetNextLength.
        /// </summary>
        public static void Expand<TKey>(SimpleSet<TKey> Set)
        {
            var oldCapacity = Set.Capacity;
            var oldEntries = Set.Entries;

            var newCapacity = GetNextLength(oldCapacity);

            var newArrayEntry = new SetEntry<TKey>[newCapacity];
            Set.Capacity = newCapacity;
            Set.Entries = newArrayEntry;
            Set.Count = 0;

            for (int i = 0; i < oldCapacity; i++)
            {
                var entry = oldEntries[i];
                if (entry != null)
                {
                    Add(Set, entry.Key);
                    while (entry.Next != null)
                    {
                        entry = entry.Next;
                        Add(Set, entry.Key);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the next good length for a bucket array.
        /// This use a predefined list of primes to decide the next length.
        /// The returned value is the first prime greater than currentLength.
        /// </summary>
        public static int GetNextLength(int currentLength)
        {
            for (int i = 0; i < Primes.Length; i++)
            {
                if (Primes[i] > currentLength)
                    return Primes[i];
            }
            return Primes[71]; // hardcode because we know the size
        }

        /// <summary>
        /// Remove all elements of the set.
        /// </summary>
        public static void Clear<TKey>(SimpleSet<TKey> set)
        {
            for (int i = 0; i < set.Capacity; i++)
            {
                var entry = set.Entries[i];
                if (entry == null)
                    continue;

                set.Entries[i] = null;

                entry.Key = default(TKey);
                entry.HashCode = -1;

                SList.Add(set.EntryPool, entry);
                while (entry.Next != null)
                {
                    entry = entry.Next;

                    entry.Key = default(TKey);
                    entry.HashCode = -1;

                    SList.Add(set.EntryPool, entry);
                }
            }

            set.Count = 0;
        }

        /// <summary>
        /// Returns a enumerator that can be used to iterate over the set.
        /// </summary>
        public static IEnumerator<TKey> GetEnumerator<TKey>(SimpleSet<TKey> set)
        {
            set.Enumerator.Reset();
            return set.Enumerator;
        }

        /// <summary>
        /// List of primes to expand the array.
        /// </summary>
        internal static readonly int[] Primes = new int[72]
        {
            3,
            7,
            11,
            17,
            23,
            29,
            37,
            47,
            59,
            71,
            89,
            107,
            131,
            163,
            197,
            239,
            293,
            353,
            431,
            521,
            631,
            761,
            919,
            1103,
            1327,
            1597,
            1931,
            2333,
            2801,
            3371,
            4049,
            4861,
            5839,
            7013,
            8419,
            10103,
            12143,
            14591,
            17519,
            21023,
            25229,
            30293,
            36353,
            43627,
            52361,
            62851,
            75431,
            90523,
            108631,
            130363,
            156437,
            187751,
            225307,
            270371,
            324449,
            389357,
            467237,
            560689,
            672827,
            807403,
            968897,
            1162687,
            1395263,
            1674319,
            2009191,
            2411033,
            2893249,
            3471899,
            4166287,
            4999559,
            5999471,
            7199369
        };
    }
}