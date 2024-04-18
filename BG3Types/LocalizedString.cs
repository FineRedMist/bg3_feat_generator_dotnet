namespace BG3Types
{
    /// <summary>
    /// Represents a reference to a localized string consisting of a string handle and a version of the string.
    /// </summary>
    public class LocalizedString
    {
        /// <summary>
        /// Handle to the localized string in the loca files.
        /// </summary>
        public string Handle { get; private set; }
        /// <summary>
        /// The version of the localized string to use.
        /// </summary>
        public int? Version { get; private set; }

        /// <summary>
        /// Creates a new localized string reference based on a string reference that could be a handle, or {handle};{version}.
        /// </summary>
        public LocalizedString(string handle)
        {
            int semicolonIndex = handle.IndexOf(';');
            if (semicolonIndex != -1)
            {
                Handle = handle.Substring(0, semicolonIndex);
                Version = int.Parse(handle.Substring(semicolonIndex + 1));
            }
            else
            {
                Handle = handle;
            }
        }

        /// <summary>
        /// Creates a new localized string reference based on a string reference with the given <paramref name="handle"/> and <paramref name="version"/>.
        /// </summary>
        public LocalizedString(string handle, int version)
        {
            Handle = handle;
            Version = version;
        }

        /// <summary>
        /// Generates the string representation of the localized string reference.
        /// Either {handle} (if no version) or {handle};{version}.
        /// </summary>
        public override string ToString()
        {
            if (Version.HasValue)
            {
                return $"{Handle};{Version}";
            }
            else
            {
                return Handle;
            }
        }
    }

}