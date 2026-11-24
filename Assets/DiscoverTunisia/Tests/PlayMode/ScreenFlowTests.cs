using System.Collections;
using DiscoverTunisia.App;
using DiscoverTunisia.Tracking;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace DiscoverTunisia.Tests
{
    public class ScreenFlowTests
    {
        [SetUp]
        public void SetUp()
        {
            ImageTracker.CameraDisabled = true;
            AppSession.Select(null, SelectionSource.Manual);
        }

        [TearDown]
        public void TearDown() => ImageTracker.CameraDisabled = false;

        [UnityTest]
        public IEnumerator Home_LeadsToTheARPreviewOfTheChosenGovernorate()
        {
            yield return ScreenDriver.Load(AppScreen.Home);
            yield return ScreenDriver.Press("startButton", AppScreen.Identification);
            yield return ScreenDriver.PressRow("Bizerte", AppScreen.Globe);
            Assert.That(AppSession.Selected.Id, Is.EqualTo("bizerte"));

            yield return ScreenDriver.Press("arButton", AppScreen.AR);
            var root = ScreenDriver.Root();
            Assert.That(root.Q<Label>("governorateName").text, Is.EqualTo("Bizerte"));
            Assert.That(root.Q("statusPill").ClassListContains("status-pill--preview"), Is.True, "AR should fall back to the 3D preview");
            Assert.That(Object.FindAnyObjectByType<ImageTracker>().State, Is.EqualTo(TrackerState.Failed));
        }

        [UnityTest]
        public IEnumerator Scan_ShowsTheFailureSheetWhenTheCameraIsOff()
        {
            yield return ScreenDriver.Load(AppScreen.Scan);
            var root = ScreenDriver.Root();
            Assert.That(Object.FindAnyObjectByType<ImageTracker>().State, Is.EqualTo(TrackerState.Failed));
            Assert.That(root.Q("failure").ClassListContains("hidden"), Is.False, "The failure sheet should show");
            Assert.That(root.Q("reticle").ClassListContains("hidden"), Is.True, "The scan frame should hide");
        }
    }
}
