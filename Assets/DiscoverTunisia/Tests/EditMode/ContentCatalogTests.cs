using System.Collections.Generic;
using System.Linq;
using DiscoverTunisia.Content;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DiscoverTunisia.Tests
{
    public class ContentCatalogTests
    {
        private const string ModelsFolder = "Assets/DiscoverTunisia/Art/Models/";

        private static ContentCatalog LoadCatalog()
        {
            var guids = AssetDatabase.FindAssets("t:" + nameof(ContentCatalog));
            Assert.That(guids, Has.Length.EqualTo(1), "Exactly one ContentCatalog asset is expected.");
            return AssetDatabase.LoadAssetAtPath<ContentCatalog>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static IEnumerable<GovernorateData> Governorates() => LoadCatalog().Governorates.Where(g => g != null);

        [Test]
        public void Catalog_HasNoEmptySlots()
        {
            Assert.That(LoadCatalog().Governorates, Has.None.Null, "The ContentCatalog has an empty governorate slot");
        }

        [Test]
        public void Catalog_HasUniqueIds()
        {
            var ids = Governorates().Select(g => g.Id).ToList();
            Assert.That(ids, Is.Not.Empty);
            Assert.That(ids, Has.None.Null.And.None.Empty);
            Assert.That(ids, Is.Unique);
        }

        [Test]
        public void Catalog_GivesEveryMonumentItsOwnModel()
        {
            var models = Governorates().SelectMany(g => g.Monuments).Select(m => m.Model).ToList();
            Assert.That(models, Is.Unique);
        }

        [TestCaseSource(nameof(Governorates))]
        public void Governorate_MonumentsUseTheBlenderModels(GovernorateData g)
        {
            foreach (var m in g.Monuments)
            {
                var meshes = m.Model.GetComponentsInChildren<MeshFilter>(true).Select(f => AssetDatabase.GetAssetPath(f.sharedMesh)).ToList();
                Assert.That(meshes, Is.Not.Empty, m.Title + " has no mesh");
                Assert.That(meshes, Has.All.StartsWith(ModelsFolder).And.All.EndsWith(".fbx"), m.Title + " should use a model exported by Tools/Blender/monuments.py");
            }
        }

        [TestCaseSource(nameof(Governorates))]
        public void Governorate_HasTextForEveryScreen(GovernorateData g)
        {
            Assert.That(g.DisplayName, Is.Not.Empty);
            Assert.That(g.Subtitle, Is.Not.Empty);
            Assert.That(g.Description, Is.Not.Empty);
            Assert.That(g.ShortBlurb, Is.Not.Empty);
        }

        [TestCaseSource(nameof(Governorates))]
        public void Governorate_LiesInTunisia(GovernorateData g)
        {
            Assert.That(g.Latitude, Is.InRange(30f, 37.6f));
            Assert.That(g.Longitude, Is.InRange(7.5f, 11.7f));
        }

        [TestCaseSource(nameof(Governorates))]
        public void Governorate_ScanCardCanBeTracked(GovernorateData g)
        {
            Assert.That(g.ScanCard, Is.Not.Null, g.DisplayName + " has no scan card");
            Assert.That(g.ScanCard.isReadable, Is.True, "Vuforia copies the pixels, so the texture must be Read/Write enabled");
            Assert.That(Mathf.Max(g.ScanCard.width, g.ScanCard.height), Is.LessThanOrEqualTo(2048), "Vuforia rejects images over 2048 px");
        }

        [TestCaseSource(nameof(Governorates))]
        public void Governorate_SectionsAreComplete(GovernorateData g)
        {
            Assert.That(g.Monuments, Is.Not.Empty);
            Assert.That(g.Foods, Is.Not.Empty);
            Assert.That(g.Clothes, Is.Not.Empty);
            foreach (var m in g.Monuments)
            {
                Assert.That(m.Title, Is.Not.Empty);
                Assert.That(m.Image, Is.Not.Null, m.Title + " has no photo");
                Assert.That(m.Model, Is.Not.Null, m.Title + " has no 3D model");
            }
            foreach (var c in g.Foods.Concat(g.Clothes))
            {
                Assert.That(c.Title, Is.Not.Empty);
                Assert.That(c.Image, Is.Not.Null, c.Title + " has no photo");
            }
        }
    }
}
