namespace BG3Types
{
    /// <summary>
    /// Extensions to facilitate working with collections.
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// Adds all elements from the <paramref name="source"/> to the <paramref name="target"/>.
        /// </summary>
        public static void AddAll<T, U>(this IList<T> target, IEnumerable<U> source) where U : T
        {
            foreach (var item in source)
            {
                target.Add(item);
            }
        }

        /// <summary>
        /// Attempts find an instance of <paramref name="key"/> in the dictionary. If it fails to find
        /// the entry, the <paramref name="factory"/> is used to create a default value and assign to the
        /// <paramref name="key"/>.
        /// </summary>
        /// <returns>Returns the entry if found, or the created one (added to the dictionary) if not.</returns>
        public static U FindOrAdd<T, U>(this IDictionary<T, U> dict, T key, Func<U> factory)
        {
            if (!dict.TryGetValue(key, out U? value) || value == null)
            {
                value = factory();
                dict[key] = value;
            }
            return value;
        }

        /// <summary>
        /// Attempts find an instance of <paramref name="key"/> in the dictionary. If it fails to find
        /// the entry, a new instance is created and assigned to the <paramref name="key"/>.
        /// </summary>
        /// <returns>Returns the entry if found, or the created one (added to the dictionary) if not.</returns>
        public static U FindOrAdd<T, U>(this IDictionary<T, U> dict, T key) where U : new()
        {
            if (!dict.TryGetValue(key, out U? value) || value == null)
            {
                value = new U();
                dict[key] = value;
            }
            return value;
        }

        /// <summary>
        /// Attempts find an instance of <paramref name="key"/> in the dictionary. If it fails to find
        /// the entry, a new instance is created and assigned to the <paramref name="key"/>.
        /// </summary>
        /// <returns>Returns the entry if found, or the created one (added to the dictionary) if not.</returns>
        public static U FindOrDefault<T, U>(this IDictionary<T, U> dict, T key, U defaultValue)
        {
            if (!dict.TryGetValue(key, out U? value) || value == null)
            {
                return defaultValue;
            }
            return value;
        }

        /// <summary>
        /// Attempts find an instance of <paramref name="key"/> in the dictionary. If it fails to find
        /// the entry, a new instance is created and assigned to the <paramref name="key"/>.
        /// </summary>
        /// <returns>Returns the entry if found, or the created one (added to the dictionary) if not.</returns>
        public static U FindOrDefault<T, U>(this IDictionary<T, U> dict, T key) where U : notnull
        {
            if (!dict.TryGetValue(key, out U? value) || value == null)
            {
                return default!;
            }
            return value;
        }

    }
}
