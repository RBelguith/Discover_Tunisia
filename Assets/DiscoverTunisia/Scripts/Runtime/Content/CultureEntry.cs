using System;
using UnityEngine;

namespace DiscoverTunisia.Content
{
    /// <summary>A dish or a garment shown in the Gastronomie and Vêtements sections.</summary>
    [Serializable]
    public sealed class CultureEntry
    {
        [SerializeField] private string title;
        [SerializeField, TextArea(3, 8)] private string description;
        [SerializeField] private Sprite image;

        public string Title => title;
        public string Description => description;
        public Sprite Image => image;

        public CultureEntry(string title, string description, Sprite image)
        {
            this.title = title;
            this.description = description;
            this.image = image;
        }
    }
}
