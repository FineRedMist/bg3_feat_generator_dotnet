using LSLib.LS;

namespace BG3Types
{
    /// <summary>
    /// Represents a module identifier.
    /// </summary>
    public class ModuleId
    {
        /// <summary>
        /// Guid of the module.
        /// </summary>
        public Guid Id { get; protected set; }
        /// <summary>
        /// Name of the module.
        /// </summary>
        public string? Name { get; private set; }
        /// <summary>
        /// Version of the module.
        /// </summary>
        public long Version { get; protected set; }

        /// <summary>
        /// Major version of the module.
        /// </summary>
        public int VersionMajor { get { return (int)(Version >> 55); } }
        /// <summary>
        /// Minor version of the module.
        /// </summary>
        public int VersionMinor { get { return (int)((Version >> 47) & 0xFF); } }
        /// <summary>
        /// Version revision of the module.
        /// </summary>
        public int VersionRevision { get { return (int)((Version >> 31) & 0xFFFF); } }
        /// <summary>
        /// Build number of the module.
        /// </summary>
        public int VersionBuild { get { return (int)(Version & 0xFFFFFFFF); } }

        /// <summary>
        /// Whether this <see cref="ModuleId"/> is valid (<seealso cref="Guid"/> is not empty and <seealso cref="Name"/> is not null or empty).
        /// </summary>
        public bool IsValid
        {
            get
            {
                return !Id.Equals(Guid.Empty)
                    && !string.IsNullOrEmpty(Name);
            }
        }

        /// <summary>
        /// Reads a <see cref="ModuleId"/> from the given <paramref name="node"/>.
        /// </summary>
        public void ReadAttributes(Node node)
        {
            Id = node.GetGuidAttribute("UUID");
            Name = node.GetStringAttribute("Name");
            Version = node.GetNumericAttribute<long>("Version64");
        }

        /// <summary>
        /// Default (empty) constructor.
        /// </summary>
        public ModuleId()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ModuleId"/> with the given <paramref name="name"/>."
        /// </summary>
        public ModuleId(string name)
        {
            Name = name;
        }
    }
}
