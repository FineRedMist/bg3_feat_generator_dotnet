using System.Collections.Generic;

namespace FeatExtractor
{
    /// <summary>
    /// The root object that stores information about where to find the game (I'd like to move this to some other logic to be more intelligent)
    /// and the collection of Nexus mods to query for extracting feats.
    /// </summary>
    internal class Config
    {
        /// <summary>
        /// Install paths to search for the game.
        /// </summary>
        public IList<string> GameInstallPaths { get; set; } = new List<string>();
        /// <summary>
        /// A list of Nexus mods to query for feats.
        /// </summary>
        public Nexus Nexus { get; set; } = new Nexus();
    }

    internal class Nexus
    {
        /// <summary>
        /// The API Key to access Nexus. Don't put it in the config, use the commandline. It just gets assigned here for propagation.
        /// </summary>
        public string? ApiKey { get; set; }
        /// <summary>
        /// Format string to generate an url to the given mod on Nexus.
        /// </summary>
        public string? UrlFormat { get; set; }
        /// <summary>
        /// A mapping of the name of mod (user determined), and the mod id on Nexus to query.
        /// Both Main and Optional files are queried 
        /// TODO: make this configurable?
        /// </summary>
        public IDictionary<string, int> GameMods { get; set; } = new SortedDictionary<string, int>();
    }
}