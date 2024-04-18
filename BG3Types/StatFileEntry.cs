using System.Text;

namespace BG3Types
{
    /// <summary>
    /// Represents a stat entry from Stats/Generated/Data text file.
    /// </summary>
    public partial class StatFileEntry
    {
        /// <summary>
        /// The name of the stat entry.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// The type (like shout, boost, etc).
        /// </summary>
        public string Type { get; private set; }
        /// <summary>
        /// A reference to another stat entry being used to inherit a number of properties.
        /// </summary>
        public string? Using { get; private set; }

        private readonly IDictionary<string, List<string>> mDataEntries = new Dictionary<string, List<string>>();

        private readonly IDictionary<string, string> mCustomJoinSeparator = new Dictionary<string, string>();

        /// <summary>
        /// Retrieves a data entry for the given <paramref name="key"/>. Returns null if the key does not exist.
        /// </summary>
        public IReadOnlyList<string>? this[string key]
        {
            get
            {
                if (mDataEntries.TryGetValue(key, out var values))
                {
                    return values;
                }
                return null;
            }
        }

        /// <summary>
        /// Creates a new stat entry with the given <paramref name="name"/> and <paramref name="type"/>.
        /// </summary>
        public StatFileEntry(string name, string type)
        {
            Name = name;
            Type = type;
            mCustomJoinSeparator["RequirementConditions"] = " and ";
        }

        /// <summary>
        /// Creates a new stat entry with the given <paramref name="name"/> and <paramref name="type"/>, inheriting properties from <paramref name="usingName"/>.
        /// </summary>
        public StatFileEntry(string name, string type, string usingName)
            : this(name, type)
        {
            Using = usingName;
        }

        /// <summary>
        /// Adds a collection of <paramref name="values"/> to the data entry with the given <paramref name="key"/>.
        /// 
        /// The entries are joined with semicolons. If the join needs to be by another method, then a single entry with all target values pre-joined should be passed.
        /// </summary>
        public void AddDataEntry(string key, params string?[] values)
        {
            AddDataEntry(key, (IEnumerable<string?>)values);
        }

        /// <summary>
        /// Adds a collection of <paramref name="values"/> to the data entry with the given <paramref name="key"/>.
        /// 
        /// The entries are joined with semicolons. If the join needs to be by another method, then a single entry with all target values pre-joined should be passed.
        /// </summary>
        public void AddDataEntry(string key, IEnumerable<string?> values)
        {
            var entries = new Lazy<List<string>>(() => { return mDataEntries.FindOrAdd(key); });
            foreach (var value in values)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    entries.Value.Add(value);
                }
            }
        }

        /// <summary>
        /// Generates the stat entry to write to the Stats/Generated/Data text file.
        /// </summary>
        public string ToStatEntry()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"new entry \"{Name}\"");
            builder.AppendLine($"type \"{Type}\"");
            if (!string.IsNullOrEmpty(Using))
            {
                builder.AppendLine($"using \"{Using}\"");
            }
            foreach (var entry in mDataEntries)
            {
                var separator = mCustomJoinSeparator.TryGetValue(entry.Key, out var sep) ? sep : ";";
                builder.AppendLine($"data \"{entry.Key}\" \"{string.Join(separator, entry.Value)}\"");
            }

            return builder.ToString();
        }
    }
}
