using DiscoverTunisia.App;
using DiscoverTunisia.Content;
using DiscoverTunisia.Tracking;
using DiscoverTunisia.World;
using UnityEngine;
using UnityEngine.UIElements;

namespace DiscoverTunisia.Screens
{
    /// <summary>Puts the governorate's monuments on the printed target, or on a turntable when tracking is unavailable.</summary>
    public sealed class ARScreen : ScreenController
    {
        private const string TargetId = "cible";

        [SerializeField] private ImageTracker tracker;
        [SerializeField] private Texture2D targetImage;
        [SerializeField, Tooltip("Printed width of the AR target, in meters.")]
        private float targetWidth = 0.2f;
        [SerializeField, Range(0.3f, 1.2f), Tooltip("Model footprint as a share of the target width.")]
        private float footprintOnTarget = 0.85f;

        [Header("Preview")]
        [SerializeField] private GameObject previewRig;
        [SerializeField] private Transform previewSpot;
        [SerializeField] private float previewFootprint = 2.4f;
        [SerializeField] private float spinDegreesPerSecond = 22f;

        private GovernorateData _governorate;
        private int _index;
        private GameObject _model;
        private Bounds _bounds;
        private Transform _anchor;
        private TrackingQuality _quality;
        private bool _preview;
        private Label _status;
        private VisualElement _statusPill;

        protected override void Bind(VisualElement root)
        {
            _governorate = Governorate;
            OnClick("backButton", OnBack);
            OnClick("prevButton", () => Show(_index - 1));
            OnClick("nextButton", () => Show(_index + 1));
            root.Q<Label>("governorateName").text = _governorate.DisplayName;
            _status = root.Q<Label>("statusText");
            _statusPill = root.Q("statusPill");

            if (tracker == null) EnterPreview("Aperçu 3D : Vuforia Engine n'est pas installé dans ce projet.");
            else
            {
                tracker.StateChanged += OnTrackerState;
                tracker.TargetChanged += OnTarget;
                tracker.AddTarget(TargetId, targetImage, targetWidth);
                OnTrackerState(tracker.State);
            }
            Show(0);
        }

        private void OnDisable()
        {
            if (tracker == null) return;
            tracker.StateChanged -= OnTrackerState;
            tracker.TargetChanged -= OnTarget;
        }

        private void OnTrackerState(TrackerState state)
        {
            if (state == TrackerState.Failed) EnterPreview("Aperçu 3D : " + tracker.FailureMessage);
            else
            {
                if (_preview) ExitPreview();
                RefreshStatus();
            }
        }

        private void OnTarget(string id, Transform anchor, TrackingQuality quality)
        {
            if (id != TargetId || _preview) return;
            _anchor = anchor;
            _quality = quality;
            Place();
            RefreshStatus();
        }

        private void EnterPreview(string message)
        {
            _preview = true;
            if (previewRig != null) previewRig.SetActive(true);
            _status.text = message;
            _statusPill.AddToClassList("status-pill--preview");
            Place();
        }

        private void ExitPreview()
        {
            _preview = false;
            if (previewRig != null) previewRig.SetActive(false);
            _statusPill.RemoveFromClassList("status-pill--preview");
            Place();
        }

        private void Show(int index)
        {
            var monuments = _governorate.Monuments;
            if (monuments.Count == 0) return;
            _index = (index % monuments.Count + monuments.Count) % monuments.Count;
            var entry = monuments[_index];
            Root.Q<Label>("monumentName").text = entry.Title;
            Root.Q<Label>("monumentCount").text = (_index + 1) + " / " + monuments.Count;
            if (_model != null) Destroy(_model);
            _model = entry.Model != null ? Instantiate(entry.Model) : null;
            if (_model != null) _bounds = ModelFitter.Measure(_model);
            Place();
        }

        private void Place()
        {
            if (_model == null) return;
            if (_preview)
            {
                ModelFitter.Fit(_model.transform, _bounds, previewSpot, previewFootprint);
                _model.SetActive(true);
            }
            else if (_anchor != null)
            {
                ModelFitter.Fit(_model.transform, _bounds, _anchor, targetWidth * footprintOnTarget);
                _model.SetActive(_quality != TrackingQuality.None);
            }
            else _model.SetActive(false);
        }


        private void RefreshStatus()
        {
            if (_preview) return;
            _statusPill.EnableInClassList("status-pill--tracked", _quality != TrackingQuality.None);
            _status.text = tracker.State == TrackerState.Starting ? "Démarrage de la caméra..."
                : _quality == TrackingQuality.None ? "Pointez la caméra vers la cible imprimée"
                : "Monument posé sur la cible : tournez autour pour l'explorer";
        }

        protected override void Update()
        {
            base.Update();
            if (_preview && _model != null) _model.transform.Rotate(0f, spinDegreesPerSecond * Time.deltaTime, 0f, Space.Self);
        }

        protected override void OnBack() => Navigator.Go(AppScreen.Globe);
    }
}
