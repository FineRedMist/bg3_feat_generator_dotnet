namespace BG3Types
{
    /// <summary>
    /// General helper methods for BG3.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Normalizes a string (particularly the module name) into a string that is safe to use for spell, boost, and other asset names.
        /// </summary>
        /// <param name="name">The name to normalize.</param>
        /// <returns>A normalized name (containing only alphanumeric and underscore).</returns>
        public static string BG3Normalize(this string name)
        {
            char[] chars = name.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                var c = chars[i];
                if (!((c >= 'A' && c <= 'Z')
                    || (c >= 'a' && c <= 'z')
                    || (c >= '0' && c <= '9')))
                {
                    c = '_';
                }
                chars[i] = c;
            }
            return new string(chars);
        }

        /// <summary>
        /// Gets the string representation of the GUID in the format expected by BG3.
        /// </summary>
        /// <param name="guid">The guid to get the string representation of.</param>
        /// <returns>The string representation used by BG3.</returns>
        public static string ForBG3(this Guid guid)
        {
            return guid.ToString("D");
        }


        /// <summary>
        /// Helper function to append requirements to a list of requirements for a spell.
        /// </summary>
        /// <param name="requirements">The current set of requirements (may be null)</param>
        /// <param name="requirement">The requirement to add</param>
        /// <returns>An updated set of requirements that includes <paramref name="requirement"/>.</returns>
        public static string AddRequirement(string? requirements, string requirement)
        {
            if (string.IsNullOrEmpty(requirements))
            {
                return requirement;
            }
            return $"{requirements} and {requirement}";
        }

        /// <summary>
        /// Helper function to append requirements to a list of requirements for a spell.
        /// </summary>
        /// <param name="requirements">The current set of requirements (may be null)</param>
        /// <param name="requirementList">The requirement to add</param>
        /// <returns>An updated set of requirements that includes <paramref name="requirementList"/>.</returns>
        public static string? AddRequirements(string? requirements, IReadOnlyList<string> requirementList)
        {
            var result = requirements;
            foreach (var entry in requirementList)
            {
                result = AddRequirement(result, entry);
            }
            return result;
        }
    }
}