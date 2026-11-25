using System;
using UnityEngine;

namespace DiscoverTunisia.Content
{
    /// <summary>A monument: text, photo, and the 3D model shown on the stage and in AR.</summary>
    [Serializable]
    public sealed class MonumentEntry
    {
        [SerializeField] private string title;
        [SerializeField, TextArea(3, 8)] private string description;
        [SerializeField] private Sprite image;
        [SerializeField] private GameObject model;

        public string Title => title;
        public string Description => description;
        public Sprite Image => image;
        public GameObject Model => model;

        public MonumentEntry(string title, string description, Sprite image, GameObject model)
        {
            this.title = title;
            this.description = description;
            this.image = image;
            this.model = model;
        }
    }
}
