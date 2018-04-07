using System;

namespace SimpleCollections.Util
{
    [Serializable]
    public class ClassPair<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;
    }
}