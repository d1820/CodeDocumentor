using System.Collections.Generic;

namespace CodeDocumentor.Helper
{
    public static class DictionaryExtensions
    {
        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, value);
                return true;
            }
            return false;
        }

    }
}
