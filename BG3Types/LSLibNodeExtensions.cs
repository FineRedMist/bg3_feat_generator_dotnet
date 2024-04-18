using System.Numerics;
using LSLib.LS;

namespace BG3Types
{
    /// <summary>
    /// Extensions for working with LSLib nodes in the LSX and similar files.
    /// </summary>
    public static class LSLibNodeExtensions
    {
        /// <summary>
        /// Default serialization settings.
        /// </summary>
        private static readonly NodeSerializationSettings SerializationSettings = new NodeSerializationSettings();

        /// <summary>
        /// Queries the <paramref name="node"/> for the given <paramref name="attributeName"/>. If found, returns the 
        /// parsed <seealso cref="bool"/> value, otherwise it returns the <paramref name="defaultValue"/> of false.
        /// </summary>
        public static bool GetBoolAttribute(this Node node, string attributeName, bool defaultValue = false)
        {
            NodeAttribute? attribute = null;
            if (node.Attributes.TryGetValue(attributeName, out attribute))
            {
                return (bool)attribute.Value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Queries the <paramref name="node"/> for the given <paramref name="attributeName"/>. If found, returns the 
        /// string value, otherwise it returns the <paramref name="defaultValue"/>.
        /// </summary>
        public static string? GetStringAttribute(this Node node, string attributeName, string? defaultValue)
        {
            NodeAttribute? attribute = null;
            if (node.Attributes.TryGetValue(attributeName, out attribute))
            {
                return attribute.AsString(SerializationSettings);
            }
            return defaultValue;
        }

        /// <summary>
        /// Queries the <paramref name="node"/> for the given <paramref name="attributeName"/>. If found, returns the 
        /// string value, otherwise an exception is thrown to indicate a missing expected node.
        /// </summary>
        public static string GetStringAttribute(this Node node, string attributeName)
        {
            NodeAttribute? attribute = null;
            if (node.Attributes.TryGetValue(attributeName, out attribute))
            {
                return attribute.AsString(SerializationSettings);
            }
            throw new Exception($"Missing the attribute {attributeName} for the node {node.Name}");
        }

        /// <summary>
        /// Queries the <paramref name="node"/> for the given <paramref name="attributeName"/>. If found, returns the 
        /// parsed <seealso cref="Guid"/> value, otherwise it returns <paramref name="defaultValue"/>.
        /// </summary>
        public static Guid? GetGuidAttribute(this Node node, string attributeName, Guid? defaultValue)
        {
            NodeAttribute? attribute = null;
            if (node.Attributes.TryGetValue(attributeName, out attribute))
            {
                return attribute.AsGuid();
            }
            return defaultValue;
        }

        /// <summary>
        /// Queries the <paramref name="node"/> for the given <paramref name="attributeName"/>. If found, returns the 
        /// parsed <seealso cref="Guid"/> value, otherwise an exception is thrown.
        /// </summary>
        public static Guid GetGuidAttribute(this Node node, string attributeName)
        {
            NodeAttribute? attribute = null;
            if (node.Attributes.TryGetValue(attributeName, out attribute))
            {
                return attribute.AsGuid();
            }
            throw new Exception($"Missing the attribute {attributeName} for the node {node.Name}");
        }

        /// <summary>
        /// Queries the <paramref name="node"/> for the given <paramref name="attributeName"/>. If found, returns the 
        /// parsed <typeparamref name="T"/> numeric value, otherwise the <paramref name="defaultValue"/> value is returned.
        /// </summary>
        public static T? GetNumericAttribute<T>(this Node node, string attributeName, T? defaultValue) where T : INumber<T>
        {
            NodeAttribute? attribute = null;
            if (node.Attributes.TryGetValue(attributeName, out attribute))
            {
                return T.Parse(attribute.AsString(SerializationSettings), null);
            }
            return defaultValue;
        }

        /// <summary>
        /// Queries the <paramref name="node"/> for the given <paramref name="attributeName"/>. If found, returns the 
        /// parsed <typeparamref name="T"/> numeric value, otherwise an exception is thrown.
        /// </summary>
        public static T GetNumericAttribute<T>(this Node node, string attributeName) where T : INumber<T>
        {
            NodeAttribute? attribute = null;
            if (node.Attributes.TryGetValue(attributeName, out attribute))
            {
                return T.Parse(attribute.AsString(SerializationSettings), null);
            }
            throw new Exception($"Missing the attribute {attributeName} for the node {node.Name}");
        }
    }
}