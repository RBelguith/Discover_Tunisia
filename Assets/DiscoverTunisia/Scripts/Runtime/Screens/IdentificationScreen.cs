using DiscoverTunisia.App;
using DiscoverTunisia.Content;
using UnityEngine;
using UnityEngine.UIElements;

namespace DiscoverTunisia.Screens
{
    /// <summary>Picks the governorate: by scanning a printed region card, or from the list.</summary>
    public sealed class IdentificationScreen : ScreenController
    {
        [SerializeField] private VisualTreeAsset governorateRow;

        protected override void Bind(VisualElement root)
        {
            OnClick("backButton", OnBack);
            var scan = OnClick("scanButton", () => Navigator.Go(AppScreen.Scan));
#if DT_VUFORIA
            root.Q("scanUnavailable").AddToClassList("hidden");
#else
            scan.SetEnabled(false);
#endif
            var list = root.Q("governorateList");
            list.Clear();
            foreach (var g in catalog.Governorates) list.Add(Row(g));
        }

        private VisualElement Row(GovernorateData g)
        {
            var row = governorateRow.Instantiate().Q<Button>("row");
            row.Q<Label>("name").text = g.DisplayName;
            row.Q<Label>("subtitle").text = g.Subtitle;
            row.Q("dot").style.backgroundColor = g.AccentColor;
            if (g.Monuments.Count > 0) row.Q("thumb").style.backgroundImage = new StyleBackground(g.Monuments[0].Image);
            row.clicked += () =>
            {
                AppSession.Select(g, SelectionSource.Manual);
                Navigator.Go(AppScreen.Globe);
            };
            return row;
        }

        protected override void OnBack() => Navigator.Go(AppScreen.Home);
    }
}
