using LSLib.LS;

namespace BG3Types
{
    /// <summary>
    /// The description information for a feat. Note: this may live in a different earlier module than the one the feat is specified in.
    /// </summary>
    public class FeatDescription
    {
        /// <summary>
        /// The Guid of the feat.
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>
        /// The description of the feat.
        /// </summary>
        public LocalizedString Description { get; private set; }
        /// <summary>
        /// The display name of the feat.
        /// </summary>
        public LocalizedString DisplayName { get; private set; }


        private FeatDescription(Guid id, LocalizedString displayName, LocalizedString description)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
        }

        /// <summary>
        /// Reads the collection of feat descriptions from the list of <paramref name="nodes"/>.
        /// </summary>
        public static IEnumerable<FeatDescription> GatherFromLsx(List<Node> nodes)
        {
            foreach (var lsxDescription in nodes)
            {
                var id = lsxDescription.GetGuidAttribute("FeatId");
                var featDisplayName = lsxDescription.GetStringAttribute("DisplayName");
                var featDescription = lsxDescription.GetStringAttribute("Description");

                yield return new FeatDescription(id, new LocalizedString(featDisplayName), new LocalizedString(featDescription));
            }
        }

        /// <summary>
        /// Reads the collection of feat descriptions from the <paramref name="packageFile"/>.
        /// </summary>
        public static IEnumerable<FeatDescription> Read(PackagedFileInfo packageFile)
        {
            using var content = packageFile.CreateContentReader();
            using var lsx = Lsx.Get(content);
            return Read(lsx.Read());
        }

        private static IEnumerable<FeatDescription> Read(Resource resource)
        {
            try
            {
                var lsxDescriptions = resource.Regions["FeatDescriptions"].Children["FeatDescription"];
                if (lsxDescriptions == null)
                {
                    return Array.Empty<FeatDescription>();
                }
                return GatherFromLsx(lsxDescriptions);
            }
            catch
            {

            }
            return Array.Empty<FeatDescription>();
        }
    }
}   