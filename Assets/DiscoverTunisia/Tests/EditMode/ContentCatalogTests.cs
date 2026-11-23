using System.Collections.Generic;
using System.Linq;
using DiscoverTunisia.Content;
using NUnit.Framework;
using UnityEditor;

namespace DiscoverTunisia.Tests
{
    public class ContentCatalogTests
    {
        private static ContentCatalog LoadCatalog()
        {
            var guids = AssetDatabase.FindAssets("t:" + nameof(ContentCatalog));
            Assert.That(guids, Has.Length.EqualTo(1), "Exactly one ContentCatalog asset is expected.");
            return AssetDatabase.LoadAssetAtPath<ContentCatalog>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static IEnumerable<GovernorateData> Governorates() => LoadCatalog().Governorates;

        [Test]
        public void Catalog_HasUniqueIds()
        {
            var ids = Governorates().Select(g => g.Id).ToList();
            Assert.That(ids, Is.Not.Empty);
            Assert.That(ids, Has.None.Null.And.None.Empty);
            Assert.That(ids, Is.Unique);
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
        public void Governorate_SectionsAreComplete(GovernorateData g)
        {
            Assert.That(g.Monuments, Is.Not.Empty);
            Assert.That(g.Foods, Is.Not.Empty);
            Assert.That(g.Clothes, Is.Not.Empty);
            foreach (var m in g.Monuments)
            {
                Assert.That(m.Title, Is.Not.Empty);
                Assert.That(m.Image, Is.Not.Null, m.Title + " has no photo");
            }
            foreach (var c in g.Foods.Concat(g.Clothes))
            {
                Assert.That(c.Title, Is.Not.Empty);
                Assert.That(c.Image, Is.Not.Null, c.Title + " has no photo");
            }
        }
    }
}
