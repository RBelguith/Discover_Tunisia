using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace DiscoverTunisia.EditorTools.Placeholders
{
    public enum PlaceholderStyle { Generic, MinaretDome, SculptedFacade, MausoléeDome, RuinsColumns, MedinaSouk, FortPort }

    /// <summary>Material assets for the placeholder models: one asset per color, created on first use.</summary>
    public static class Palette
    {
        public const string MaterialDir = "Assets/DiscoverTunisia/Art/Materials";
        public const string LitShader = "Universal Render Pipeline/Lit";
        public const string UnlitShader = "Universal Render Pipeline/Unlit";

        /// <summary>Stand-in color the avatar builder uses for the outfit, tinted per governorate at runtime.</summary>
        public static readonly Color Outfit = new Color(1f, 0f, 1f);
        public static readonly Color OutfitDark = Color.Lerp(Outfit, Color.black, 0.35f);

        private static readonly Dictionary<Color32, string> Names = new Dictionary<Color32, string>
        {
            { new Color(0.86f, 0.80f, 0.66f), "Stone" },
            { new Color(0.66f, 0.58f, 0.45f), "StoneDark" },
            { new Color(0.78f, 0.55f, 0.38f), "Brick" },
            { new Color(0.20f, 0.55f, 0.80f), "Tile" },
            { new Color(0.92f, 0.92f, 0.90f), "Whitewash" },
            { new Color(1f, 0.8f, 0.3f), "Gold" },
            { new Color(1.0f, 0.82f, 0.35f), "Jewel" },
            { new Color(0.18f, 0.13f, 0.09f), "Doorway" },
            { new Color(0.16f, 0.12f, 0.09f), "DoorwayDark" },
            { new Color(0.25f, 0.16f, 0.10f), "Wood" },
            { new Color(0.75f, 0.68f, 0.55f), "Sand" },
            { new Color(0.10f, 0.35f, 0.55f), "Water" },
            { new Color(0.85f, 0.25f, 0.2f), "Beacon" },
            { new Color(0.85f, 0.85f, 0.8f), "Hull" },
            { new Color(0.5f, 0.4f, 0.3f), "Mast" },
            { Color.white, "Sail" },
            { new Color(0.94f, 0.78f, 0.62f), "Skin" },
            { new Color(0.16f, 0.10f, 0.07f), "Hair" },
            { new Color(0.72f, 0.12f, 0.15f), "Chechia" },
            { new Color(0.10f, 0.18f, 0.30f), "Plinth" },
            { new Color(0.10f, 0.20f, 0.34f), "Stage" },
            { Outfit, "Outfit" },
            { OutfitDark, "OutfitDark" },
        };

        private static readonly Dictionary<Color32, Material> Cache = new Dictionary<Color32, Material>();

        public static Material Lit(Color color)
        {
            var key = (Color32)color;
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            string name = Names.TryGetValue(key, out var n) ? n : "Color_" + ColorUtility.ToHtmlStringRGB(color);
            Color baseColor = name == "Outfit" ? new Color(1f, 0.55f, 0.2f)
                : name == "OutfitDark" ? Color.Lerp(new Color(1f, 0.55f, 0.2f), Color.black, 0.35f)
                : color;
            var mat = Asset("M_" + name, LitShader, m =>
            {
                m.SetColor("_BaseColor", baseColor);
                m.SetFloat("_Smoothness", 0.15f);
            });
            Cache[key] = mat;
            return mat;
        }

        /// <summary>Creates or updates a material asset, keeping its GUID when it already exists.</summary>
        public static Material Asset(string name, string shaderName, Action<Material> setup)
        {
            string path = MaterialDir + "/" + name + ".mat";
            var shader = Shader.Find(shaderName) ?? throw new InvalidOperationException("Shader not found: " + shaderName);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.shader = shader;
            setup(mat);
            BaseShaderGUI.SetMaterialKeywords(mat);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        /// <summary>Alpha-blended URP surface, set through the same keyword setup the material inspector uses.</summary>
        public static void Transparent(Material m, Color color, Texture texture = null, CullMode cull = CullMode.Back)
        {
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetFloat("_Cull", (float)cull);
            m.SetColor("_BaseColor", color);
            if (texture != null) m.SetTexture("_BaseMap", texture);
        }

        public static void StripColliders(GameObject go)
        {
            foreach (var c in go.GetComponentsInChildren<Collider>(true))
                Object.DestroyImmediate(c);
        }

        public static void ClearCache() => Cache.Clear();
    }
}
