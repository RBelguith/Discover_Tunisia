using System.Linq;
using DiscoverTunisia.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DiscoverTunisia.Tests
{
    public class ModelPrefabTests
    {
        private const string Prefabs = "Assets/DiscoverTunisia/Art/Prefabs/";
        private const string ModelsFolder = "Assets/DiscoverTunisia/Art/Models/";
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        [TestCase("Avatar")]
        [TestCase("Marker")]
        [TestCase("Plinth")]
        public void Prefab_UsesTheBlenderModels(string prefab)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + prefab + ".prefab");
            var meshes = go.GetComponentsInChildren<MeshFilter>(true).Select(f => AssetDatabase.GetAssetPath(f.sharedMesh)).ToList();
            Assert.That(meshes, Is.Not.Empty, prefab + " has no mesh");
            Assert.That(meshes, Has.All.StartsWith(ModelsFolder), prefab + " should use models exported by Tools/Blender/models.py");
        }

        [Test]
        public void AvatarOutfit_TakesTheAccentColor()
        {
            var avatar = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "Avatar.prefab"));
            try
            {
                var accent = new Color(0.8f, 0.1f, 0.2f);
                avatar.GetComponent<OutfitTint>().Apply(accent);
                int outfit = 0, dark = 0;
                var block = new MaterialPropertyBlock();
                foreach (var r in avatar.GetComponentsInChildren<Renderer>())
                    for (int i = 0; i < r.sharedMaterials.Length; i++)
                    {
                        r.GetPropertyBlock(block, i);
                        if (r.sharedMaterials[i].name == "M_Outfit") { outfit++; AssertColor(block.GetColor(BaseColor), accent); }
                        if (r.sharedMaterials[i].name == "M_OutfitDark") { dark++; Assert.That(block.GetColor(BaseColor).r, Is.LessThan(accent.r)); }
                    }
                Assert.That(outfit, Is.GreaterThan(0), "No M_Outfit slot on the avatar");
                Assert.That(dark, Is.GreaterThan(0), "No M_OutfitDark slot on the avatar");
            }
            finally
            {
                Object.DestroyImmediate(avatar);
            }
        }

        private static void AssertColor(Color actual, Color expected)
        {
            const float tolerance = 1e-3f;
            Assert.That(actual.r, Is.EqualTo(expected.r).Within(tolerance), "red");
            Assert.That(actual.g, Is.EqualTo(expected.g).Within(tolerance), "green");
            Assert.That(actual.b, Is.EqualTo(expected.b).Within(tolerance), "blue");
        }
    }
}
