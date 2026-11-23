using UnityEngine;
using DiscoverTunisia.Core;
using DiscoverTunisia.Globe;
using DiscoverTunisia.UI;

namespace DiscoverTunisia.Screens
{
    /// <summary>
    /// Scène principale : globe 3D (F03/F04), avatar (F05), rubriques
    /// Monuments/Gastronomie/Vêtements (F06-F08) et navigation (F10).
    /// </summary>
    public class GlobeBootstrap : SceneBootstrap
    {
        private GlobeWorld _world;
        private GlobeUI _ui;

        protected override void Awake()
        {
            base.Awake();

            var gov = AppState.Selected;
            if (gov == null)
            {
                gov = ContentDatabase.All[0];
                AppState.Select(gov.id, "manuel");
            }

            _world = new GameObject("World").AddComponent<GlobeWorld>();
            _world.Build(gov, Canvas);

            _ui = new GlobeUI();
            _ui.Build(Canvas, _world, gov);
        }
    }
}
