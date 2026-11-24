using System.Collections;
using DiscoverTunisia.App;
using DiscoverTunisia.Tracking;
using DiscoverTunisia.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DiscoverTunisia.Tests
{
    public class GlobeMarkerTests
    {
        [SetUp]
        public void SetUp() => ImageTracker.CameraDisabled = true;

        [TearDown]
        public void TearDown()
        {
            ImageTracker.CameraDisabled = false;
            AppSession.Select(null, SelectionSource.Manual);
        }

        [UnityTest]
        public IEnumerator Globe_PinsOnlyTheChosenGovernorate()
        {
            AppSession.Select(ScreenDriver.Catalog().Find("bizerte"), SelectionSource.Manual);
            yield return ScreenDriver.Load(AppScreen.Globe);
            var globe = Object.FindAnyObjectByType<GlobeController>();
            Assert.That(globe.Markers, Has.Count.EqualTo(ScreenDriver.Catalog().Governorates.Count));
            foreach (var marker in globe.Markers)
            {
                var anchor = marker.View.Anchor;
                if (marker.Governorate.Id == "bizerte")
                {
                    Assert.That(anchor, Is.Not.SameAs(marker.View.transform), "The chosen governorate's label should sit on its pin");
                    Assert.That(anchor.gameObject.activeInHierarchy, Is.True, "The chosen governorate's pin should show");
                }
                else Assert.That(anchor, Is.SameAs(marker.View.transform), marker.Governorate.Id + " should have no pin");
            }
        }
    }
}
