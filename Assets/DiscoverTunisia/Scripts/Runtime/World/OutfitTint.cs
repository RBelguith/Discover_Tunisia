using UnityEngine;

namespace DiscoverTunisia.World
{
    /// <summary>Colors the avatar's outfit with a governorate accent color, without new materials.</summary>
    public sealed class OutfitTint : MonoBehaviour
    {
        private const string OutfitMaterial = "M_Outfit";
        private const string OutfitDarkMaterial = "M_OutfitDark";
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        [SerializeField, Range(0f, 1f)] private float darkShade = 0.35f;

        public void Apply(Color accent)
        {
            var dark = Color.Lerp(accent, Color.black, darkShade);
            var block = new MaterialPropertyBlock();
            foreach (var target in GetComponentsInChildren<Renderer>(true))
            {
                var materials = target.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] == null) continue;
                    if (materials[i].name == OutfitMaterial) Tint(target, i, accent, block);
                    else if (materials[i].name == OutfitDarkMaterial) Tint(target, i, dark, block);
                }
            }
        }

        private static void Tint(Renderer target, int slot, Color color, MaterialPropertyBlock block)
        {
            target.GetPropertyBlock(block, slot);
            block.SetColor(BaseColor, color);
            target.SetPropertyBlock(block, slot);
        }
    }
}
