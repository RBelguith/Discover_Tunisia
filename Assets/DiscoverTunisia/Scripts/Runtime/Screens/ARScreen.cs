using DiscoverTunisia.App;
using DiscoverTunisia.Content;
using DiscoverTunisia.Tracking;
using DiscoverTunisia.World;
using UnityEngine;
using UnityEngine.UIElements;

namespace DiscoverTunisia.Screens
{
    /// <summary>Shows the governorate's monuments turning on a plinth, and on the printed target when the visitor turns the camera on.</summary>
    public sealed class ARScreen : ScreenController
    {
        private const string TargetId = "cible";
        private const string PreviewHint = "Glissez pour faire tourner le monument";

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
        [SerializeField, Tooltip("Degrees the model turns per pixel dragged.")]
        private float dragDegreesPerPixel = 0.35f;
        [SerializeField, Tooltip("Seconds after a drag before the model spins again.")]
        private float spinResumeDelay = 1.5f;

        private GovernorateData _governorate;
        private int _index;
        private GameObject _model;
        private Bounds _bounds;
        private Transform _anchor;
        private TrackingQuality _quality;
        private bool _preview = true;
        private bool _dragging;
        private float _spinPausedUntil;
        private VisualElement _screen;
        private Label _status;
        private VisualElement _statusPill;
        private Button _cameraButton;
        private Label _cameraLabel;

        protected override void Bind(VisualElement root)
        {
            _governorate = Governorate;
            OnClick("backButton", OnBack);
            OnClick("prevButton", () => Show(_index - 1));
            OnClick("nextButton", () => Show(_index + 1));
            _cameraButton = OnClick("cameraButton", ToggleCamera);
            _cameraLabel = root.Q<Label>("cameraLabel");
            root.Q<Label>("governorateName").text = _governorate.DisplayName;
            _screen = root.Q("screen");
            _status = root.Q<Label>("statusText");
            _statusPill = root.Q("statusPill");
            BindDrag(root.Q("stage"));

            if (tracker == null)
            {
                _cameraButton.SetEnabled(false);
                EnterPreview("Aperçu 3D : Vuforia Engine n'est pas installé dans ce projet.");
            }
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

        private void ToggleCamera()
        {
            if (tracker.State == TrackerState.Starting || tracker.State == TrackerState.Running) tracker.StopCamera();
            else tracker.StartCamera();
        }

        private void OnTrackerState(TrackerState state)
        {
            bool on = state == TrackerState.Starting || state == TrackerState.Running;
            _cameraLabel.text = on ? "Revenir à l'aperçu" : state == TrackerState.Failed ? "Réessayer la caméra" : "Voir sur la cible";
            switch (state)
            {
                case TrackerState.Off: EnterPreview(PreviewHint); break;
                case TrackerState.Starting: EnterPreview("Démarrage de la caméra..."); break;
                case TrackerState.Failed: EnterPreview("Caméra indisponible : " + tracker.FailureMessage); break;
                case TrackerState.Running: ExitPreview(); break;
            }
        }

        private void OnTarget(string id, Transform anchor, TrackingQuality quality)
        {
            if (id != TargetId) return;
            _anchor = anchor;
            _quality = quality;
            if (_preview) return;
            Place();
            RefreshStatus();
        }

        private void EnterPreview(string message)
        {
            _preview = true;
            if (previewRig != null) previewRig.SetActive(true);
            SetCameraLayout(false);
            _status.text = message;
            _statusPill.AddToClassList("status-pill--preview");
            _statusPill.RemoveFromClassList("status-pill--tracked");
            Place();
        }

        private void ExitPreview()
        {
            _preview = false;
            _dragging = false;
            if (previewRig != null) previewRig.SetActive(false);
            SetCameraLayout(true);
            _statusPill.RemoveFromClassList("status-pill--preview");
            Place();
            RefreshStatus();
        }

        private void SetCameraLayout(bool camera)
        {
            _screen.EnableInClassList("ar--camera", camera);
            _screen.EnableInClassList("ar--preview", !camera);
            _screen.Q(className: "top-bar").EnableInClassList("top-bar--camera", camera);
        }

        private void Show(int index)
        {
            var monuments = _governorate.Monuments;
            if (monuments.Count == 0) return;
            _index = (index % monuments.Count + monuments.Count) % monuments.Count;
            var entry = monuments[_index];
            Root.Q<Label>("monumentName").text = entry.Title;
            Root.Q<Label>("monumentCount").text = (_index + 1) + " / " + monuments.Count;
            Root.Q<Label>("monumentText").text = entry.Description;
            Root.Q<ScrollView>("monumentScroll").scrollOffset = Vector2.zero;
            Root.Q("monumentPhoto").style.backgroundImage = entry.Image != null ? new StyleBackground(entry.Image) : StyleKeyword.None;
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
            _status.text = _quality == TrackingQuality.None ? "Pointez la caméra vers la cible imprimée"
                : "Monument posé sur la cible : tournez autour pour l'explorer";
        }

        private void BindDrag(VisualElement stage)
        {
            stage.RegisterCallback<PointerDownEvent>(e =>
            {
                _dragging = true;
                stage.CapturePointer(e.pointerId);
            });
            stage.RegisterCallback<PointerMoveEvent>(e =>
            {
                if (!_dragging || _model == null || !_preview) return;
                _model.transform.Rotate(0f, -e.deltaPosition.x * dragDegreesPerPixel, 0f, Space.Self);
            });
            stage.RegisterCallback<PointerUpEvent>(e => EndDrag(stage, e.pointerId));
            stage.RegisterCallback<PointerCaptureOutEvent>(_ => EndDrag(stage, -1));
        }

        private void EndDrag(VisualElement stage, int pointerId)
        {
            if (!_dragging) return;
            _dragging = false;
            _spinPausedUntil = Time.time + spinResumeDelay;
            if (pointerId >= 0 && stage.HasPointerCapture(pointerId)) stage.ReleasePointer(pointerId);
        }

        protected override void Update()
        {
            base.Update();
            if (_preview && _model != null && !_dragging && Time.time >= _spinPausedUntil)
                _model.transform.Rotate(0f, spinDegreesPerSecond * Time.deltaTime, 0f, Space.Self);
        }

        protected override void OnBack() => Navigator.Go(AppScreen.Globe);
    }
}
