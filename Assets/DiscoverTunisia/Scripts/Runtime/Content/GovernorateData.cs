using System.Collections.Generic;
using UnityEngine;

namespace DiscoverTunisia.Content
{
    /// <summary>Everything the app shows about one governorate.</summary>
    [CreateAssetMenu(menuName = "Discover Tunisia/Governorate", fileName = "Governorate")]
    public sealed class GovernorateData : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private string subtitle;
        [SerializeField, TextArea(3, 10)] private string description;
        [SerializeField, TextArea(2, 4)] private string shortBlurb;

        [Header("Globe")]
        [SerializeField, Range(-90f, 90f)] private float latitude;
        [SerializeField, Range(-180f, 180f)] private float longitude;
        [SerializeField] private Color accentColor = Color.white;

        [Header("Sections")]
        [SerializeField] private MonumentEntry[] monuments = new MonumentEntry[0];
        [SerializeField] private CultureEntry[] foods = new CultureEntry[0];
        [SerializeField] private CultureEntry[] clothes = new CultureEntry[0];

        public string Id => id;
        public string DisplayName => displayName;
        public string Subtitle => subtitle;
        public string Description => description;
        public string ShortBlurb => shortBlurb;
        public float Latitude => latitude;
        public float Longitude => longitude;
        public Color AccentColor => accentColor;
        public IReadOnlyList<MonumentEntry> Monuments => monuments;
        public IReadOnlyList<CultureEntry> Foods => foods;
        public IReadOnlyList<CultureEntry> Clothes => clothes;

#if UNITY_EDITOR
        public void EditorSetup(string id, string displayName, string subtitle, string description, string shortBlurb,
            float latitude, float longitude, Color accentColor,
            MonumentEntry[] monuments, CultureEntry[] foods, CultureEntry[] clothes)
        {
            this.id = id;
            this.displayName = displayName;
            this.subtitle = subtitle;
            this.description = description;
            this.shortBlurb = shortBlurb;
            this.latitude = latitude;
            this.longitude = longitude;
            this.accentColor = accentColor;
            this.monuments = monuments;
            this.foods = foods;
            this.clothes = clothes;
        }
#endif
    }
}
