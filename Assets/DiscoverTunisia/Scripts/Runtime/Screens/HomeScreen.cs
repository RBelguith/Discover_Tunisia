using DiscoverTunisia.App;
using DiscoverTunisia.World;
using UnityEngine;
using UnityEngine.UIElements;

namespace DiscoverTunisia.Screens
{
    /// <summary>Welcome screen: the app's pitch over a slowly turning globe.</summary>
    public sealed class HomeScreen : ScreenController
    {
        [SerializeField] private GlobeController globe;

        protected override void Bind(VisualElement root)
        {
            OnClick("startButton", () => Navigator.Go(AppScreen.Identification));
            root.Q<Label>("regionList").text = string.Join("  ·  ", Names());
            if (globe != null)
            {
                globe.ShowMarkers(new[] { catalog.Default }, catalog.Default);
                globe.FocusOn(catalog.Default, Camera.main);
            }
        }

        private string[] Names()
        {
            var names = new string[catalog.Governorates.Count];
            for (int i = 0; i < names.Length; i++) names[i] = catalog.Governorates[i].DisplayName;
            return names;
        }

        protected override void OnBack() => Application.Quit();
    }
}
