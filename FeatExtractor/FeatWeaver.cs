using System;
using System.Collections.Generic;
using System.Linq;
using BG3Types;

namespace FeatExtractor
{
    internal static class FeatExtensions
    {
        /// <summary>
        /// Whether the feat is currently supported for export.
        /// </summary>
        public static bool IsSupported(this Feat feat)
        {
            return feat.Selectors.Count == 0
                || feat.Selectors.All(selector => selector.Type == SelectorType.Ability);
        }
    }

    internal class FeatWeaver
    {
        private readonly SpellCollector mSpellCollector;
        private readonly IReadOnlyList<Module> mModules;

        public FeatWeaver(IReadOnlyList<Module> modules, SpellCollector spellWiring)
        {
            mModules = modules;
            mSpellCollector = spellWiring;
        }

        private class EntryDescription
        {
            public LocalizedString? DisplayName { get; set; } = null;
            public LocalizedString? Description { get; set; } = null;
            public string Icon { get; set; } = string.Empty;

            public EntryDescription()
            {
            }

            public EntryDescription(string displayName, string description, string icon)
            {
                DisplayName = new LocalizedString(displayName);
                Description = new LocalizedString(description);
                Icon = icon;
            }
        }

        /// <summary>
        /// Top level function that goes through each of the feats and generates the tier wiring.
        /// Selectors trigger recursion to generate the spells for each of the selectors.
        /// </summary>
        public void Generate()
        {
            var baseSpellContainerId = "E6_Shout_EpicFeats";
            var featIds = mModules.SelectMany(m => m.Feats.Select(feat => feat.Id)).Distinct().ToArray();
            foreach (var featId in featIds)
            {
                var tier = new TierSpellWiring();
                mSpellCollector.Add(baseSpellContainerId, tier);
                for (int moduleIndex = 0; moduleIndex < mModules.Count; ++moduleIndex)
                {
                    var module = mModules[moduleIndex];
                    var feat = module.Feats.FirstOrDefault(f => f.Id == featId);
                    if (feat == null)
                    {
                        continue;
                    }
                    if (!feat.IsSupported())
                    {
                        // We still need to add the module to the tier if it is unsupported, but with an empty set 
                        // so that an incorrect feat isn't added (in case a subsequent module overrides it).
                        tier.Add(module.NormalizedModName);
                        continue;
                    }
                    var description = GetEntryDescription(moduleIndex, feat);
                    Generate($"{feat.Name}_{module.NormalizedModName}", moduleIndex, feat, description, baseSpellContainerId, tier, new SelectorCollector(feat.Selectors));
                }
            }
        }

        private void Generate(string partialPrefix, int moduleIndex, Feat feat, EntryDescription description, string spellContainerId, TierSpellWiring tier, SelectorCollector selectors)
        {
            var module = mModules[moduleIndex];
            var spell = GenerateSpell(!selectors.IsComplete, $"E6_Shout_{partialPrefix}", spellContainerId, feat, description, selectors.GatherRequirements());
            mSpellCollector.AddSpell(module.NormalizedModName, spell);

            tier.Add(module.NormalizedModName, spell.Name);

            if (selectors.IsComplete)
            {
                // Here is where we actually generate the boost with all of the selector boosts, and the base spell that does real work.
                var boostName = $"E6_FEAT_{partialPrefix}";
                var boost = GenerateBoost(boostName, feat, selectors.GatherBoosts());
                mSpellCollector.AddBoost(module.NormalizedModName, boost);

                // Update the spell to use the boost.
                spell.AddDataEntry("SpellProperties", $"ApplyStatus({boostName},-1,-1)");
                return;
            }

            var currentSelector = selectors.Selectors.First();
            // Look up the corresponding list for the given selector and generate an entry for each possible combination, for each module that has a different list.
            var selectorTier = new TierSpellWiring();
            var selectorList = currentSelector.Type.GetListType();
            mSpellCollector.Add(spell.Name, selectorTier);

            for (var subModuleIndex = 0; subModuleIndex <  mModules.Count; ++subModuleIndex)
            {
                var subModule = mModules[subModuleIndex];
                if(!subModule.Lists.TryGetValue(selectorList!, out var selectionSet))
                {
                    continue;
                }
                if (!selectionSet.TryGetValue(currentSelector.ListId, out var selections) || selections == null)
                {
                    continue;
                }

                var cases = selectors.GatherCases(selections);

                foreach(var cas in cases)
                {
                    string newPrefix = $"{partialPrefix}_{cas.Item1}_{subModule.NormalizedModName}";
                    var newDescription = GetEntryDescriptionAbility(cas.Item1);
                    Generate(newPrefix, subModuleIndex, feat, newDescription, spell.Name, selectorTier, cas.Item2);
                }
                break;
            }
        }

        private StatFileEntry GenerateSpell(bool isContainer, string spellName, string spellContainerId, Feat feat, EntryDescription description, IEnumerable<string> selectorRequirements)
        {
            StatFileEntry spell = isContainer ? new SpellContainerTemplateEntry(spellName) : new SpellTemplateEntry(spellName);
            spell.AddDataEntry("SpellContainerID", spellContainerId);
            spell.AddDataEntry("RequirementConditions", feat.Requirements);
            spell.AddDataEntry("RequirementConditions", selectorRequirements);
            if(!feat.CanBeTakenMultipleTimes)
            {
                spell.AddDataEntry("RequirementConditions", feat.PassivesAdded.Select(passive => $"not HasPassive('{passive}', context.Source)"));
            }

            if (description.DisplayName != null)
            {
                spell.AddDataEntry("DisplayName", description.DisplayName.ToString());
            }
            if (description.Description != null)
            {
                spell.AddDataEntry("Description", description.Description.ToString());
            }
            if (!string.IsNullOrEmpty(description.Icon))
            {
                spell.AddDataEntry("Icon", description.Icon);
            }

            return spell;
        }

        private StatFileEntry GenerateBoost(string boostName, Feat feat, IEnumerable<string> selectorBoosts)
        {
            var boost = new BoostTemplateEntry(boostName);
            boost.AddDataEntry("Passives", feat.PassivesAdded);
            boost.AddDataEntry("Boosts", selectorBoosts);
            return boost;
        }

        private IEnumerable<Module> GetPreviousModules(int moduleIndex)
        {
            return mModules.Skip(moduleIndex);
        }

        private EntryDescription GetEntryDescription(int moduleIndex, Feat feat)
        {
            var result = new EntryDescription();
            var featDescription = GetDescription(moduleIndex, feat.Id);
            if (featDescription != null)
            {
                result.Description = featDescription.Description;
                result.DisplayName = featDescription.DisplayName;
            }

            // TODO: Gather stat entries for passives to be able to get the icon id for the first passive of a feat.
            result.Icon = "PassiveFeature_Generic_Magical";
            foreach (var passive in feat.PassivesAdded)
            {
                var stat = GetStatEntry(moduleIndex, passive, "Icon");
                if (stat != null && stat.Count > 0 && !string.IsNullOrEmpty(stat[0]))
                {
                    result.Icon = stat[0];
                    break;
                }
            }

            return result;
        }

        private static IDictionary<string, EntryDescription> mAbilityDescriptions = new Dictionary<string, EntryDescription>
        {
            { "Strength", new EntryDescription("h6c83537fg6358g41e0g8a18g32cc8316ced2_1;1", "haaf3959ag320eg4f68ga9c9gc143d7f64a8c;1", "PassiveFeature_Generic_Magical") },
            { "Dexterity", new EntryDescription("h6c83537fg6358g41e0g8a18g32cc8316ced2_2;1", "hbf128ebdgdfffg4ea9gbf4bg1659ccefd287;1", "PassiveFeature_Generic_Magical") },
            { "Constitution", new EntryDescription("h6c83537fg6358g41e0g8a18g32cc8316ced2_3;1", "h7a02f64dg4593g408fgbf93gb0dbabc182c9;1", "PassiveFeature_Generic_Magical") },
            { "Intelligence", new EntryDescription("h6c83537fg6358g41e0g8a18g32cc8316ced2_4;1", "h411a732ag4b4cg4094g9a5egd325fecf4645;1", "PassiveFeature_Generic_Magical") },
            { "Wisdom", new EntryDescription("h6c83537fg6358g41e0g8a18g32cc8316ced2_5;1", "h35233e68gf68ag461cgac5fgc15806be3dc7;1", "PassiveFeature_Generic_Magical") },
            { "Charisma", new EntryDescription("h6c83537fg6358g41e0g8a18g32cc8316ced2_6;1", "h441085efge3a5g4004gba8dgf2378e8986c8;1", "PassiveFeature_Generic_Magical") },
        };
        private static EntryDescription GetEntryDescriptionAbility(string ability)
        {
            EntryDescription? result = null;
            if(!mAbilityDescriptions.TryGetValue(ability, out result) || result == null)
            {
                throw new NotImplementedException();
            }
            return result;
        }

        private FeatDescription? GetDescription(int moduleIndex, Guid featId)
        {
            foreach (var module in GetPreviousModules(moduleIndex))
            {
                if (module.Descriptions.TryGetValue(featId, out FeatDescription? description))
                {
                    return description;
                }
            }
            return null;
        }

        private StatFileEntry? GetStat(int moduleIndex, string name)
        {
            foreach(var module in GetPreviousModules(moduleIndex))
            {
                if (module.StatFiles.TryGetValue(name, out StatFileEntry? stat))
                {
                    return stat;
                }
            }
            return null;
        }

        private IReadOnlyList<string>? GetStatEntry(int moduleIndex, string name, string field)
        {
            var stat = GetStat(moduleIndex, name);
            if(stat == null)
            {
                return null;
            }    
            var current = stat[field];
            if(current != null)
            {
                return current;
            }
            if(!string.IsNullOrEmpty(stat.Using))
            {
                return GetStatEntry(moduleIndex, stat.Using, field);
            }
            return null;
        }
    }
}