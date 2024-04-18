using LSLib.LS;

namespace BG3Types
{
    /// <summary>
    /// Represents information about a feat from Feat.lsx.
    /// </summary>
    public class Feat
    {
        /// <summary>
        /// The ID of the feat.
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>
        /// The name.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Whether the feat can be taken multiple times, typically false.
        /// </summary>
        public bool CanBeTakenMultipleTimes { get; private set; } = false;
        /// <summary>
        /// The list of passives for the feat.
        /// </summary>
        public IReadOnlyList<string> PassivesAdded { get; private set; } = Array.Empty<string>();
        /// <summary>
        /// Helper to query whether passives are present.
        /// </summary>
        public bool HasPassives { get { return PassivesAdded.Count > 0; } }
        /// <summary>
        /// The set of requirements for the feat to be selectable.
        /// </summary>
        public string? Requirements { get; private set; }
        /// <summary>
        /// Selectors are 'functions' that reference entries in various lists for passives, skills, abilities, and spells.
        /// They take extra work to process as each selection needs to be mapped out.
        /// SelectPassives(f6b6e71f-79b1-4ba3-8fd8-ee38a44d3d39,1)
        /// SelectPassives(e51a2ef5-3663-43f9-8e74-5e28520323f1,2,MAManeuvers)
        /// SelectAbilities(499230af-5946-4680-a7ee-4d76d421f2ef,1,1,LightlyArmoredASI)
        /// SelectAbilities(98acdfcb-3c74-4e1a-8707-5d6da747d430,1,1,PerformerASI)
        /// SelectPassives(f21e6b94-44e8-4ae0-a6f1-0c81abac03a2,4,WeaponMasterProficiencies)
        /// SelectSpells(8c32c900-a8ea-4f2f-9f6f-eccd0d361a9d,2,0,,,,AlwaysPrepared)
        /// SelectSkills(f974ebd6-3725-4b90-bb5c-2b647d41615d,3,SkilledSkills)
        /// SelectAbilities(b9149c8e-52c8-46e5-9cb6-fc39301c05fe,2,2,FeatASI)
        /// SelectAbilities(49cbc5da-1ab0-4694-80cf-6434ee3c18a9,1,1,ShadowTouchedIntelligence);SelectSpells(204d78ae-1374-455f-91ee-f23dcbfb93d7,1,0,,Intelligence,None,AlwaysPrepared,UntilRest)
        /// SelectAbilities(372c1167-b9ed-46ef-be41-f345d45a7e13,1,1,SkillExpert);SelectSkills(f974ebd6-3725-4b90-bb5c-2b647d41615d,1);SelectSkillsExpertise(ed664663-93b9-4070-a54b-3c7b19c0e7b4,1)
        /// SelectSpells(61f79a30-2cac-4a7a-b5fe-50c89d307dd6,2,0,MIBardCantrips,Charisma,,AlwaysPrepared,,92cd50b6-eb1b-4824-8adb-853e90c34c90);SelectSpells(dcb45167-86bd-4297-9b9d-c295be51af5b,1,0,MIBardSpells,Charisma,None,AlwaysPrepared,UntilRest,92cd50b6-eb1b-4824-8adb-853e90c34c90)
        /// 
        /// SelectPassives(/guid/,/count/, [/filter?/])
        ///     guid found in Public/-mod-/Lists/PassiveLists.lsx
        /// SelectAbilities(/guid/,/count?/,/points?/,/build in category? can't find in assets/) LightlyArmoredASI, PerformerASI, FeatASI, /name of feat/
        ///     guid found in Public/-mod-/Lists/AbilityLists.lsx
        /// SelectSkills(/guid/,/count/)
        ///     guid found in Public/-mod-/Lists/SkillLists.lsx
        /// SelectSkillsExpertise(/guid/,/count/)
        ///     guid found in Public/-mod-/Lists/SkillLists.lsx
        /// SelectSpells(/guid/,/count/,0,/MIBardCantrips-MIBardSpells not sure where this value comes from/,/Cha/Wis/Int/,/None or blank??/,/AlwaysPrepared?/,/Recharge--blank always or UntilRest/,/Class guid/)
        ///     guid found in Public/-mod-/Lists/SpellLists.lsx
        ///     class guid found in Public/-mod-/ClassDescriptions/ClassDescriptions.lsx (UUID)
        /// </summary>
        public IReadOnlyList<ISelector> Selectors { get; set; } = Array.Empty<ISelector>();

        /// <summary>
        /// Creates a feat.
        /// </summary>
        /// <param name="id">The ID</param>
        /// <param name="name">The name</param>
        /// <param name="canBeTakenMultipleTimes">Whether the feat can be taken multiple times.</param>
        /// <param name="passivesAdded">If set, semicolon delimited list of passves to add.</param>
        /// <param name="requirements">If set, semicolon delimited list of requirements.</param>
        /// <param name="selectors">If set, semicolon delimited list of selectors to process.</param>
        private Feat(Guid id, string name, bool canBeTakenMultipleTimes, string? passivesAdded, string? requirements, string? selectors)
        {
            Id = id;
            Name = name;
            CanBeTakenMultipleTimes = canBeTakenMultipleTimes;
            PassivesAdded = Lsx.ParseList(passivesAdded);
            Requirements = requirements;
            Selectors = Lsx.ParseList(selectors)
                .Select(ISelector.Parse)
                .ToArray();
        }

        /// <summary>
        /// Gets the name to use for the spell based on the feat name.
        /// </summary>
        /// <param name="normalizedModuleName">The normalized module name that avoids characters that could be problematic.</param>
        /// <returns>A string to use for the spell name.</returns>
        public string GetSpellName(string normalizedModuleName)
        {
            return $"E6_Shout_{normalizedModuleName}_{Name}";
        }

        /// <summary>
        /// Parses a feat node from the feat.lsx file.
        /// </summary>
        /// <param name="reader">The reader for the Lsx file.</param>
        /// <returns>A collection of feats (may be empty)</returns>
        private static IEnumerable<Feat> GatherFromLsx(List<Node> reader){
            foreach (var lsxFeat in reader)
            {
                var id = lsxFeat.GetGuidAttribute("UUID");
                var name = lsxFeat.GetStringAttribute("Name");
                var canBeTakenMultipleTimes = lsxFeat.GetBoolAttribute("CanBeTakenMultipleTimes");
                var selectors = lsxFeat.GetStringAttribute("Selectors", null);
                var passivesAdded = lsxFeat.GetStringAttribute("PassivesAdded", null);
                var requirements = lsxFeat.GetStringAttribute("Requirements", null);

                yield return new Feat(id, name, canBeTakenMultipleTimes, passivesAdded, requirements, selectors);
            }
        }

        /// <summary>
        /// Reads a collection of feats from a Feats.lsx file.
        /// </summary>
        /// <param name="packageFile">The package file representing a feats.lsx file.</param>
        /// <returns>A collection of feats (may be empty)</returns>
        public static IEnumerable<Feat> Read(PackagedFileInfo packageFile)
        {
            using var content = packageFile.CreateContentReader();
            using var lsx = Lsx.Get(content);
            return Read(lsx.Read());
        }

        /// <summary>
        /// Reads a collection of feats from a Feats.lsx file.
        /// </summary>
        /// <param name="resource">The resource representing the feat.lsx file.</param>
        /// <returns>A collection of feats (may be empty)</returns>
        private static IEnumerable<Feat> Read(Resource resource)
        {
            try
            {
                var lsxFeats = resource.Regions["Feats"].Children["Feat"];
                if (lsxFeats == null)
                {
                    return Array.Empty<Feat>();
                }
                return GatherFromLsx(lsxFeats);
            }
            catch
            {
            }
            return Array.Empty<Feat>();
        }
    }
}