using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BG3Types;
using LSLib.LS;

namespace FeatExtractor
{
    internal class PakFile
    {
        private static readonly Regex GetModNameFromPath = new Regex("^[^/]+/(?<mod>[^/]+)/.*$");

        public IDictionary<string, Module> Modules { get; private set; } = new Dictionary<string, Module>();

        private static string? GetModuleNameFromPath(string path)
        {
            var match = GetModNameFromPath.Match(path);
            if (!match.Success)
            {
                return null;
            }
            return match.Groups["mod"].Captures[0].Value;
        }

        public void Read(string pakFile, List<PackagedFileInfo> files)
        {
            foreach (var packageFile in files)
            {
                var modName = GetModuleNameFromPath(packageFile.Name);
                if (string.IsNullOrEmpty(modName))
                {
                    Console.WriteLine($"  Skipping: {packageFile.Name}");
                    continue;
                }
                Module? module = null;
                if (!Modules.TryGetValue(modName, out module))
                {
                    module = new Module(pakFile, modName);
                    Modules.Add(modName, module);
                }
                module.ProcessFile(packageFile);
            }
        }
    }

    internal class GameInfo
    {
        public IDictionary<string, List<Module>> ParsedModules { get; private set; } = new Dictionary<string, List<Module>>();

        public IReadOnlyList<Module> Modules { get; private set; } = new List<Module>();

        public void ReadPakFile(string pakFile)
        {
            try
            {
                var pak = new PakFile();
                var reader = new PackageReader();
                using var package = reader.Read(pakFile);
                if (package == null)
                {
                    return;
                }

                pak.Read(pakFile, package.Files);

                foreach (var module in pak.Modules)
                {
                    foreach(var stat in module.Value.StatFiles)
                    {
                        if(stat.Value["ContainerSpellID"] != null && stat.Value["ContainerSpells"] != null)
                        {
                            Console.WriteLine($"  Module: {module.Key}, Stat Entry: {stat.Value.Name}");
                        }
                    }
                    if (module.Value.IsInteresting)
                    {
                        List<Module>? namedModules = ParsedModules.FindOrAdd(module.Key);
                        namedModules.Add(module.Value);
                        Console.WriteLine($"  Found Module: {module.Key}");
                        foreach (var dependency in module.Value.Dependencies)
                        {
                            Console.WriteLine($"    Depends on: {dependency.Name}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Skipping pak due to: {ex.Message}");
            }
        }

        public void GatherModules(string gameInstallPath)
        {
            foreach (var pakFile in Directory.EnumerateFiles(gameInstallPath, "*.pak", SearchOption.AllDirectories))
            {
                Console.WriteLine($"{gameInstallPath}: Reading: {pakFile}");
                ReadPakFile(pakFile);
            }
        }

        static void GetNexusMod(string id, string nexusApiKey)
        {
            // Url to get file list:
            // https://api.nexusmods.com/v1/games/baldursgate3/mods/768/files.json?category=main
            //  Host headers:
            //      accept: application/json
            //      apikey: <key>
            // for each file found
            //  download
            //  get module load order
            //  get module id
            //  get feats
            //  get any new feats and data
        }

        public void Merge()
        {
            // Merge the module information across patches, etc.
            var mergedModules = new Dictionary<string, Module>();
            foreach (var module in ParsedModules)
            {
                if (module.Value.Count == 0)
                {
                    continue;
                }
                var target = new Module(string.Empty, module.Key);

                foreach (var entry in module.Value.OrderBy(mod => mod.Version))
                {
                    target.MergeFrom(entry);
                }
                mergedModules[target.Name!] = target;
            }

            // Remove dependencies we don't care about
            foreach (var module in mergedModules)
            {
                module.Value.TrimDependencies(mergedModules);
            }

            // Now determine the loading order (as best as possible).
            var added = new HashSet<string>();
            var ordered = new List<Module>
            {
                mergedModules["Shared"],
                mergedModules["SharedDev"]
            };
            mergedModules.Remove("Shared");
            mergedModules.Remove("SharedDev");
            added.Add("Shared");
            added.Add("SharedDev");

            while (mergedModules.Count > 0)
            {
                Module? moduleToAdd = null;
                foreach (var module in mergedModules)
                {
                    moduleToAdd = module.Value;
                    foreach (var dependency in module.Value.Dependencies)
                    {
                        if (!added.Contains(dependency.Name!))
                        {
                            moduleToAdd = null;
                            break;
                        }
                    }
                    if (moduleToAdd != null)
                    {
                        break;
                    }
                }
                if (moduleToAdd != null)
                {
                    ordered.Add(moduleToAdd);
                    added.Add(moduleToAdd.Name!);
                    mergedModules.Remove(moduleToAdd.Name!);
                }
            }

            Modules = ordered;
        }

        public void GenerateFiles(string targetPath)
        {
            var spellCollector = new SpellCollector();
            FeatWeaver helper = new FeatWeaver(Modules.Reverse().ToArray(), spellCollector);
            helper.Generate();
            spellCollector.GenerateFiles(targetPath);
        }
    }
}