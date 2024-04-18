using System.Text.RegularExpressions;

namespace BG3Types
{
    /// <summary>
    /// The kind of selector for choosing options for feats.
    /// </summary>
    public enum SelectorType
    {
        /// <summary>
        /// Unknown/unidentified selector.
        /// </summary>
        Unknown,
        /// <summary>
        /// Strength, Constitution, Dexterity, Wisdom, Intelligence, Charisma (may be more limited than this).
        /// </summary>
        Ability,
        /// <summary>
        /// Skills that can gain proficiency.
        /// </summary>
        Skill,
        /// <summary>
        /// Skills that can gain expertise.
        /// </summary>
        Expertise,
        /// <summary>
        /// A passive ability (custom generated).
        /// </summary>
        Passive,
        /// <summary>
        /// A spell to select.
        /// </summary>
        Spell
    }

    /// <summary>
    /// An attribute to indicate the text to match against for a cheap filter for parsing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SelectorFunctionAttribute : Attribute
    {
        /// <summary>
        /// The name of the function to test against to eliminate 
        /// </summary>
        public string FunctionName { get; set; }

        /// <summary>
        /// Constructor for assigning the function name.
        /// </summary>
        public SelectorFunctionAttribute(string functionName)
        {
            FunctionName = functionName;
        }
    }

    /// <summary>
    /// Base interface for a selector.
    /// </summary>
    public interface ISelector
    {
        /// <summary>
        /// Generic <seealso cref="Regex"/> for a <seealso cref="Guid"/>.
        /// </summary>
        protected static string GuidRegex = "[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}";
        /// <summary>
        /// Generic <seealso cref="Regex"/> for an identifier.
        /// </summary>
        protected static string Identifier = "[a-zA-Z0-9_]+";
        /// <summary>
        /// The <seealso cref="SelectorType"/> for the selector.
        /// </summary>
        public SelectorType Type { get; }
        /// <summary>
        /// The list id to use for selecting entries.
        /// </summary>
        public Guid ListId { get; }

        private delegate ISelector? SelectorParserDelegate(string input);

        private static readonly SelectorParserDelegate[] mParsers = new SelectorParserDelegate[]
        {
            SelectorAbility.Parse,
            SelectorSkill.Parse,
            SelectorExpertise.Parse,
            SelectorPassive.Parse,
            SelectorSpell.Parse
        };

        /// <summary>
        /// Generic unknown selector to ensure we identify a selector is occurring even if we don't successfully identify it.
        /// </summary>
        private class UnknownSelector : ISelector
        {
            public SelectorType Type { get { return SelectorType.Unknown; } }
            public Guid ListId { get { return Guid.Empty; } }
            public string SelectorText { get; private set; }

            public UnknownSelector(string text)
            {
                SelectorText = text;
            }
        }

        /// <summary>
        /// Parses the given <paramref name="input"/> into the corresponding selector.
        /// </summary>
        public static ISelector Parse(string input)
        {
            input = input.Trim();
            foreach (var parser in mParsers)
            {
                var attribute = parser.Method.GetCustomAttributes(typeof(SelectorFunctionAttribute), false)
                    .Cast<SelectorFunctionAttribute>()
                    .FirstOrDefault();
                if (attribute != null
                    && !input.ToLowerInvariant().StartsWith(attribute.FunctionName))
                {
                    continue;
                }
                var result = parser(input);
                if (result != null)
                {
                    return result;
                }
            }
            Console.WriteLine($"    Failed to parse selector: {input}");
            return new UnknownSelector(input);
        }
    }


    /// <summary>
    /// A selector for increasing an ability score.
    /// </summary>
    public class SelectorAbility : ISelector
    {
        /// <summary>
        /// The type of the selector: <see cref="SelectorType.Ability"/>.
        /// </summary>
        public SelectorType Type { get { return SelectorType.Ability; } }
        /// <summary>
        /// The list id in the abilitylist.xml to draw the list of abilties from.
        /// </summary>
        public Guid ListId { get; private set; }

        /// <summary>
        /// The number of abilities to select.
        /// </summary>
        public int Count { get; private set; }
        /// <summary>
        /// The maximum score for each ability.
        /// </summary>
        public int Max { get; private set; }
        /// <summary>
        /// The label to apply for the ability boost to show in the UI correctly.
        /// </summary>
        public string Label { get; private set; }

        private SelectorAbility(Guid id, int count, int max, string label)
        {
            ListId = id;
            Count = count;
            Max = max;
            Label = label;
        }

        private static readonly string mParserString = string.Format(@"^SelectAbilities\s*\(\s*(?<id>{0})\s*,\s*(?<count>\d+)\s*,\s*(?<max>\d+)\s*,\s*(?<tag>{1})\s*\)$", ISelector.GuidRegex, ISelector.Identifier);
        private static readonly Regex mParser = new Regex(mParserString, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        ///  SelectAbilities(/guid/,/count?/,/points?/,/build in category? can't find in assets/)
        /// </summary>
        [SelectorFunction("selectabilities")]
        public static ISelector? Parse(string input)
        {
            Match m = mParser.Match(input);
            if (!m.Success)
            {
                return null;
            }
            var id = Guid.Parse(m.Groups["id"].Value);
            var count = int.Parse(m.Groups["count"].Value);
            var max = int.Parse(m.Groups["max"].Value);
            var tag = m.Groups["tag"].Value;

            return new SelectorAbility(id, count, max, tag);
        }
    }

    internal class SelectorSkill : ISelector
    {
        public SelectorType Type { get { return SelectorType.Skill; } }
        public Guid ListId { get; private set; }

        public int Count { get; private set; }

        public string? Label { get; private set; }

        private SelectorSkill(Guid id, int count, string? label)
        {
            ListId = id;
            Count = count;
            Label = label;
        }

        private static readonly Regex mParser = new Regex($@"^SelectSkills\s*\(\s*(?<id>{ISelector.GuidRegex})\s*,\s*(?<count>\d+)\s*(,\s*(?<tag>{ISelector.Identifier})\s*)?\)$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// SelectSkills(233793b3-838a-4d4e-9d68-1e0a1089aba5,2)
        /// SelectSkills(f974ebd6-3725-4b90-bb5c-2b647d41615d,1,HumanVersatility)
        /// </summary>
        [SelectorFunction("selectskills")]
        public static ISelector? Parse(string input)
        {
            Match m = mParser.Match(input);
            if (!m.Success)
            {
                return null;
            }
            var id = Guid.Parse(m.Groups["id"].Value);
            var count = int.Parse(m.Groups["count"].Value);
            string? tag = null;
            if (m.Groups.TryGetValue("tag", out Group? value) && value != null)
            {
                tag = value.Value;
            }

            return new SelectorSkill(id, count, tag);
        }
    }

    internal class SelectorExpertise : ISelector
    {
        public SelectorType Type { get { return SelectorType.Expertise; } }
        public Guid ListId { get; private set; }

        public int Count { get; private set; }

        public bool? Unknown { get; private set; }

        private SelectorExpertise(Guid id, int count, bool? unknown)
        {
            ListId = id;
            Count = count;
            Unknown = unknown;
        }

        private static readonly Regex mParser = new Regex($@"^SelectSkillsExpertise\s*\(\s*(?<id>{ISelector.GuidRegex})\s*,\s*(?<count>\d+)\s*(,\s*(?<unknown>(true|false))\s*)?\)$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// SelectSkillsExpertise(f974ebd6-3725-4b90-bb5c-2b647d41615d,2)
        /// SelectSkillsExpertise(f974ebd6-3725-4b90-bb5c-2b647d41615d,2,true)
        /// </summary>
        [SelectorFunction("selectskillsexpertise")]
        public static ISelector? Parse(string input)
        {
            Match m = mParser.Match(input);
            if (!m.Success)
            {
                return null;
            }
            var id = Guid.Parse(m.Groups["id"].Value);
            var count = int.Parse(m.Groups["count"].Value);
            bool? unknown = null;
            if (m.Groups.TryGetValue("unknown", out Group? value) && value != null)
            {
                unknown = bool.Parse(value.Value);
            }

            return new SelectorExpertise(id, count, unknown);
        }
    }

    internal class SelectorPassive : ISelector
    {
        public SelectorType Type { get { return SelectorType.Passive; } }
        public Guid ListId { get; private set; }

        public int Count { get; private set; }

        public string? Category { get; private set; }

        private SelectorPassive(Guid id, int count, string? category)
        {
            ListId = id;
            Count = count;
            Category = category;
        }

        private static readonly Regex mParser = new Regex($@"^SelectPassives\s*\(\s*(?<id>{ISelector.GuidRegex})\s*,\s*(?<count>\d+)\s*(,\s*(?<tag>{ISelector.Identifier})\s*)?\)$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// SelectPassives(f8ebba38-932a-4c64-ae55-3df23e2f60fa,1,FightingStyle)
        /// SelectPassives(f6b6e71f-79b1-4ba3-8fd8-ee38a44d3d39,1)
        /// </summary>
        [SelectorFunction("selectpassives")]
        public static ISelector? Parse(string input)
        {
            Match m = mParser.Match(input);
            if (!m.Success)
            {
                return null;
            }
            var id = Guid.Parse(m.Groups["id"].Value);
            var count = int.Parse(m.Groups["count"].Value);
            string? tag = null;
            if (m.Groups.TryGetValue("tag", out Group? value) && value != null)
            {
                tag = value.Value;
            }

            return new SelectorPassive(id, count, tag);
        }
    }

    /// <summary>
    /// TODO: Implement spell selector.
    /// </summary>
    internal class SelectorSpell : ISelector
    {
        public SelectorType Type { get { return SelectorType.Spell; } }
        public Guid ListId { get { return Guid.Empty; } }

        public static ISelector? Parse(string input)
        {
            return null;
        }
    }
}