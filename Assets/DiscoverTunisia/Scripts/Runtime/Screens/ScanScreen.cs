using DiscoverTunisia.App;
using DiscoverTunisia.Content;
using DiscoverTunisia.Tracking;
using UnityEngine;
using UnityEngine.UIElements;

namespace DiscoverTunisia.Screens
{
    /// <summary>Recognizes a printed region card with the camera and opens its governorate.</summary>
    public sealed class ScanScreen : ScreenController
    {
        [SerializeField] private ImageTracker tracker;
        [SerializeField, Tooltip("Printed width of the photo on a region card, in meters.")]
        private float cardWidth = 0.0856f;

        private Label _status;
        private VisualElement _reticle;
        private VisualElement _result;
        private VisualElement _failure;
        private GovernorateData _found;

        protected override void Bind(VisualElement root)
        {
            OnClick("backButton", OnBack);
            OnClick("failureBackButton", OnBack);
            OnClick("continueButton", Continue);
            OnClick("retryButton", Retry);
            _status = root.Q<Label>("statusText");
            _reticle = root.Q("reticle");
            _result = root.Q("result");
            _failure = root.Q("failure");

            if (tracker == null)
            {
                ShowFailure("Le scan a besoin de Vuforia Engine, qui n'est pas installé dans ce projet.");
                return;
            }
            tracker.StateChanged += OnTrackerState;
            tracker.TargetChanged += OnTarget;
            foreach (var g in catalog.Governorates)
                if (g.ScanCard != null) tracker.AddTarget(g.Id, g.ScanCard, cardWidth);
            OnTrackerState(tracker.State);
        }

        private void OnDisable()
        {
            if (tracker == null) return;
            tracker.StateChanged -= OnTrackerState;
            tracker.TargetChanged -= OnTarget;
        }

        private void OnTrackerState(TrackerState state)
        {
            switch (state)
            {
                case TrackerState.Starting: _status.text = "Démarrage de la caméra..."; break;
                case TrackerState.Running: HideFailure(); _status.text = "Placez une carte région dans le cadre"; break;
                case TrackerState.Failed: ShowFailure(tracker.FailureMessage); break;
            }
        }

        private void OnTarget(string id, Transform anchor, TrackingQuality quality)
        {
            if (quality != TrackingQuality.Tracked || _found != null) return;
            _found = catalog.Find(id);
            if (_found == null) return;

            _result.Q<Label>("resultName").text = _found.DisplayName;
            _result.Q<Label>("resultSubtitle").text = _found.Subtitle;
            _result.Q("resultPhoto").style.backgroundImage = new StyleBackground(_found.ScanCard);
            _result.Q("resultDot").style.backgroundColor = _found.AccentColor;
            _result.RemoveFromClassList("hidden");
            _reticle.AddToClassList("hidden");
        }

        private void Continue()
        {
            AppSession.Select(_found, SelectionSource.Scan);
            Navigator.Go(AppScreen.Globe);
        }

        private void Retry()
        {
            _found = null;
            _result.AddToClassList("hidden");
            _reticle.RemoveFromClassList("hidden");
        }

        private void ShowFailure(string message)
        {
            Retry();
            _failure.Q<Label>("failureText").text = message;
            _failure.RemoveFromClassList("hidden");
            _reticle.AddToClassList("hidden");
        }

        private void HideFailure()
        {
            _failure.AddToClassList("hidden");
            _reticle.EnableInClassList("hidden", _found != null);
        }

        protected override void OnBack() => Navigator.Go(AppScreen.Identification);
    }
}
