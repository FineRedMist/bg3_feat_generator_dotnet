using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BG3Types;
using LSLib.LS;

namespace FeatExtractor
{
    /// <summary>
    /// The list type as defined in the module the selector type references.
    /// </summary>
    public enum SelectorListType
    {
        /// <summary>
        /// Passives.
        /// </summary>
        Passive,
        /// <summary>
        /// Abilities.
        /// </summary>
        Ability,
        /// <summary>
        /// Skills.
        /// </summary>
        Skill,
        /// <summary>
        /// Spells.
        /// </summary>
        Spell
    }

    /// <summary>
    /// Extensions to facilitate selector use.
    /// </summary>
    public static class SelectorExtensions
    {
        /// <summary>
        /// Returns the module list type the given <paramref name="selectorType"/> refers to, or null if not found.
        /// </summary>
        public static SelectorListType GetListType(this SelectorType selectorType)
        {
            switch(selectorType)
            {
                case SelectorType.Passive: return SelectorListType.Passive;
                case SelectorType.Spell: return SelectorListType.Spell;
                case SelectorType.Skill: return SelectorListType.Skill;
                case SelectorType.Expertise: return SelectorListType.Skill;
                case SelectorType.Ability: return SelectorListType.Ability;
                default:
                    throw new KeyNotFoundException();
            }
        }
    }

    internal class Module : ModuleId
    {
        public string PakFile { get; private set; }

        public IDictionary<Guid, FeatDescription> Descriptions { get; private set; } = new Dictionary<Guid, FeatDescription>();
        public IList<Feat> Feats { get; private set; } = new List<Feat>();

        public IList<ModuleId> Dependencies { get; private set; } = new List<ModuleId>();

        public IDictionary<string, StatFileEntry> StatFiles { get; private set; } = new Dictionary<string, StatFileEntry>();

        public IDictionary<SelectorListType, IDictionary<Guid, ISet<string>>> Lists { get; private set; } = new Dictionary<SelectorListType, IDictionary<Guid, ISet<string>>>();

        private readonly Lazy<string> mNormalizedName;

        public bool IsInteresting
        {
            get
            {
                return IsValid
                    && (Descriptions.Count > 0
                        || Feats.Count > 0
                        || Lists.Count > 0);
            }
        }


        public string NormalizedModName
        {
            get
            {
                return mNormalizedName.Value;
            }
        }

        public Module(string pakFile, string moduleName)
            : base(moduleName)
        {
            PakFile = pakFile;
            mNormalizedName = new Lazy<string>(() =>
            {
                return Name!.BG3Normalize();
            });
        }

        delegate void ProcessFileDelegate(Module module, PackagedFileInfo package);


        private static readonly Dictionary<string, ProcessFileDelegate?> fileProcessors = new Dictionary<string, ProcessFileDelegate?>()
        {
            { "meta.lsx", ReadModule },
            { "feats.lsx", ReadFeats },
            { "featdescriptions.lsx", ReadFeatDescriptions },
        };

        public void ProcessFile(PackagedFileInfo packageFile)
        {
            var name = Path.GetFileName(packageFile.Name);
            var lookup = name.ToLowerInvariant();
            var lowerPath = packageFile.Name.ToLowerInvariant();
            ProcessFileDelegate? processor = null;
            if (fileProcessors.TryGetValue(lookup, out processor) && processor != null)
            {
                processor(this, packageFile);
            }

            var extension = Path.GetExtension(lookup);

            if (extension == ".txt"
                && lowerPath.IndexOf("/stats/generated/data/") >= 0)
            {
                ReadStats(this, packageFile);
            }

            if (extension == ".lsx"
                && lowerPath.IndexOf($"public/{Name!.ToLowerInvariant()}/lists/") >= 0)
            {
                ReadList(this, packageFile);
            }
        }

        private class ListInfo
        {
            public IReadOnlyList<string> Entries { get; private set; }
            public char Separator { get; private set; }

            public ListInfo(char separator, params string[] entries)
            {
                Entries = entries;
                Separator = separator;
            }
        }

        private static readonly IReadOnlyDictionary<SelectorListType, ListInfo> ListNodes = new Dictionary<SelectorListType, ListInfo>()
        {
            { SelectorListType.Passive, new ListInfo ( ',', "PassiveLists", "PassiveList", "Passives" ) },
            { SelectorListType.Ability, new ListInfo ( ',', "AbilityLists", "AbilityList", "Abilities" ) },
            { SelectorListType.Skill, new ListInfo ( ',', "SkillLists", "SkillList", "Skills" ) },
            { SelectorListType.Spell, new ListInfo ( ',', "SpellLists", "SpellList", "Spells" ) },
        };

        private static void ReadListType(Module module, Region region, SelectorListType listType, ListInfo info)
        {
            var listElements = region.Children[info.Entries[1]];
            if (listElements == null)
            {
                return;
            }

            var listContainer = module.Lists.FindOrAdd(listType, () => new Dictionary<Guid, ISet<string>>());

            foreach (var elem in listElements)
            {
                var id = elem.GetGuidAttribute("UUID");
                var list = elem.GetStringAttribute(info.Entries[2]);
                var items = list.Split(info.Separator)
                    .Select(item => item.Trim())
                    .Where(item => !string.IsNullOrEmpty(item))
                    .ToHashSet();

                listContainer[id] = items;
            }
        }

        private static void ReadList(Module module, PackagedFileInfo package)
        {
            using var content = package.CreateContentReader();
            using var lsx = Lsx.Get(content);
            var resource = lsx.Read();
            var regions = resource.Regions;

            foreach (var nodeType in ListNodes)
            {
                if (regions.TryGetValue(nodeType.Value.Entries[0], out var listRegion))
                {
                    if (listRegion == null)
                    {
                        continue;
                    }
                    ReadListType(module, listRegion, nodeType.Key, nodeType.Value);
                }
            }

        }
        private static void ReadStats(Module module, PackagedFileInfo package)
        {
            foreach (var stat in StatFileEntry.Parse(package))
            {
                module.StatFiles.Add(stat.Name, stat);
            }
        }

        private static void ReadFeats(Module module, PackagedFileInfo package)
        {
            module.Feats.AddAll(Feat.Read(package));
        }
        private static void ReadFeatDescriptions(Module module, PackagedFileInfo package)
        {
            foreach (var description in FeatDescription.Read(package))
            {
                module.Descriptions.Add(description.Id, description);
            }
        }

        private static void ReadModule(Module module, PackagedFileInfo packageFile)
        {
            using var content = packageFile.CreateContentReader();
            using var lsx = Lsx.Get(content);
            ReadModule(module, lsx.Read());
        }

        private static void ReadModule(Module module, Resource resource)
        {
            var config = resource.Regions["Config"].Children;
            var moduleInfo = config["ModuleInfo"][0];
            module.ReadAttributes(moduleInfo);

            if (!module.IsValid)
            {
                return;
            }

            Console.WriteLine($"    Parsed mod: {module.Name}, version: {module.Version}");
            List<Node>? dependencies = null;
            if (config.TryGetValue("Dependencies", out dependencies)
                && dependencies != null
                && dependencies.Count > 0
                && dependencies[0].ChildCount > 0)
            {
                foreach (var node in dependencies[0].Children)
                {
                    if (node.Key != "ModuleShortDesc")
                    {
                        continue;
                    }
                    foreach (var depModule in node.Value)
                    {
                        ModuleId dep = new ModuleId();
                        dep.ReadAttributes(depModule);
                        if (dep.IsValid)
                        {
                            Console.WriteLine($"      Parsed dependency mod: {dep.Name}, version: {dep.Version}");
                            module.Dependencies.Add(dep);
                        }
                    }
                }
            }

        }

        public void MergeFrom(Module module)
        {
            if (!module.Id.Equals(Guid.Empty))
            {
                Id = module.Id;
            }
            if (module.Version != 0)
            {
                Version = module.Version;
            }

            if (module.Descriptions != null && module.Descriptions.Count > 0)
            {
                Descriptions = module.Descriptions;
            }
            if (module.Feats != null && module.Feats.Count > 0)
            {
                Feats = module.Feats;
            }
            if (module.Dependencies != null && module.Dependencies.Count > 0)
            {
                Dependencies = module.Dependencies;
            }
            foreach (var stat in module.StatFiles)
            {
                StatFiles[stat.Key] = stat.Value;
            }
            foreach (var list in module.Lists)
            {
                foreach (var id in list.Value)
                {
                    var current = Lists.FindOrAdd(list.Key, () => new Dictionary<Guid, ISet<string>>());
                    current[id.Key] = id.Value;
                }
            }
        }

        public void TrimDependencies<T>(IReadOnlyDictionary<string, T> trackedModules)
        {
            var trackedDependencies = Dependencies
                .Where(dep => !string.IsNullOrEmpty(dep.Name) && trackedModules.ContainsKey(dep.Name))
                .ToList();
            Dependencies = trackedDependencies;
        }

    }
}