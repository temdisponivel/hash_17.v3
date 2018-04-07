using System;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
#if USE_UNITY
using UnityEngine;
#endif

namespace SimpleCollections.Lists
{
    /// <summary>
    /// Lightweight list.
    /// Please note that diffently than C#'s list, you can access items with indexes greater than Count, just not greater than Capacity.
    /// Use the SList class to perform operations on this.
    /// This list is slightly more efficient than C#'s default List.
    /// This doesn't rely on ANY of microsoft's collections class, only on the native array (not event the Array class).
    /// </summary>
    [Serializable]
    public class SimpleList<T> : IEqualityComparer<SimpleList<T>>
#if USE_UNITY
        , ISerializationCallbackReceiver
#endif
    {
#if USE_UNITY
        [NonSerialized]
#endif
        public int Id;

#if USE_UNITY
        [SerializeField]
#endif
        public int Count;

#if USE_UNITY
        [SerializeField]
#endif
        public int Capacity;

#if USE_UNITY
        [SerializeField]
#endif
        public T[] Data;

        /// <summary>
        /// Gets or sets the item on the given index.
        /// If the index passed to set is greater than Count, the SList.Add method will be called,
        /// otherwise we just define the item.
        /// You can pass an index greater than Count (I don't know why would you want that) to the get method, but never greater than Capacity.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            get { return Data[index]; }
            set
            {
                if (index >= Count)
                    SList.Add(this, value);
                else
                    Data[index] = value;
            }
        }

        public bool Equals(SimpleList<T> x, SimpleList<T> y)
        {
            return SList.Compare(x, y);
        }

        public int GetHashCode(SimpleList<T> obj)
        {
            return SList.HashCode(obj);
        }

#if USE_UNITY

        public void OnBeforeSerialize()
        {
            if (Data == null)
            {
                Count = 0;
                Capacity = 0;
            }
            else
            {
                Count = Data.Length;
                Capacity = Data.Length;
            }
        }

        public void OnAfterDeserialize()
        {
            Id = SList.GetId();
        }

#endif
    }

    /// <summary>
    /// Performs operations on the SimpleSet.
    /// </summary>
    public static class SList
    {
        private static int _id = int.MinValue;

        /// <summary>
        /// Returns a unique list id.
        /// </summary>
        internal static int GetId()
        {
            return _id++;
        }

        /// <summary>
        /// Creates an simple list with the given capacity.
        /// </summary>
        public static SimpleList<T> Create<T>(int size)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(size > 0, "SIMPLE LIST SIZE MUST BE GREATER THAN 0! SIZE: " + size);
#if USE_UNITY
            if (size <= 0)
                UnityEngine.Debug.LogError("SIMPLE LIST SIZE MUST BE GREATER THAN 0! SIZE: " + size);
#endif
#endif
            var list = new SimpleList<T>();
            list.Count = 0;
            list.Capacity = size;
            list.Data = new T[size];
            list.Id = _id++;
            return list;
        }

        /// <summary>
        /// Adds an item to the end of the list.
        /// The list will be expanded if necessary (by a factor of 2.)
        /// You can add a null item, but bear in mind that the count of the list will be increment
        /// and as such, you'll have to validate null items yourself.
        /// </summary>
        public static void Add<T>(SimpleList<T> list, T item)
        {
            if (list.Count >= list.Capacity)
                EnsureCapacity(list, list.Capacity * 2);
            list.Data[list.Count++] = item;
        }

        /// <summary>
        /// Shorthand for calling Insert(list, item, 0).
        /// </summary>
        public static void AddFirst<T>(SimpleList<T> list, T item)
        {
            Insert(list, item, 0);
        }

        /// <summary>
        /// Inserts an item on the specific index.
        /// If index is greater than the list capacity, the item will be added as the last element of the list.
        /// If you want to add the item to a index greater than the list's capacity, first use EnsureCapacity
        /// to ensure that the list's capacity is greater than index, then call this method.
        /// The list will be expanded if necessary (by a factor of 2.)
        /// </summary>
        public static void Insert<T>(SimpleList<T> list, T item, int index)
        {
            if (list.Count >= list.Capacity)
                EnsureCapacity(list, list.Capacity * 2);

            index = Math.Min(list.Capacity - 1, index);

            for (int i = list.Count; i > index; i--)
                list.Data[i] = list.Data[i - 1];

            list.Data[index] = item;
            list.Count++;
        }

        /// <summary>
        /// Removes a specific item from the list.
        /// If the items doesn't exists on the array, does nothing.
        /// Returns true if the item was found and removed, false otherwise.
        /// To remove multiple items, use RemoveAll.
        /// </summary>
        public static bool Remove<T>(SimpleList<T> list, T item)
        {
            var index = IndexOf(list, item);
            if (index != -1)
            {
                RemoveAt(list, index);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Calls Remove until it returns false.
        /// Returns true if any item was found and removed.
        /// </summary>
        public static bool RemoveAll<T>(SimpleList<T> list, T item)
        {
            bool result = false;
            do
            {
                if (!Remove(list, item))
                    break;
                result = true;
            } while (true);

            return result;
        }

        /// <summary>
        /// Calls predicate for each item of the list and remove the first item that the predicates returned true.
        /// Returns true if a item was found and removed, false otherwise.
        /// To delete multiple items, use DeleteAll.
        /// </summary>
        public static bool Delete<T>(SimpleList<T> list, Predicate<T> predicate)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list.Data[i]))
                {
                    RemoveAt(list, i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Calls Delete untill it returns false.
        /// Returns true if any item was found and removed.
        /// </summary>
        public static bool DeleteAll<T>(SimpleList<T> list, Predicate<T> predicate)
        {
            bool result = false;
            do
            {
                if (!Delete(list, predicate))
                    break;
                result = true;
            } while (true);

            return result;
        }

        /// <summary>
        /// Remove the item at the specific index.
        /// Make sure the given index is smaller than the list's count.
        /// </summary>
        public static void RemoveAt<T>(SimpleList<T> list, int index)
        {
#if DEBUG
            if (index < 0 || list.Count <= index)
            {
                var msg = "THE GIVEN INDEX MUST BE SMALLER THAN COUNT AND GREATER THAN 0. INDEX: " + index;
                System.Diagnostics.Debug.Assert(false, msg);
#if USE_UNITY
                UnityEngine.Debug.LogError(msg);
#endif
            }
#endif
            for (int i = index; i < list.Count - 1; i++)
                list.Data[i] = list.Data[i + 1];

            list.Count--;
            list.Data[list.Count] = default(T);
        }

        /// <summary>
        /// Shorthand for RemoveAt(list, 0).
        /// </summary>
        public static void RemoveFirst<T>(SimpleList<T> list)
        {
            RemoveAt(list, 0);
        }

        /// <summary>
        /// Shorthand for RemoveAt(list, list.Count - 1).
        /// </summary>
        public static void RemoveLast<T>(SimpleList<T> list)
        {
            RemoveAt(list, list.Count - 1);
        }

        /// <summary>
        /// Removes and returns the last element of the list.
        /// If the list is empty, default(T) is returned.
        /// </summary>
        public static T Pop<T>(SimpleList<T> list)
        {
            if (list.Count == 0)
                return default(T);

            var last = list[list.Count - 1];
            RemoveLast(list);
            return last;
        }

        /// <summary>
        /// Returns the last element of the list.
        /// If the list is empty, default(T) is returned.
        /// </summary>
        public static T Peek<T>(SimpleList<T> list)
        {
            if (list.Count == 0)
                return default(T);

            return list[list.Count - 1];
        }

        /// <summary>
        /// Add an item to the end of the list.
        /// </summary>
        public static void Push<T>(SimpleList<T> list, T item)
        {
            Add(list, item);
        }

        /// <summary>
        /// Adds an element to the end of the list.
        /// </summary>
        public static void Enqueue<T>(SimpleList<T> list, T item)
        {
            Add(list, item);
        }

        /// <summary>
        /// Removes and returns the element at the start of the list.
        /// If the list is empty, default(T) is returned.
        /// </summary>
        public static T Dequeue<T>(SimpleList<T> list)
        {
            if (list.Count == 0)
                return default(T);

            var item = list[0];
            RemoveAt(list, 0);
            return item;
        }

        /// <summary>
        /// Iterate through the lists and returns true if any of its elements are equal to the given item.
        /// This will use EqualityComparer<T>.Default for comparison.
        /// </summary>
        public static bool Contains<T>(SimpleList<T> list, T item)
        {
            return IndexOf(list, item) != -1;
        }

        /// <summary>
        /// Calls the predicate for each item of the list and returns true if any of the calls to predicated returned true.
        /// The iteration will stop on the first time the predicate returns true.
        /// </summary>
        public static bool Exists<T>(SimpleList<T> list, Predicate<T> predicate)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list.Data[i]))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the index of the given item on the list.
        /// If the item is not on the list, returns -1.
        /// This uses EqualityComparer<T>.Default for comparison.
        /// </summary>
        public static int IndexOf<T>(SimpleList<T> list, T item)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(list[i], item))
                    return i;
            }
            return -1;
        }

        public static T Find<T>(SimpleList<T> list, Predicate<T> predicate)
        {
            var index = FindIndex(list, predicate);
            if (index == -1)
                return default(T);
            else
                return list[index];
        }

        /// <summary>
        /// Same as IndexOf, except instead of using EqualityComparer<T>.Default, it uses the given predicate as equality comparer.
        /// </summary>
        public static int FindIndex<T>(SimpleList<T> list, Predicate<T> predicate)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                    return i;
            }
            return -1;
        }

        /// <summary>   
        /// Ensures that list has at least as many slots as 'capacity'.
        /// If the current list's capacity is already greater or equal to 'capacity', nothing happens.
        /// If the capacity is less or equal to zero, it will be defined to 1.
        /// </summary>
        public static void EnsureCapacity<T>(SimpleList<T> list, int capacity)
        {
            capacity = Math.Max(capacity, 1);
            if (list.Capacity < capacity)
            {
                list.Capacity = capacity;
                var newArray = new T[capacity];
                Array.Copy(list.Data, 0, newArray, 0, list.Count);
                list.Data = newArray;
            }
        }

        /// <summary>
        /// Returns the underlying array of the list.
        /// You can use this safely. Just bear in mind that the count and capacity of the list will not
        /// reflect the changed you make directly on the array.
        /// </summary>
        public static T[] GetUnderlyingArray<T>(SimpleList<T> list)
        {
            return list.Data;
        }

        /// <summary>
        /// Creates a new SimpleSet item and copy the properties of the given list to the new list.
        /// The copy is made by value, so the two lists are completely independent from one another.
        /// </summary>
        public static SimpleList<T> Clone<T>(SimpleList<T> toClone)
        {
            var clone = Create<T>(toClone.Capacity);
            Array.Copy(toClone.Data, 0, clone.Data, 0, toClone.Count);
            clone.Count = toClone.Count;
            return clone;
        }

        /// <summary>
        /// Creates a simple list and copies the content of the array into the underlying array of the list.
        /// The array is copies by value, so there will be memory allocation here. Use FromArrayNonAlloc to prevent allocation.
        /// The count and capacity of the list will be the same as the array's length.
        /// </summary>
        public static SimpleList<T> FromArray<T>(T[] array)
        {
            var result = Create<T>(array.Length);
            result.Count = array.Length;
            Array.Copy(array, 0, result.Data, 0, array.Length);
            return result;
        }

        /// <summary>
        /// Same as FromArray, except this will not create a new array, instead, it will define the underlying array of the list
        /// to be the given array.
        /// </summary>
        public static SimpleList<T> FromArrayNonAlloc<T>(T[] array)
        {
            var result = new SimpleList<T>();
            result.Id = _id++;
            result.Data = array;
            result.Count = array.Length;
            result.Capacity = array.Length;
            return result;
        }

        /// <summary>
        /// Copy the contents of the list 'from' to the list 'to'.
        /// Same as: CopyRange(from, 0, to, 0, from.Count);
        /// </summary>
        public static void Copy<T>(SimpleList<T> from, SimpleList<T> to)
        {
            CopyRange(from, 0, to, 0, from.Count);
        }

        /// <summary>
        /// Copy the items from 'from', starting at 'fromStartIndex' to 'to' starting at the index 'toStartIndex'.
        /// It will copy 'count' items.
        /// Ex: 
        ///     from = [1, 2, 3];
        ///     to = [4, 5, 6]
        ///     CopyRange(from, 2, to, 1, 1)
        ///     from == [1, 2, 3]
        ///     to == [4, 3, 6]
        /// You need to make sure that:
        ///  fromStartIndex + count > from.Count
        ///  toStartIndex + count > to.Capacity
        /// </summary>
        public static void CopyRange<T>(SimpleList<T> from, int fromStartIndex, SimpleList<T> to, int toStartIndex,
            int count)
        {
#if DEBUG
            if (from.Count < fromStartIndex + count)
            {
                var msg = string.Format("from.Count >= fromStartIndex + count MUST BE TRUE!\n from.Count: {0} - fromStartIndex: {1} - count {2}", from.Count, fromStartIndex, count);
                System.Diagnostics.Debug.Assert(false, msg);

#if USE_UNITY
                UnityEngine.Debug.LogError(msg);
#endif
            }
            if (to.Capacity < toStartIndex + count)
            {
                var msg = string.Format("to.Capacity >= toStartIndex + count MUST BE TRUE!\n to.Capacity: {0} - toStartIndex: {1} - count {2}", to.Capacity, toStartIndex, count);
                System.Diagnostics.Debug.Assert(false, msg);

#if USE_UNITY
                UnityEngine.Debug.LogError(msg);
#endif
            }
#endif

            for (int i = 0, fromI = fromStartIndex, toI = toStartIndex; i < count; i++, fromI++, toI++)
                to[toI] = from[fromI];
        }

        /// <summary>
        /// Sets all items of the list to default(T).
        /// </summary>
        public static void Clear<T>(SimpleList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
                list[i] = default(T);
            list.Count = 0;
        }

        /// <summary>
        /// Returns true if the two lists are equal (uses ==).
        /// </summary>
        public static bool Compare<T>(SimpleList<T> left, SimpleList<T> right)
        {
            return left == right;
        }

        /// <summary>
        /// Returns a real hashcode for the list.
        /// </summary>
        public static int HashCode<T>(SimpleList<T> simpleList)
        {
            var hashCode = simpleList.Id;
            // Let wrap around
            unchecked
            {
                hashCode = (hashCode * 397) ^ simpleList.Count;
                hashCode = (hashCode * 397) ^ simpleList.Capacity;
            }
            return hashCode;
        }
    }
}