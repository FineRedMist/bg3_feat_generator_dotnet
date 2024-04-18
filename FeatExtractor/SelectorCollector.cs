using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BG3Types;

namespace FeatExtractor
{
    internal class SelectorCollector
    {
        public IDictionary<string, int> Attributes { get; private set; } = new Dictionary<string, int>();
        public HashSet<string> Skills { get; private set; } = new HashSet<string>();
        public HashSet<string> Spells { get; private set; } = new HashSet<string>();
        public HashSet<string> Passives { get; private set; } = new HashSet<string>();

        public IReadOnlyList<ISelector> Selectors { get; private set; }

        public bool IsComplete
        {
            get
            {
                return Selectors.Count == 0;
            }
        }

        /// <summary>
        /// Tracks the current selector being processed.
        /// </summary>
        private ISelector? CurrentSelector { get; set; } = null;
        /// <summary>
        /// If the selector allows multiple things to be selected, there may be mutual exclusive properties between them. Track choices so far.
        /// </summary>
        private IList<string> mCurrentSelectorChoices = new List<string>();

        public SelectorCollector(IReadOnlyList<ISelector> selectors)
        { 
            Selectors = selectors;
            CurrentSelector = selectors.FirstOrDefault();
        }

        private SelectorCollector Clone(bool removeFirstSelector)
        {
            var result = new SelectorCollector(removeFirstSelector ? Selectors.Skip(1).ToList() : Selectors);
            result.Attributes = new Dictionary<string, int>(Attributes);
            result.Skills = new HashSet<string>(Skills);
            result.Spells = new HashSet<string>(Spells);
            result.Passives = new HashSet<string>(Passives);

            // Still working on the current collector
            if(!removeFirstSelector)
            {
                result.mCurrentSelectorChoices = new List<string>(mCurrentSelectorChoices);
            }
            return result;
        }

        public IEnumerable<(string, SelectorCollector)> GatherCases(IEnumerable<string> cases)
        { 
            if(CurrentSelector == null)
            {
                yield break;
            }

            // Any previous choices are in mCurrentSelectorChoices if we need to exclude them.
            switch (CurrentSelector.Type)
            {
                case SelectorType.Ability:
                    SelectorAbility ability = (SelectorAbility)CurrentSelector;
                    bool isDone = ability.Count == mCurrentSelectorChoices.Count + 1; // +1 for the choice we are generating now.
                    foreach(var cas in cases)
                    {
                        if (Attributes.FindOrDefault(cas) >= ability.Max)
                        {
                            continue;
                        }
                        // Generate a case.
                        var nextCollector = Clone(isDone);
                        nextCollector.mCurrentSelectorChoices.Add(cas);
                        nextCollector.IncrementAbility(cas);

                        yield return (cas, nextCollector);
                    }
                    yield break;

                default:
                    throw new NotImplementedException();
            };
        }

        private void IncrementAbility(string ability)
        {
            int current = Attributes.FindOrAdd(ability);
            Attributes[ability] = current + 1;
        }

        public IEnumerable<string> GatherBoosts()
        {
            foreach(var attribute in Attributes)
            {
                yield return $"Ability({attribute.Key},{attribute.Value})";
            }
        }
        public IEnumerable<string> GatherRequirements()
        {
            foreach (var attribute in Attributes)
            {
                int maxAbility = 20 - attribute.Value;
                yield return $"not AbilityGreaterThan('{attribute.Key}',{maxAbility})";
            }
        }
    }
}
