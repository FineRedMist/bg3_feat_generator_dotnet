using LSLib.LS;

namespace BG3Types
{
    /// <summary>
    /// Helper functions and extension for LSX files.
    /// </summary>
    public static class Lsx
    {
        /// <summary>
        /// Returns an <see cref="LSXReader"/> for the provided <paramref name="stream"/> with <see cref="NodeSerializationSettings.ByteSwapGuids"/> set to false.
        /// </summary>
        public static LSXReader Get(Stream stream)
        {
            var reader = new LSXReader(stream);
            reader.SerializationSettings.ByteSwapGuids = false;
            reader.SerializationSettings.DefaultByteSwapGuids = false;
            return reader;
        }

        /// <summary>
        /// Parses a semicolon delimited list of entries into a list of non-empty strings.
        /// </summary>
        public static IReadOnlyList<string> ParseList(string? list)
        {
            if (list == null)
            {
                return new string[0];
            }
            return list.Split(';')
                .Select(item => item.Trim())
                .Where(item => !string.IsNullOrEmpty(item))
                .ToArray();
        }
    }
}
