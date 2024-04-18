using BG3Types;

namespace FeatExtractor
{
    internal class BoostStatEntry : StatFileEntry
    {
        public BoostStatEntry(string name)
            : base(name, "StatusData")
        {
            AddDataEntry("StatusType", "BOOST");
        }

        public BoostStatEntry(string name, string usingName)
            : base(name, "StatusData", usingName)
        {
            AddDataEntry("StatusType", "BOOST");
        }
    }

    internal class SpellStatEntry : StatFileEntry
    {
        public SpellStatEntry(string name)
            : base(name, "SpellData")
        {
        }
        public SpellStatEntry(string name, string usingName)
            : base(name, "SpellData", usingName)
        {
        }
    }

    internal class SpellTemplateEntry : SpellStatEntry
    {
        public SpellTemplateEntry(string name)
            : base(name)
        {
            AddDataEntry("SpellType", "Shout");
            AddDataEntry("Level", "");
            AddDataEntry("SpellSchool", "");
            AddDataEntry("AIFlags", "CanNotUse");
            AddDataEntry("TargetConditions", "Self()");
            AddDataEntry("CastTextEvent", "Cast");
            AddDataEntry("SpellAnimation", "b3b2d16b-61c7-4082-8394-0c04fb9ffdec,,;81c58c55-625d-46c3-bbb7-179b23ef725e,,;3c35a4e1-4441-4603-9c71-82179057d452,,;18c8ab7a-cfef-45b9-851d-e2bc52c9ebc3,,;e601e8fd-4017-4d26-a63a-e1d7362c99b3,,;,,;0b07883a-08b8-43b6-ac18-84dc9e84ff50,,;,,;,,");
            AddDataEntry("SpellFlags", "IgnoreSilence");
            AddDataEntry("DamageType", "None");
            AddDataEntry("PrepareEffect", "c520a0bf-adc6-44f6-abcd-94bc0925b881");
            AddDataEntry("VerbalIntent", "Utility");
            AddDataEntry("UseCosts", "FeatPoint:1");
            AddDataEntry("Requirements", "!Combat");
        }
    }

    internal class SpellContainerTemplateEntry : SpellStatEntry
    {
        public SpellContainerTemplateEntry(string name)
            : base(name)
        {
            AddDataEntry("SpellType", "Shout");
            AddDataEntry("Level", "");
            AddDataEntry("SpellSchool", "");
            AddDataEntry("ContainerSpells", "");
            AddDataEntry("AIFlags", "CanNotUse");
            AddDataEntry("TargetConditions", "Self()");
            AddDataEntry("CastTextEvent", "Cast");
            AddDataEntry("UseCosts", "FeatPoint:1");
            AddDataEntry("Requirements", "!Combat");
            AddDataEntry("SpellFlags", "IsLinkedSpellContainer");
        }
    }

    internal class BoostTemplateEntry : BoostStatEntry
    {
        public BoostTemplateEntry(string name)
            : base(name)
        {
            AddDataEntry("StatusPropertyFlags", "IgnoreResting", "DisableCombatlog", "ApplyToDead", "DisableOverhead", "ExcludeFromPortraitRendering", "DisablePortraitIndicator");
            AddDataEntry("StatusGroups", "SG_RemoveOnRespec");
            AddDataEntry("HideOverheadUI", "1");
            AddDataEntry("IsUnique", "1");
            AddDataEntry("Boosts", "ActionResource(UsedFeatPoints,1,0)");
        }
    }
}