using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiscoverTunisia.EditorTools.Placeholders;
using DiscoverTunisia.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DiscoverTunisia.EditorTools
{
    /// <summary>
    /// Bakes the primitive-built placeholder models into prefabs with mesh and material assets.
    /// Swap a prefab reference in a GovernorateData asset to replace a placeholder with a real model.
    /// </summary>
    public static class PlaceholderModelBaker
    {
        private const string Art = "Assets/DiscoverTunisia/Art";
        private const string Prefabs = Art + "/Prefabs";
        private const string Meshes = Art + "/Meshes";
        private const string Textures = Art + "/Textures";
        private const float GlobeRadius = 3f;

        public static readonly Dictionary<PlaceholderStyle, string> MonumentNames = new Dictionary<PlaceholderStyle, string>
        {
            { PlaceholderStyle.MinaretDome, "Monument_GreatMosque" },
            { PlaceholderStyle.SculptedFacade, "Monument_SculptedFacade" },
            { PlaceholderStyle.MausoléeDome, "Monument_Mausoleum" },
            { PlaceholderStyle.RuinsColumns, "Monument_Ruins" },
            { PlaceholderStyle.MedinaSouk, "Monument_Medina" },
            { PlaceholderStyle.FortPort, "Monument_OldPort" },
        };

        [MenuItem("Discover Tunisia/Rebuild Placeholder Models")]
        public static void RebuildAll()
        {
            EnsureFolders();
            Palette.ClearCache();
            foreach (var style in MonumentNames.Keys) BakeMonument(style);
            BakeAvatar();
            BakePlinth();
            BakeMarker();
            BakeGlobe();
            BakeBackdrop();
            AssetDatabase.SaveAssets();
        }

        public static string MonumentPath(PlaceholderStyle style) => Prefabs + "/" + MonumentNames[style] + ".prefab";

        private static void EnsureFolders()
        {
            foreach (var dir in new[] { Art, Prefabs, Meshes, Textures, Palette.MaterialDir })
            {
                if (AssetDatabase.IsValidFolder(dir)) continue;
                AssetDatabase.CreateFolder(Path.GetDirectoryName(dir).Replace('\\', '/'), Path.GetFileName(dir));
            }
        }

        // Models

        private static void BakeMonument(PlaceholderStyle style)
        {
            var go = PlaceholderMonuments.Build(style, null, null);
            go.name = MonumentNames[style];
            Flatten(go);
            Save(go, MonumentPath(style));
        }

        private static void BakeAvatar()
        {
            var go = PlaceholderAvatar.Create(Palette.Outfit, null);
            var materials = Flatten(go);
            var tint = go.AddComponent<OutfitTint>();
            tint.EditorSetup(IndexOf(materials, "M_Outfit"), IndexOf(materials, "M_OutfitDark"));
            Save(go, Prefabs + "/Avatar.prefab");
        }

        /// <summary>Round display stand the monuments and the avatar sit on.</summary>
        private static void BakePlinth()
        {
            var root = new GameObject("Plinth");
            var disc = Primitive(PrimitiveType.Cylinder, "Disc", root.transform, Vector3.zero, new Vector3(2.4f, 0.05f, 2.4f));
            disc.GetComponent<MeshRenderer>().sharedMaterial = Palette.Lit(new Color(0.10f, 0.20f, 0.34f));
            var halo = Primitive(PrimitiveType.Cylinder, "Halo", root.transform, new Vector3(0f, -0.06f, 0f), new Vector3(3.4f, 0.02f, 3.4f));
            halo.GetComponent<MeshRenderer>().sharedMaterial = Palette.Asset("M_PlinthHalo", Palette.UnlitShader,
                m => Palette.Transparent(m, new Color(0.31f, 0.76f, 0.97f, 0.35f)));
            Save(root, Prefabs + "/Plinth.prefab");
        }

        private static void BakeMarker()
        {
            var glowTex = SaveTexture("MarkerGlow", 256, RadialGlow);
            var dotTex = SaveTexture("MarkerDot", 128, SoftDisc);

            var root = new GameObject("Marker");
            var glow = Primitive(PrimitiveType.Quad, "Glow", root.transform, new Vector3(0f, 0f, 0.02f), Vector3.one, Quaternion.Euler(0f, 180f, 0f));
            glow.GetComponent<MeshRenderer>().sharedMaterial = Palette.Asset("M_MarkerGlow", Palette.UnlitShader, m => Palette.Transparent(m, Color.white, glowTex));
            var dot = Primitive(PrimitiveType.Quad, "Dot", root.transform, new Vector3(0f, 0f, 0.045f), new Vector3(0.34f, 0.34f, 1f), Quaternion.Euler(0f, 180f, 0f));
            dot.GetComponent<MeshRenderer>().sharedMaterial = Palette.Asset("M_MarkerDot", Palette.UnlitShader, m => Palette.Transparent(m, Color.white, dotTex));

            var pin = new GameObject("Pin");
            pin.transform.SetParent(root.transform, false);
            var stem = Primitive(PrimitiveType.Cylinder, "Stem", pin.transform, new Vector3(0f, 0f, 0.30f), new Vector3(0.045f, 0.30f, 0.045f), Quaternion.Euler(90f, 0f, 0f));
            stem.GetComponent<MeshRenderer>().sharedMaterial = Palette.Asset("M_PinStem", Palette.UnlitShader, m => m.SetColor("_BaseColor", new Color(1f, 1f, 1f, 0.95f)));
            var head = Primitive(PrimitiveType.Sphere, "Head", pin.transform, new Vector3(0f, 0f, 0.66f), Vector3.one * 0.30f);
            head.GetComponent<MeshRenderer>().sharedMaterial = Palette.Asset("M_PinHead", Palette.UnlitShader, m => m.SetColor("_BaseColor", Color.white));

            foreach (var r in root.GetComponentsInChildren<MeshRenderer>()) NoShadows(r);
            root.AddComponent<MarkerView>().EditorSetup(glow.GetComponent<Renderer>(), dot.GetComponent<Renderer>(), head.GetComponent<Renderer>(), pin);
            Save(root, Prefabs + "/Marker.prefab");
        }

        private static void BakeGlobe()
        {
            var root = new GameObject("Globe");
            var spin = new GameObject("Spin");
            spin.transform.SetParent(root.transform, false);

            var earthTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Images/earth.jpg");
            var earth = new GameObject("Earth");
            earth.transform.SetParent(spin.transform, false);
            earth.AddComponent<MeshFilter>().sharedMesh = SaveMesh(Globe.SphereMesh.Build(GlobeRadius), "EarthSphere");
            earth.AddComponent<MeshRenderer>().sharedMaterial = Palette.Asset("M_Earth", Palette.LitShader, m =>
            {
                m.SetColor("_BaseColor", Color.white);
                m.SetTexture("_BaseMap", earthTex);
                m.SetFloat("_Smoothness", 0.12f);
            });

            var atmosphere = new GameObject("Atmosphere");
            atmosphere.transform.SetParent(root.transform, false);
            atmosphere.AddComponent<MeshFilter>().sharedMesh = SaveMesh(Globe.SphereMesh.Build(GlobeRadius * 1.045f, 64, 32), "AtmosphereSphere");
            var atmoRenderer = atmosphere.AddComponent<MeshRenderer>();
            atmoRenderer.sharedMaterial = Palette.Asset("M_Atmosphere", Palette.UnlitShader,
                m => Palette.Transparent(m, new Color(0.30f, 0.70f, 1.00f, 0.20f), null, CullMode.Front));
            NoShadows(atmoRenderer);

            Ring(root.transform, "Ring1", 1.55f, new Color(0.45f, 0.85f, 1f, 0.55f), new Vector3(68f, 0f, 12f));
            Ring(root.transform, "Ring2", 1.78f, new Color(0.30f, 0.65f, 1f, 0.35f), new Vector3(74f, 0f, -18f));
            Save(root, Prefabs + "/Globe.prefab");
        }

        private static void Ring(Transform parent, string name, float radiusFactor, Color color, Vector3 euler)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localEulerAngles = euler;
            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.widthMultiplier = 0.035f;
            line.numCapVertices = 4;
            line.shadowCastingMode = ShadowCastingMode.Off;
            const int count = 128;
            line.positionCount = count;
            float radius = GlobeRadius * radiusFactor;
            for (int i = 0; i < count; i++)
            {
                float a = i / (float)count * Mathf.PI * 2f;
                line.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius));
            }
            line.sharedMaterial = Palette.Asset("M_" + name, Palette.UnlitShader, m => Palette.Transparent(m, color));
        }

        /// <summary>Starfield quad placed behind the 3D scenes.</summary>
        private static void BakeBackdrop()
        {
            var tex = SaveTexture("Starfield", 1024, Starfield, 512);
            var root = Primitive(PrimitiveType.Quad, "Backdrop", null, Vector3.zero, new Vector3(64f, 32f, 1f));
            var r = root.GetComponent<MeshRenderer>();
            r.sharedMaterial = Palette.Asset("M_Starfield", Palette.UnlitShader, m =>
            {
                m.SetColor("_BaseColor", Color.white);
                m.SetTexture("_BaseMap", tex);
            });
            NoShadows(r);
            Save(root, Prefabs + "/Backdrop.prefab");
        }

        // Mesh helpers

        /// <summary>Merges every child mesh into one mesh on the root, one submesh per material.</summary>
        private static Material[] Flatten(GameObject root)
        {
            root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            var renderers = root.GetComponentsInChildren<MeshRenderer>();
            var groups = renderers.GroupBy(r => r.sharedMaterial).ToList();
            var parts = new List<CombineInstance>();
            foreach (var group in groups)
            {
                var instances = group.Select(r => new CombineInstance
                {
                    mesh = r.GetComponent<MeshFilter>().sharedMesh,
                    transform = r.transform.localToWorldMatrix,
                }).ToArray();
                var merged = new Mesh { indexFormat = IndexFormat.UInt32 };
                merged.CombineMeshes(instances, true, true);
                parts.Add(new CombineInstance { mesh = merged, transform = Matrix4x4.identity });
            }

            var mesh = new Mesh { name = root.name, indexFormat = IndexFormat.UInt32 };
            mesh.CombineMeshes(parts.ToArray(), false, false);
            mesh.RecalculateBounds();
            mesh.Optimize();

            for (int i = root.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(root.transform.GetChild(i).gameObject);
            root.transform.localScale = Vector3.one;

            var materials = groups.Select(g => g.Key).ToArray();
            root.AddComponent<MeshFilter>().sharedMesh = SaveMesh(mesh, root.name);
            root.AddComponent<MeshRenderer>().sharedMaterials = materials;
            return materials;
        }

        private static int IndexOf(Material[] materials, string name) => System.Array.FindIndex(materials, m => m.name == name);

        private static Mesh SaveMesh(Mesh mesh, string name)
        {
            string path = Meshes + "/" + name + ".asset";
            mesh.name = name;
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return mesh;
            }
            EditorUtility.CopySerialized(mesh, existing);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static GameObject Primitive(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, Quaternion? rot = null)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = rot ?? Quaternion.identity;
            go.transform.localScale = scale;
            Palette.StripColliders(go);
            return go;
        }

        private static void NoShadows(Renderer r)
        {
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
        }

        private static void Save(GameObject go, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        // Generated textures

        private delegate Color Pixel(float u, float v, System.Random rnd);

        private static Texture2D SaveTexture(string name, int width, Pixel pixel, int height = 0)
        {
            if (height == 0) height = width;
            string path = Textures + "/" + name + ".png";
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var rnd = new System.Random(12345);
            var px = new Color[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    px[y * width + x] = pixel((x + 0.5f) / width, (y + 0.5f) / height, rnd);
            tex.SetPixels(px);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.alphaIsTransparency = true;
            imp.wrapMode = TextureWrapMode.Clamp;
            imp.mipmapEnabled = false;
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Color RadialGlow(float u, float v, System.Random _)
        {
            float d = Mathf.Clamp01(Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f)) * 2f);
            return new Color(1f, 1f, 1f, Mathf.Pow(1f - d, 2.2f));
        }

        private static Color SoftDisc(float u, float v, System.Random _)
        {
            float d = Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f)) * 2f;
            return new Color(1f, 1f, 1f, Mathf.Clamp01((1f - d) * 24f));
        }

        private static Color Starfield(float u, float v, System.Random rnd)
        {
            var top = new Color(0.031f, 0.075f, 0.176f);
            var bottom = new Color(0.012f, 0.031f, 0.086f);
            var c = Color.Lerp(bottom, top, v);
            if (rnd.NextDouble() < 0.0016)
            {
                float a = 0.35f + (float)rnd.NextDouble() * 0.65f;
                c = Color.Lerp(c, Color.white, a);
            }
            c.a = 1f;
            return c;
        }
    }
}
