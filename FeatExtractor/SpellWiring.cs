using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BG3Types;

namespace FeatExtractor
{
    /// <summary>
    /// Contains a mapping of modules to the list of spells to enable for that module for the given parent spell.
    /// </summary>
    public class TierSpellWiring : Dictionary<string, List<string>>
    {
        /// <summary>
        /// The lua code will searh for the latest version of the <paramref name="moduleName"/> found in the dictionary
        /// and add the given <paramref name="spellName"/> to the list of spells to add for the parent spell.
        /// </summary>
        public void Add(string moduleName, string? spellName = null)
        {
            var featList = this.FindOrAdd(moduleName);
            if (!string.IsNullOrEmpty(spellName))
            {
                featList.Add(spellName);
            }
        }

        /// <summary>
        /// Whether there are any interesting entries in this tier to include.
        /// </summary>
        public bool HasEntries
        {
            get
            {
                return Count > 0 && this.Any(pair => pair.Value.Count > 0);
            }
        }
    }

    /// <summary>
    /// Maps a spell to the set of various potential other spells to attach as child spells for that tier.
    /// </summary>
    public class SpellWiring : Dictionary<string, List<TierSpellWiring>>
    {
        /// <summary>
        /// Adds the mapping of the modules and child spells for a given tier to the parent spell.
        /// </summary>
        public void Add(string parentSpellName, TierSpellWiring tier)
        {
            var moduleSpells = this.FindOrAdd(parentSpellName);
            moduleSpells.Add(tier);
        }

        /// <summary>
        /// Removes tiers that are empty and then any container spells with nothing to assign.
        /// </summary>
        public void Clean()
        {
            List<string> spellsToRemove = new List<string>();
            foreach (var containerSpell in this)
            {
                for (int i = containerSpell.Value.Count - 1; i >= 0; --i)
                {
                    var tier = containerSpell.Value[i];
                    if (!tier.HasEntries)
                    {
                        containerSpell.Value.RemoveAt(i);
                    }
                }
                if (containerSpell.Value.Count == 0)
                {
                    spellsToRemove.Add(containerSpell.Key);
                }
            }
            foreach (var key in spellsToRemove)
            {
                this.Remove(key);
            }
        }
    }

    /// <summary>
    /// Gathers all the data for spells to be generated into the wiring json file,
    /// and each of the boost and shout files.
    /// </summary>
    public class SpellCollector
    {
        /// <summary>
        /// The wiring of parent spells to child spells based on the presence of modules.
        /// </summary>
        private readonly SpellWiring mSpellWiring = new SpellWiring();
        /// <summary>
        /// The collectino of all the boost stat entries for each module.
        /// </summary>
        private readonly IDictionary<string, List<StatFileEntry>> mModuleBoosts = new Dictionary<string, List<StatFileEntry>>();
        /// <summary>
        /// The collectino of all shout spells for each module.
        /// </summary>
        private readonly IDictionary<string, List<StatFileEntry>> mModuleShouts = new Dictionary<string, List<StatFileEntry>>();

        /// <summary>
        /// Adds a <paramref name="tier"/> of spells to the parent spell <paramref name="parentSpellName"/>.
        /// </summary>
        public void Add(string parentSpellName, TierSpellWiring tier)
        {
            mSpellWiring.Add(parentSpellName, tier);
        }

        /// <summary>
        /// Adds a boost stat <paramref name="entry"/> for the given <paramref name="moduleName"/>.
        /// </summary>
        public void AddBoost(string moduleName, StatFileEntry entry)
        {
            var list = mModuleBoosts.FindOrAdd(moduleName);
            list.Add(entry);
        }

        /// <summary>
        /// Adds a spell stat <paramref name="entry"/> for the given <paramref name="moduleName"/>.
        /// </summary>
        public void AddSpell(string moduleName, StatFileEntry entry)
        {
            var list = mModuleShouts.FindOrAdd(moduleName);
            list.Add(entry);
        }

        /// <summary>
        /// Writes out the files based on the collected spells, boosts, and wiring to <paramref name="targetPath"/>
        /// </summary>
        public void GenerateFiles(string targetPath)
        {
            mSpellWiring.Clean();

            foreach(var file in Directory.EnumerateFiles(targetPath, "E6_Gen_*.*"))
            {
                File.Delete(file);
            }
            foreach (var module in mModuleBoosts)
            {
                GenerateStatEntries(Path.Combine(targetPath, $"E6_Gen_{module.Key}_Boosts.txt"), module.Value);
            }
            foreach (var module in mModuleShouts)
            {
                GenerateStatEntries(Path.Combine(targetPath, $"E6_Gen_{module.Key}_Shouts.txt"), module.Value);
            }

            var text = JsonSerializer.Serialize(mSpellWiring, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(targetPath, "E6_Gen_Wiring.json"), text);
        }

        private static void GenerateStatEntries(string targetFile, IReadOnlyList<StatFileEntry> entries)
        {
            using var file = File.OpenWrite(targetFile);
            using var writer = new StreamWriter(file);

            foreach (var entry in entries)
            {
                writer.WriteLine(entry.ToStatEntry());
            }

        }
    }
}