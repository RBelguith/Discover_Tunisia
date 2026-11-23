using UnityEngine;

namespace DiscoverTunisia.Core
{
    /// <summary>État global partagé entre les scènes (aucune donnée sensible conservée).</summary>
    public static class AppState
    {
        /// <summary>Gouvernorat sélectionné ou détecté (id de ContentDatabase).</summary>
        public static string SelectedGovernorateId = null;

        /// <summary>Comment le gouvernorat a été choisi : "scan" ou "manuel".</summary>
        public static string SelectionSource = "manuel";

        public static GovernorateInfo Selected
        {
            get
            {
                if (string.IsNullOrEmpty(SelectedGovernorateId)) return null;
                return ContentDatabase.GetById(SelectedGovernorateId);
            }
        }

        public static void Select(string governorateId, string source)
        {
            SelectedGovernorateId = governorateId;
            SelectionSource = source;
        }

        public static void Reset()
        {
            SelectedGovernorateId = null;
            SelectionSource = "manuel";
        }
    }

    public static class SceneNames
    {
        public const string Accueil = "01_Accueil";
        public const string Identification = "02_Identification";
        public const string Globe = "03_Globe";
        public const string AR = "04_AR";
    }
}
