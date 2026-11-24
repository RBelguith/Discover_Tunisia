using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DiscoverTunisia.App;
using DiscoverTunisia.Screens;
using DiscoverTunisia.Tracking;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace DiscoverTunisia.Tests
{
    /// <summary>Shows every monument of the content on the AR screen's 3D preview and checks how it stands.</summary>
    public class MonumentPreviewTests
    {
        private const string MaterialsFolder = "Assets/DiscoverTunisia/Art/Materials/";

        public static IEnumerable<TestCaseData> Entries()
        {
            foreach (var g in ScreenDriver.Catalog().Governorates)
                for (int i = 0; i < g.Monuments.Count; i++)
                    yield return new TestCaseData(g.Id, i).Returns(null);
        }

        [SetUp]
        public void SetUp()
        {
            ImageTracker.CameraDisabled = true;
            AppSession.Select(null, SelectionSource.Manual);
        }

        [TearDown]
        public void TearDown() => ImageTracker.CameraDisabled = false;

        [UnityTest, TestCaseSource(nameof(Entries))]
        public IEnumerator Monument_StandsOnThePreviewSpot(string governorateId, int index)
        {
            yield return Show(governorateId, index);
            var (ar, model, entryTitle) = Shown(governorateId, index);
            var spot = Field<Transform>(ar, "previewSpot");
            var footprint = Field<float>(ar, "previewFootprint");

            Assert.That(ScreenDriver.Root().Q<Label>("monumentName").text, Is.EqualTo(entryTitle));
            Assert.That(model.activeInHierarchy, Is.True, "The model should show on the preview");
            Assert.That(BoundsIn(spot, model).min.y, Is.EqualTo(0f).Within(footprint * 0.005f), "The base should sit on the preview spot");
            var local = BoundsIn(model.transform, model);
            Assert.That(Mathf.Max(local.size.x, local.size.z) * model.transform.lossyScale.x, Is.EqualTo(footprint).Within(footprint * 0.01f), "The footprint should match the preview size");
            foreach (var r in model.GetComponentsInChildren<Renderer>())
                foreach (var m in r.sharedMaterials)
                {
                    Assert.That(m, Is.Not.Null, r.name + " has an empty material slot");
#if UNITY_EDITOR
                    Assert.That(UnityEditor.AssetDatabase.GetAssetPath(m), Does.StartWith(MaterialsFolder), r.name + " uses " + m.name + " from outside the palette");
#endif
                }
        }

        [UnityTest, TestCaseSource(nameof(Entries))]
        public IEnumerator Monument_SpinsInPlace(string governorateId, int index)
        {
            yield return Show(governorateId, index);
            var (_, model, _) = Shown(governorateId, index);
            var local = BoundsIn(model.transform, model);
            var size = Mathf.Max(local.size.x, local.size.z);
            var offset = new Vector2(local.center.x, local.center.z).magnitude;
            Assert.That(offset, Is.LessThanOrEqualTo(size * 0.02f), "The footprint should be centered on the pivot, or the turntable makes it orbit");
        }

        private static IEnumerator Show(string governorateId, int index)
        {
            AppSession.Select(ScreenDriver.Catalog().Find(governorateId), SelectionSource.Manual);
            yield return ScreenDriver.Load(AppScreen.AR);
            for (int i = 0; i < index; i++) yield return ScreenDriver.Tap("nextButton");
        }

        private static (ARScreen ar, GameObject model, string title) Shown(string governorateId, int index)
        {
            var entry = ScreenDriver.Catalog().Find(governorateId).Monuments[index];
            var model = GameObject.Find(entry.Model.name + "(Clone)");
            Assert.That(model, Is.Not.Null, entry.Title + " has no instance on the AR screen");
            return (Object.FindAnyObjectByType<ARScreen>(), model, entry.Title);
        }

        private static T Field<T>(object owner, string name) =>
            (T)owner.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(owner);

        /// <summary>Axis-aligned bounds of the model's meshes, measured in the given transform's space.</summary>
        private static Bounds BoundsIn(Transform space, GameObject model)
        {
            var bounds = new Bounds();
            bool first = true;
            foreach (var filter in model.GetComponentsInChildren<MeshFilter>())
            {
                var toSpace = space.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                var mesh = filter.sharedMesh.bounds;
                for (int i = 0; i < 8; i++)
                {
                    var corner = mesh.center + Vector3.Scale(mesh.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    var p = toSpace.MultiplyPoint3x4(corner);
                    if (first) { bounds = new Bounds(p, Vector3.zero); first = false; }
                    else bounds.Encapsulate(p);
                }
            }
            return bounds;
        }
    }
}
