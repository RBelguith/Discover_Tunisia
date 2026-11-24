using System.Collections;
using System.Linq;
using DiscoverTunisia.App;
using DiscoverTunisia.Content;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace DiscoverTunisia.Tests
{
    /// <summary>Drives the real screens: loads them through the Navigator and presses their buttons like a visitor.</summary>
    public static class ScreenDriver
    {
        private const float LoadTimeout = 15f;
        private const float PickTimeout = 2f;

        public static IEnumerator Load(AppScreen screen)
        {
            var previous = SceneManager.GetActiveScene().handle;
            Navigator.Go(screen);
            yield return WaitFor(screen, previous);
        }

        /// <summary>Waits until the screen's scene replaces the scene that had the given handle, even when it reloads the same screen.</summary>
        private static IEnumerator WaitFor(AppScreen screen, SceneHandle previous)
        {
            var name = Navigator.SceneName(screen);
            var start = Time.realtimeSinceStartup;
            while (SceneManager.GetActiveScene().name != name || SceneManager.GetActiveScene().handle == previous)
            {
                if (Time.realtimeSinceStartup - start > LoadTimeout) Assert.Fail("Timed out waiting for " + name);
                yield return null;
            }
            // One frame for Start and the Navigator's load callback, one for the panel layout.
            yield return null;
            yield return null;
        }

        public static VisualElement Root() => Object.FindAnyObjectByType<UIDocument>().rootVisualElement;

        public static IEnumerator Press(string buttonName, AppScreen next)
        {
            var button = Root().Q<Button>(buttonName);
            Assert.That(button, Is.Not.Null, "No button named " + buttonName);
            yield return Press(button, next);
        }

        public static IEnumerator PressRow(string displayName, AppScreen next)
        {
            var row = Root().Q("governorateList").Query<Button>("row").ToList()
                .FirstOrDefault(r => r.Q<Label>("name").text == displayName);
            Assert.That(row, Is.Not.Null, "No governorate row for " + displayName);
            yield return Press(row, next);
        }

        public static IEnumerator Press(Button button, AppScreen next)
        {
            var previous = SceneManager.GetActiveScene().handle;
            yield return Tap(button);
            yield return WaitFor(next, previous);
        }

        /// <summary>Presses a button that keeps the visitor on the same screen.</summary>
        public static IEnumerator Tap(string buttonName)
        {
            var button = Root().Q<Button>(buttonName);
            Assert.That(button, Is.Not.Null, "No button named " + buttonName);
            yield return Tap(button);
            yield return null;
        }

        /// <summary>Presses a button through the panel once a tap at its center would reach it.</summary>
        private static IEnumerator Tap(Button button)
        {
            var start = Time.realtimeSinceStartup;
            VisualElement picked;
            while (!Reaches(button, picked = button.panel.Pick(button.worldBound.center)) && Time.realtimeSinceStartup - start < PickTimeout)
                yield return null;
            Assert.That(Reaches(button, picked), Is.True, button.name + " at " + button.worldBound + " is covered by " + Describe(picked));
            button.Focus();
            using (var e = NavigationSubmitEvent.GetPooled())
            {
                e.target = button;
                button.SendEvent(e);
            }
        }

        private static bool Reaches(Button button, VisualElement picked) => picked != null && (picked == button || button.Contains(picked));

        private static string Describe(VisualElement e) =>
            e == null ? "nothing" : e.GetType().Name + "#" + e.name + " ." + string.Join(" .", e.GetClasses()) + " in " + Describe(e.parent);

        public static ContentCatalog Catalog()
        {
#if UNITY_EDITOR
            var guids = UnityEditor.AssetDatabase.FindAssets("t:" + nameof(ContentCatalog));
            Assert.That(guids, Has.Length.EqualTo(1), "Exactly one ContentCatalog asset is expected.");
            return UnityEditor.AssetDatabase.LoadAssetAtPath<ContentCatalog>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));
#else
            Assert.Ignore("The content catalog is read through the AssetDatabase, so these tests run in the Editor only.");
            return null;
#endif
        }
    }
}
