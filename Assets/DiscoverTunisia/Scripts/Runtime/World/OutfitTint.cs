using UnityEngine;

namespace DiscoverTunisia.World
{
    /// <summary>Colors the avatar's outfit with a governorate accent color, without new materials.</summary>
    [RequireComponent(typeof(Renderer))]
    public sealed class OutfitTint : MonoBehaviour
    {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        [SerializeField] private int outfitMaterialIndex = -1;
        [SerializeField] private int outfitDarkMaterialIndex = -1;
        [SerializeField, Range(0f, 1f)] private float darkShade = 0.35f;

        public void Apply(Color accent)
        {
            var target = GetComponent<Renderer>();
            var block = new MaterialPropertyBlock();
            if (outfitMaterialIndex >= 0)
            {
                block.SetColor(BaseColor, accent);
                target.SetPropertyBlock(block, outfitMaterialIndex);
            }
            if (outfitDarkMaterialIndex >= 0)
            {
                block.SetColor(BaseColor, Color.Lerp(accent, Color.black, darkShade));
                target.SetPropertyBlock(block, outfitDarkMaterialIndex);
            }
        }

#if UNITY_EDITOR
        public void EditorSetup(int outfitIndex, int outfitDarkIndex)
        {
            outfitMaterialIndex = outfitIndex;
            outfitDarkMaterialIndex = outfitDarkIndex;
        }
#endif
    }
}
