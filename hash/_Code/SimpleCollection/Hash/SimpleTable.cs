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
    /// A simple hash table.
    /// This is a very lightweight implementation of a hash table, also known as dictionary.
    /// Use STable class to perform operations on it.
    /// The performance of this class will be similiar (if not better) than the C#'s default Dictionary.
    /// </summary>
    public class SimpleTable<TKey, TValue> : IEnumerable<Pair<TKey, TValue>>
#if USE_UNITY
        , ISerializationCallbackReceiver
#endif
    {
        public int Count;
        public int Capacity;

#if USE_UNITY
        [SerializeField]
#endif
        internal TableEntry<TKey, TValue>[] Entries;

        internal IEqualityComparer<TKey> Comparer;
        internal SimpleList<TableEntry<TKey, TValue>> EntryPool;
        internal SimpleTableEnumerator<TKey, TValue> Enumerator;
        
        /// <summary>
        /// Shorthand for STable.Find(this, key) and STable.Add(this, key, value, true).
        /// </summary>
        public TValue this[TKey key]
        {
            get { return STable.Find(this, key); }
            set { STable.Add(this, key, value, true); }
        }

        public IEnumerator<Pair<TKey, TValue>> GetEnumerator()
        {
            return STable.GetEnumerator(this);
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
                STable.Add(this, entry.Key, entry.Value, true);
            }
            Capacity = Entries.Length;
        }
#endif
    }

    /// <summary>
    /// Represents a entry on the simple table.
    /// </summary>
#if USE_UNITY
    [Serializable]
#endif
    internal class TableEntry<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;
        [NonSerialized]
        public TableEntry<TKey, TValue> Next;
        [NonSerialized]
        public int HashCode;
    }

    /// <summary>
    /// An enumerator used to iterate over the simple table.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal class SimpleTableEnumerator<TKey, TValue> : IEnumerator<Pair<TKey, TValue>>
    {
        internal SimpleTable<TKey, TValue> Table;
        internal int CurrentIndex;
        internal TableEntry<TKey, TValue> CurrentEntry;

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

        public Pair<TKey, TValue> Current
        {
            get
            {
                var pair = new Pair<TKey, TValue>();
                pair.Key = CurrentEntry.Key;
                pair.Value = CurrentEntry.Value;
                return pair;
            }
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
            
            for (; CurrentIndex < Table.Capacity; CurrentIndex++)
            {
                CurrentEntry = Table.Entries[CurrentIndex];
                if (CurrentEntry != null)
                    break;
            }
        }
    }

    /// <summary>
    /// Performs operations on a FakeHashTable
    /// </summary>
    public static class STable
    {
        /// <summary>
        /// Creates a fake hash table with the given capacity.
        /// This will also create an pool of TableEntry (the internal class used to store elements
        /// on the array) to reduce garbage generation and improve performance.
        /// If preWarmPool is true, this will pre-instantiate the elements on the pool.
        /// If preWarmPool is false, this will only create a SimpleSet of size 'size' and
        /// pool removed elements from the table for later reuse.
        /// </summary>
        public static SimpleTable<TKey, TValue> Create<TKey, TValue>(int size, bool preWarmPool)
        {
            var table = new SimpleTable<TKey, TValue>();
            table.Count = 0;
            table.Capacity = GetNextLength(size);
            table.Entries = new TableEntry<TKey, TValue>[table.Capacity];
            table.Comparer = EqualityComparer<TKey>.Default;
            table.Enumerator = new SimpleTableEnumerator<TKey, TValue>();
            table.Enumerator.Table = table;

            table.EntryPool = SList.Create<TableEntry<TKey, TValue>>(size);
            if (preWarmPool)
            {
                for (int i = 0; i < size; i++)
                    table.EntryPool[i] = new TableEntry<TKey, TValue>();
            }
            
            return table;
        }

        /// <summary>
        /// Set the comparer to use when getting the hash code.
        /// If comparer is null, the current comparer is not changed.
        /// </summary>
        public static void SetComparer<TKey, TValue>(SimpleTable<TKey, TValue> table, IEqualityComparer<TKey> comparer)
        {
            if (comparer != null)
                table.Comparer = comparer;
        }

        /// <summary>
        /// Adds the value to the table using the key.
        /// If replace is true and the key is already present on the table, we will replace the value,
        /// if false, we will return false to signal the failure.
        /// </summary>
        public static bool Add<TKey, TValue>(SimpleTable<TKey, TValue> table, TKey key, TValue value, bool replace)
        {
            var hash = table.Comparer.GetHashCode(key);
            var index = (hash & int.MaxValue) % table.Capacity;

            var inserted = false;
            var entry = table.Entries[index];
            if (entry != null)
            {
                // If we find a entry with the given key, validate if it's the same key
                // this is needed because two keys can produce the same hash
                if (entry.HashCode == hash && table.Comparer.Equals(key, entry.Key))
                {
                    if (replace)
                        entry.Value = value;
                    else
                    {
#if DEBUG
                        var msg = string.Format("ADDING THE KEY {0} TWICE TO THE SIMPLE TABLE!", key);
                        System.Diagnostics.Debug.Assert(true, msg);

#if USE_UNITY
                        UnityEngine.Debug.LogError(msg);
#endif

#endif
                    }
                }
                else
                {
                    // It's a collision, create an entry and add to the linked list of collisions
                    var newEntry = CreateEntry(table, key, value, hash);

                    // Find the last element of the collision chain
                    while (entry.Next != null)
                        entry = entry.Next;

                    entry.Next = newEntry;

                    inserted = true;

                    if (table.Count >= table.Capacity)
                        Expand(table);
                }
            }
            // Did not found, create and add an entry
            else
            {
                var newEntry = CreateEntry(table, key, value, hash);

                if (table.Count >= table.Capacity)
                    Expand(table);

                table.Entries[index] = newEntry;
                inserted = true;
            }

            if (inserted)
                table.Count++;

            return inserted;
        }

        /// <summary>
        /// Creates a new table entry with the given values.
        /// </summary>
        private static TableEntry<TKey, TValue> CreateEntry<TKey, TValue>(SimpleTable<TKey, TValue> table, TKey key, TValue value, int hash)
        {
            TableEntry<TKey, TValue> newEntry;
            if (table.EntryPool.Count > 0)
            {
                newEntry = table.EntryPool[table.EntryPool.Count - 1];
                SList.RemoveLast(table.EntryPool); // remove last for better perfomance
            }
            else
                newEntry = new TableEntry<TKey, TValue>();

            newEntry.Key = key;
            newEntry.Value = value;
            newEntry.HashCode = hash;
            return newEntry;
        }

        /// <summary>
        /// Returns true if the given key is present on the table.
        /// </summary>
        public static bool Contains<TKey, TValue>(SimpleTable<TKey, TValue> table, TKey key)
        {
            var hash = table.Comparer.GetHashCode(key);
            var index = (hash & int.MaxValue) % table.Capacity;

            var entry = table.Entries[index];
            if (entry != null)
            {
                if (entry.HashCode == hash && table.Comparer.Equals(key, entry.Key))
                    return true;
                else
                {
                    while (entry.Next != null)
                    {
                        entry = entry.Next;
                        if (entry.HashCode == hash && table.Comparer.Equals(entry.Key, key))
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
        public static void Remove<TKey, TValue>(SimpleTable<TKey, TValue> table, TKey key)
        {
            var hash = table.Comparer.GetHashCode(key);
            var index = (hash & int.MaxValue) % table.Capacity;

            var entry = table.Entries[index];
            if (entry == null)
                return;

            if (entry.HashCode == hash && table.Comparer.Equals(entry.Key, key))
            {
                table.Entries[index] = null;
                table.Count--;

                entry.Key = default(TKey);
                entry.Value = default(TValue);
                entry.HashCode = -1;
                SList.Add(table.EntryPool, entry);
            }
            else
            {
                while (entry.Next != null)
                {
                    if (entry.Next.HashCode == hash && table.Comparer.Equals(entry.Next.Key, key))
                    {
                        var removed = entry.Next;
                        entry.Next = entry.Next.Next;

                        removed.Key = default(TKey);
                        removed.Value = default(TValue);
                        removed.HashCode = -1;
                        SList.Add(table.EntryPool, removed);

                        table.Count--;
                    }
                    entry = entry.Next;
                }
            }
        }

        /// <summary>
        /// Finds the value that corresponds to the given key.
        /// Returns the value for the key or null if the key is not on the dictionary.
        /// </summary>
        public static TValue Find<TKey, TValue>(SimpleTable<TKey, TValue> table, TKey key)
        {
            var hash = table.Comparer.GetHashCode(key);
            var index = (hash & int.MaxValue) % table.Capacity;

            var entry = table.Entries[index];
            if (entry != null)
            {
                if (entry.HashCode == hash && table.Comparer.Equals(entry.Key, key))
                    return entry.Value;
                else
                {
                    while (entry.Next != null)
                    {
                        entry = entry.Next;
                        if (entry.HashCode == hash && table.Comparer.Equals(entry.Key, key))
                            return entry.Value;
                    }
                }
            }

            return default(TValue);
        }

        /// <summary>
        /// Tries to get the value. Returns true if found, false otherwise.
        /// If this returns true, the value parameter will be the found value, otherwise it will be null.
        /// </summary>
        public static bool TryGetValue<TKey, TValue>(SimpleTable<TKey, TValue> table, TKey key, out TValue value)
        {
            value = Find(table, key);
            return value != null;
        }

        /// <summary>
        /// Expands the table to the next good capacity.
        /// This will expand to the capacity GetNextLength(table.Capacity).
        /// See: GetNextLength.
        /// </summary>
        public static void Expand<TKey, TValue>(SimpleTable<TKey, TValue> table)
        {
            var oldCapacity = table.Capacity;
            var oldEntries = table.Entries;

            var newCapacity = GetNextLength(oldCapacity);

            var newArrayEntry = new TableEntry<TKey, TValue>[newCapacity];
            table.Capacity = newCapacity;
            table.Entries = newArrayEntry;
            table.Count = 0;

            for (int i = 0; i < oldCapacity; i++)
            {
                var entry = oldEntries[i];
                if (entry != null)
                {
                    Add(table, entry.Key, entry.Value, false);
                    while (entry.Next != null)
                    {
                        entry = entry.Next;
                        Add(table, entry.Key, entry.Value, false);
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
        /// Remove all elements of the table.
        /// </summary>
        public static void Clear<TKey, TValue>(SimpleTable<TKey, TValue> table)
        {
            for (int i = 0; i < table.Capacity; i++)
            {
                var entry = table.Entries[i];
                if (entry == null)
                    continue;

                table.Entries[i] = null;

                entry.Key = default(TKey);
                entry.Value = default(TValue);
                entry.HashCode = -1;

                SList.Add(table.EntryPool, entry);
                while (entry.Next != null)
                {
                    entry = entry.Next;

                    entry.Key = default(TKey);
                    entry.Value = default(TValue);
                    entry.HashCode = -1;

                    SList.Add(table.EntryPool, entry);
                }
            }

            table.Count = 0;
        }

        /// <summary>
        /// Returns a enumerator that can be used to iterate over the table.
        /// </summary>
        public static IEnumerator<Pair<TKey, TValue>> GetEnumerator<TKey, TValue>(SimpleTable<TKey, TValue> table)
        {
            table.Enumerator.Reset();
            return table.Enumerator;
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