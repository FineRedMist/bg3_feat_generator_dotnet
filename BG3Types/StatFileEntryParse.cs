using System.Text.RegularExpressions;
using LSLib.LS;

namespace BG3Types
{
    public partial class StatFileEntry
    {
        private static readonly Regex ReStatEntry = new Regex(@"^new\s+entry\s+""(?<name>[^""]+)""\s*$", RegexOptions.Compiled);
        private static readonly Regex ReStatData = new Regex(@"data\s+""(?<key>[^""]+)""\s+""(?<values>[^""]+)""\s*$", RegexOptions.Compiled);
        private static readonly Regex ReStatUsing = new Regex(@"using\s+""(?<name>[^""]+)""\s*$", RegexOptions.Compiled);
        private static readonly Regex ReStatType = new Regex(@"type\s+""(?<type>[^""]+)""\s*$", RegexOptions.Compiled);
        private StatFileEntry()
        {
            Name = string.Empty;
            Type = string.Empty;
        }

        /// <summary>
        /// Parses stat file entries from the given <paramref name="file"/>.
        /// </summary>
        public static IEnumerable<StatFileEntry> Parse(PackagedFileInfo file)
        {
            using var content = file.CreateContentReader();
            using var reader = new StreamReader(content);

            StatFileEntry? current = null;

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                var m = ReStatEntry.Match(line);
                if (m.Success)
                {
                    if (current != null)
                    {
                        yield return current;
                    }
                    current = new StatFileEntry();
                    current.Name = m.Groups["name"].Value;
                }

                m = ReStatUsing.Match(line);
                if (m.Success)
                {
                    current!.Using = m.Groups["name"].Value;
                }

                m = ReStatType.Match(line);
                if (m.Success)
                {
                    current!.Type = m.Groups["type"].Value;
                }

                m = ReStatData.Match(line);
                if (m.Success)
                {
                    var key = m.Groups["key"].Value;
                    var values = m.Groups["values"].Value.Split(';');
                    current!.AddDataEntry(key, values);
                }
            }
            if(current != null && !string.IsNullOrEmpty(current.Name))
            {
                yield return current;
            }
        }
    }
}
