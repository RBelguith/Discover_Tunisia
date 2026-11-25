using System.Collections.Generic;
using UnityEngine;

namespace DiscoverTunisia.Content
{
    /// <summary>The governorates the app offers, in display order.</summary>
    [CreateAssetMenu(menuName = "Discover Tunisia/Content Catalog", fileName = "ContentCatalog")]
    public sealed class ContentCatalog : ScriptableObject
    {
        [SerializeField] private GovernorateData[] governorates = new GovernorateData[0];

        public IReadOnlyList<GovernorateData> Governorates => governorates;

        /// <summary>First governorate, used when a scene is opened directly in the Editor.</summary>
        public GovernorateData Default => governorates.Length > 0 ? governorates[0] : null;

        public GovernorateData Find(string id)
        {
            foreach (var g in governorates)
                if (g != null && g.Id == id) return g;
            return null;
        }

#if UNITY_EDITOR
        public void EditorSetup(GovernorateData[] items) => governorates = items;
#endif
    }
}
